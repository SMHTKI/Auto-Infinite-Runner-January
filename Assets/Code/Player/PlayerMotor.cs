using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Events;


public class PlayerMotor : MonoBehaviour
{
    #region Variables
    [Header("Cached Components")]
    [SerializeField] PlayerController playerController;
    [SerializeField] private Transform _mainTransform;

    [Header("Speed Vars")]
    [SerializeField] private float _startSpeed;
    [SerializeField] private float _maxSpeed;
    public float CurrentSpeed => _currentSpeed;
    private float _currentSpeed;
    private float additionalSpeed;
    private Vector3 _velocity;

    // Spline Vars
    public SplineContainer CurrentSpline
    {
        get =>_container;
        set
        {
            _container = value;
            NormalizedTime = 0;
            RebuildSplinePath();
        }
    }
    [SerializeField] private SplineContainer StartContainer;
    [SerializeField] private SplineContainer _container;
    private SplinePath<Spline> m_SplinePath;

    // Spline Movement
    private float _elapsedTime;
    private float _currentSplineLength = -1;
    //The period of time that it takes for the GameObject to complete its animation along the spline.
    private float _splineDuration = 1f;
    //The period of time that it takes for any additional Speed added to the base speed to normalize back to current.
    private float _additionalSpeedDecayTimer;
    /// <summary>
    /// Normalized time of the Spline's traversal. The integer part is the number of times the Spline has been traversed.
    /// The fractional part is the % (0-1) of progress in the current loop.
    /// </summary>
    public float NormalizedTime
    {
        get => _normalizedTime;
        set
        {
            _normalizedTime = value;
            _elapsedTime = _splineDuration * _normalizedTime;
        }
    }
    private float _normalizedTime;
    #endregion

    #region Unity Messages
    void Start()
    {
        _currentSpeed = _startSpeed;
        _container = StartContainer;

        if (GameEventsManager.Instance)
        { 
            GameEventsManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged += UpdateSpeed;
            GameEventsManager.Instance.OnRoomChanged += OnRoomChanged;

        }

    }

    void OnDestroy()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged -= UpdateSpeed;
            GameEventsManager.Instance.OnRoomChanged -= OnRoomChanged;
        }
    }
  
    void Update()
    {
        if(playerController.IsAlive)
        {
            // Handle Additional Speed from Slopes
            if (additionalSpeed > 0 && CourseManager.Instance.CurrentRoomType != RoomType.down)
            {
                _additionalSpeedDecayTimer += Time.fixedDeltaTime / 3;
                additionalSpeed = Mathf.Lerp(additionalSpeed, 0, _additionalSpeedDecayTimer);
                if (_additionalSpeedDecayTimer >= 1)
                {
                    _additionalSpeedDecayTimer = 0;
                }
            }

            // Move Forward
            if (_container)
            {
                Move();
            }
        }
    }
    #endregion
    #region Movement Calculations

    private void UpdateSpeed(float difficulty)
    {
        _currentSpeed = Mathf.Lerp(_startSpeed, _maxSpeed, difficulty);
    }

    private void Move()
    {
        Vector3 position;
        Quaternion rotation;
        EvaluatePositionAndRotation(out position, out rotation);
        _mainTransform.position = position;
        _mainTransform.rotation = rotation;
    }

    void EvaluatePositionAndRotation(out Vector3 position, out Quaternion rotation)
    {
        CalculateDuration();
        CalculateNormalizedTime(Time.deltaTime);
        float t = GetLoopInterpolation();
        position = _container.EvaluatePosition(m_SplinePath, t);
     
        #region Rotation
        // Correct forward and up vectors based on axis remapping parameters
        var forward = Vector3.Normalize(_container.EvaluateTangent(m_SplinePath, t));
        var up = _container.EvaluateUpVector(m_SplinePath, t);
        rotation = Quaternion.LookRotation(forward, up);
        #endregion

        if (t >= 1f)
        {
            if (GameEventsManager.Instance)
            {
                GameEventsManager.Instance.CompleteRoom();
            }
        }
    }

    private float GetLoopInterpolation()
    {
        var t = 0f;
        if (Mathf.Floor(NormalizedTime) == NormalizedTime)
            t = Mathf.Clamp01(NormalizedTime);
        else
            t = NormalizedTime % 1f;

        return t;
    }


    private void CalculateNormalizedTime(float deltaTime)
    {
        // Increase Elapsed time
        _elapsedTime += deltaTime;
        var t = Mathf.Min(_elapsedTime, _splineDuration) / _splineDuration;

        // forcing reset to 0 if the m_NormalizedTime reach the end of the spline previously (1).
        _normalizedTime = t == 0 ? 0f : Mathf.Floor(_normalizedTime) + t;
    }

    private void CalculateDuration()
    {
        if (_currentSplineLength < 0f)
            RebuildSplinePath();

        if (_currentSplineLength >= 0f)
        {
            _splineDuration = _currentSplineLength / (_currentSpeed + additionalSpeed);
        }
    }
    private void RebuildSplinePath()
    {
        if (_container != null)
        {
            m_SplinePath = new SplinePath<Spline>(_container.Splines);
            _currentSplineLength = m_SplinePath != null ? m_SplinePath.GetLength() : 0f;
        }
    }
    #endregion
    #region Public Functions
    public void AddSpeed(float speedToAdd)
    {
        additionalSpeed = speedToAdd;
    }
    #endregion
    #region Events
    private void OnGameStateChanged(GameState _newState)
    {
        enabled = (_newState == GameState.GAMEPLAY || _newState == GameState.DEATH || _newState == GameState.RESPAWN);
    }

    private void OnRoomChanged(RoomController newRoom)
    {
        CurrentSpline = newRoom.followSpline;
    }
    #endregion
}
