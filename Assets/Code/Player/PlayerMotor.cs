using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Events;


public class PlayerMotor : MonoBehaviour
{
 

    [Header("Cached Components")]
    [SerializeField] PlayerController playerController;
    [SerializeField] private Transform _mainTransform;

    [Header("Speed Components")]
    [SerializeField] private float _startSpeed;
    [SerializeField] private float _maxSpeed;
    public float CurrentSpeed => _currentSpeed;
    private float _currentSpeed;
    private float additionalSpeed;
    private Vector3 _velocity;
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
    public SplineContainer StartContainer;

    SplineContainer _container;
    SplinePath<Spline> m_SplinePath;
    float m_ElapsedTime;
    float m_SplineLength = -1;
    [Tooltip("The period of time that it takes for the GameObject to complete its animation along the spline.")]
    float m_Duration = 1f;
    public float m_NormalizedTime;
    float normalizeTimer;
    /// <summary>
    /// Normalized time of the Spline's traversal. The integer part is the number of times the Spline has been traversed.
    /// The fractional part is the % (0-1) of progress in the current loop.
    /// </summary>
    public float NormalizedTime
    {
        get => m_NormalizedTime;
        set
        {
            m_NormalizedTime = value;
            m_ElapsedTime = m_Duration * m_NormalizedTime;
            //UpdateTransform();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        _currentSpeed = _startSpeed;
        _container = StartContainer;

        if (GameEventsManager.Instance)
        { 
            GameEventsManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged += UpdateSpeed;
        }

    }

    void OnDestroy()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged -= UpdateSpeed;
        }
    }

    private void UpdateSpeed(float difficulty)
    {
        _currentSpeed = Mathf.Lerp(_startSpeed, _maxSpeed, difficulty);
    }
    // Update is called once per frame
    void Update()
    {
        if(playerController.IsAlive)
        {
            if (additionalSpeed > 0 && CourseManager.Instance.CurrentRoomType != RoomType.down)
            {
                normalizeTimer += Time.fixedDeltaTime / 3;
                additionalSpeed = Mathf.Lerp(additionalSpeed, 0, normalizeTimer);
                if (normalizeTimer >= 1)
                {
                    normalizeTimer = 0;
                }
            }

            _velocity = Vector3.forward * (_currentSpeed + additionalSpeed) * Time.deltaTime;
            if (_container)
            {
                Move(_velocity);
            }
        }
    }

    public void Move(Vector3 _forwardVelocity)
    {
        _mainTransform.Translate(_forwardVelocity, Space.World);
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
        float t = GetLoopInterpolation(false);
        position = _container.EvaluatePosition(m_SplinePath, t);
        rotation = Quaternion.identity;
     
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

    internal float GetLoopInterpolation(bool offset)
    {
        var t = 0f;
        if (Mathf.Floor(NormalizedTime) == NormalizedTime)
            t = Mathf.Clamp01(NormalizedTime);
        else
            t = NormalizedTime % 1f;

        return t;
    }


    void CalculateNormalizedTime(float deltaTime)
    {
        m_ElapsedTime += deltaTime;
        var currentDuration = m_Duration;
        var t = 0f;
        t = Mathf.Min(m_ElapsedTime, currentDuration);
        t /= currentDuration;
        // forcing reset to 0 if the m_NormalizedTime reach the end of the spline previously (1).
        m_NormalizedTime = t == 0 ? 0f : Mathf.Floor(m_NormalizedTime) + t;
    }

    void CalculateDuration()
    {
        if (m_SplineLength < 0f)
            RebuildSplinePath();
       

        if (m_SplineLength >= 0f)
        {
            m_Duration = m_SplineLength / (_currentSpeed + additionalSpeed);
        }
    }
    void RebuildSplinePath()
    {
        if (_container != null)
        {
            m_SplinePath = new SplinePath<Spline>(_container.Splines);
            m_SplineLength = m_SplinePath != null ? m_SplinePath.GetLength() : 0f;
        }
    }
    private void OnGameStateChanged(GameState _newState)
    {
        enabled = (_newState == GameState.GAMEPLAY || _newState == GameState.DEATH || _newState == GameState.RESPAWN);
    }

    public void AddSpeed(float speedToAdd)
    {
        additionalSpeed = speedToAdd;
    }

}
