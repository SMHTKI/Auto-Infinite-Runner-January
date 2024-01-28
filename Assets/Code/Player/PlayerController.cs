using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Variables
    [Header("Cached Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _mesh;
    [SerializeField] private BoxCollider _playerCollider;
    [SerializeField] private PlayerMotor _playerMotor;

    protected AnimationEventHandler animationEventHandler;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpLandSplash;
    [SerializeField] private AudioClip deathSplash;
    [SerializeField] private AudioClip NiceTrick;


    [Header("Input Action Strings")]
    [SerializeField] private string _jumpActionString;
    [SerializeField] private string _pauseActionString;
    [SerializeField] private string _trickOneActionString;
    [SerializeField] private string _trickTwoActionString;
    [SerializeField] private string _turnActionString;
    [SerializeField] private PlayerInput input;


    [Header("Health Settings")]
    [SerializeField] private int _maxStartingLives;
    [SerializeField] private float _invincibilityDurationSeconds;
    [SerializeField] private float _continuePressCooldownTime;
    private float respawnCooldownTimer;
    public bool IsAlive => _liveCount > 0;
    public int CurrentLives => _liveCount;
    private int _liveCount;

    public bool IsInvincible => _isInvincible;
    private bool _isInvincible;

    [Header("Movement Settings")]
    [SerializeField] private float _maxTurnTime;
    [SerializeField] private float _minTurnTime;
    [SerializeField] private float _turnCoyoteTime;
    private float currentMaxTurnTime;
    private float warningTimeOne;
    private float warningTimeTwo;
    private bool _isJumping = false;
    public bool IsJumping => _isJumping;
    public bool IsTurning => _startTurning;
    private bool _startTurning = false;
    private float turnCoyoteTimer;
    private float turnTimer;

    // Events
    public UnityEvent<int> OnLostLife;
    public UnityEvent OnGameOver;
    public UnityEvent OnRespawned;

    // Audio & Animation Vars
    private float animatorSpeed;
    public TurnInfo StartTurnRoom;
    private float yPreturnRotation;

    // Trick Variables
    private bool _isTricking = false;

    #endregion

    #region Unity Messages
    private void OnEnable()
    {
        input.actions.Enable();
        input.actions.RemoveAllBindingOverrides();

        // Handles Jumping
        input.actions[_jumpActionString].started += OnJumpInputDown;
        input.actions[_jumpActionString].canceled +=  OnJumpInputUp;

        // Handles Turning
        input.actions[_turnActionString].started += OnTurnInputDown;
        input.actions[_turnActionString].canceled += OnTurnInputUp;

        // Handles Pausing
        input.actions[_pauseActionString].started += OnPauseInputDown;
        input.actions[_pauseActionString].canceled += OnPauseInputUp;

        // Handles Trick One
        input.actions[_trickOneActionString].started += OnTrickOneInputDown;
        input.actions[_trickOneActionString].canceled += OnTrickOneInputUp;

        // Handles Trick Two
        input.actions[_trickTwoActionString].started += OnTrickTwoInputDown;
        input.actions[_trickTwoActionString].canceled += OnTrickTwoInputUp;

        if (CourseManager.Instance)
            CourseManager.Instance.OnRoomChanged.AddListener(OnRoomChanged);

        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged += UpdateTurnHandling;
        }

        if (_animator)
        {
            animationEventHandler = _animator.GetComponent<AnimationEventHandler>();

            if (animationEventHandler)
            {
                animationEventHandler.OnJumpLanded += OnJumpLanded;
                animationEventHandler.OnJumpStarted += OnJumpStarted;
                animationEventHandler.OnDeathSplash += PlayDeathEffects;
                animationEventHandler.OnFinish += OnAnimationFinished;
                animationEventHandler.OnFinishedRespawning += OnFinishedRespawning;
            }
        }

    }
    void Start()
    {
        _liveCount = _maxStartingLives;

        if (_animator == null)
        {
            _animator = gameObject.GetComponentInChildren<Animator>();
        }

        if (_mesh == null)
        {
            _mesh = gameObject.GetComponentInChildren<SpriteRenderer>();
        }

        if (_playerCollider == null)
        {
            _playerCollider = gameObject.GetComponentInChildren<BoxCollider>();
        }

        if (_playerMotor == null)
        {
            _playerMotor = GetComponent<PlayerMotor>();
        }

        if (_animator)
            animationEventHandler = _animator.GetComponent<AnimationEventHandler>();

        if (_animator)
            animatorSpeed = _animator.speed;

        if (CourseManager.Instance)
            CourseManager.Instance.OnRoomChanged.AddListener(OnRoomChanged);

        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged += UpdateTurnHandling;
        }
    }
   
    void FixedUpdate()
    {
        if (GameEventsManager.Instance.CurrentGameState == GameState.DEATH)
        {
            respawnCooldownTimer -= Time.deltaTime;
        }

        if (_isInvincible)
            return;

        RoomType CurrentRoomType = CourseManager.Instance.CurrentRoomType;

        if (CurrentRoomType == RoomType.down)
        {
            _playerMotor.AddSpeed(100);
        }

        HandleTurning(CurrentRoomType);
        CheckAliveState(CurrentRoomType);
    }

    private void OnDisable()
    {
        // Handles Jumping
        input.actions[_jumpActionString].started -= OnJumpInputDown;
        input.actions[_jumpActionString].canceled -= OnJumpInputUp;

        // Handles Turning
        input.actions[_turnActionString].started -= OnTurnInputDown;
        input.actions[_turnActionString].canceled -= OnTurnInputUp;

        // Handles Pausing
        input.actions[_pauseActionString].started -= OnPauseInputDown;
        input.actions[_pauseActionString].canceled -= OnPauseInputUp;

        // Handles Trick One
        input.actions[_trickOneActionString].started -= OnTrickOneInputDown;
        input.actions[_trickOneActionString].canceled -= OnTrickOneInputUp;

        // Handles Trick Two
        input.actions[_trickTwoActionString].started -= OnTrickTwoInputDown;
        input.actions[_trickTwoActionString].canceled -= OnTrickTwoInputUp;

        input.actions.Disable();

        animationEventHandler.OnJumpLanded -= OnJumpLanded;
        animationEventHandler.OnDeathSplash -= PlayDeathEffects;
        animationEventHandler.OnFinish -= OnAnimationFinished;

        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged -= UpdateTurnHandling;
        }
    }

    private void OnDestroy()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEventsManager.Instance.OnDifficultyChanged -= UpdateTurnHandling;
        }
    
        // Handles Jumping
        input.actions[_jumpActionString].performed -= OnJumpInputDown;
        input.actions[_jumpActionString].canceled -= OnJumpInputUp;
    
        // Handles Turning
        input.actions[_turnActionString].performed -= OnTurnInputDown;
        input.actions[_turnActionString].canceled -= OnTurnInputUp;
    
        // Handles Pausing
        input.actions[_pauseActionString].performed -= OnPauseInputDown;
        input.actions[_pauseActionString].canceled -= OnPauseInputUp;
    
        // Handles Trick One
        input.actions[_trickOneActionString].performed -= OnTrickOneInputDown;
        input.actions[_trickOneActionString].canceled -= OnTrickOneInputUp;

        // Handles Trick Two
        input.actions[_trickTwoActionString].started -= OnTrickTwoInputDown;
        input.actions[_trickTwoActionString].canceled -= OnTrickTwoInputUp;

        animationEventHandler.OnJumpLanded -= OnJumpLanded;
        animationEventHandler.OnDeathSplash -= PlayDeathEffects;
        animationEventHandler.OnFinish -= OnAnimationFinished;
        input.actions.RemoveAllBindingOverrides();
    }
    #endregion
    #region Input Events

    private void OnTurnInputDown(InputAction.CallbackContext context)
    {
        if (GameEventsManager.Instance == null || GameEventsManager.Instance.CurrentGameState != GameState.GAMEPLAY)
            return;
 
        if (_isJumping || _startTurning || _isTricking)
        {
            return;
        }

        _startTurning = true;

        if (_animator)
            _animator.ResetTrigger("Reset Turn");

        Vector2 MovementInput = input.actions["Movement"].ReadValue<Vector2>();
        bool isTurningRight = (MovementInput.x > 0);

        if (_animator)
        {
            if (isTurningRight)
            {
                _animator.SetTrigger("Turn Right");
            }
            else
            {
                _animator.SetTrigger("Turn Left");
            }
        }
       

        // Set the Information of the room the player is currently trying to turn in.
        TurnInfo turnInfo = new TurnInfo();
        turnInfo.TurningRoom = CourseManager.Instance.CurrentRoom;
        turnInfo.completionPercentage = _playerMotor.NormalizedTime;
        Debug.Log("Current Room: " + turnInfo.TurningRoom.name + " Percentage: " + turnInfo.completionPercentage);
        StartTurnRoom = turnInfo;
    }
    private void OnTurnInputUp(InputAction.CallbackContext context)
    {
        _startTurning = false;
        turnTimer = 0;

        if (_animator)
        {
            _animator.SetTrigger("Reset Turn");
            _animator.ResetTrigger("Fall");
            _animator.ResetTrigger("Warning 2");
            _animator.ResetTrigger("Warning 1");
            _animator.ResetTrigger("Respawn");
        }
        }
    private void OnJumpInputDown(InputAction.CallbackContext context)
    {
         if (GameEventsManager.Instance == null || GameEventsManager.Instance.CurrentGameState == GameState.GAMEOVER || GameEventsManager.Instance.CurrentGameState == GameState.PAUSE)
            return;

        if (GameEventsManager.Instance.CurrentGameState == GameState.DEATH && respawnCooldownTimer <= 0)
        {
            if(_mesh)
            {
                _mesh.gameObject.SetActive(true);
                StartCoroutine(GiveIFrames());
            }

            OnRespawned?.Invoke();

            if (_animator)
            {
                _animator.SetTrigger("Respawn");
            }

            Debug.Log("Player is no longer invincible!");
            GameEventsManager.Instance.SetState(GameState.RESPAWN);
        }
        else if(GameEventsManager.Instance.CurrentGameState == GameState.GAMEPLAY && !_isJumping && !_isTricking)
        {
            if(_animator)
            {
                _animator.SetTrigger("Jump");
            }
        }
    }

    
    private IEnumerator GiveIFrames()
    {
        Color color = Color.white;

        if (_mesh)
        {
            color = _mesh.color;
            color.a = .5f;
            _mesh.color = color;
        }
       
        yield return new WaitForSeconds(_invincibilityDurationSeconds);
        color.a = 1f;
        if (_mesh)
            _mesh.color = color;

        _isInvincible = false;
    }
    private void OnJumpInputUp(InputAction.CallbackContext context)
    {
        if (_animator) 
        {
            _animator.ResetTrigger("Jump");
            _animator.ResetTrigger("Respawn");
            _animator.ResetTrigger("Reset Turn");
        }
    }
    private void OnPauseInputDown(InputAction.CallbackContext context)
    {
        if (GameEventsManager.Instance)
        {
            switch (GameEventsManager.Instance.CurrentGameState)
            {
                case GameState.GAMEPLAY:
                    GameEventsManager.Instance.SetState(GameState.PAUSE);
                    break;
                case GameState.PAUSE:
                    GameEventsManager.Instance.SetState(GameState.GAMEPLAY);
                    break;
                case GameState.GAMEOVER:
                    break;
                case GameState.DEATH:
                    break;
                case GameState.RESPAWN:
                    break;
                default:
                    break;
            }
        }
    }
    private void OnPauseInputUp(InputAction.CallbackContext context)
    {
    }
    private void OnTrickOneInputDown(InputAction.CallbackContext context)
    {

        if (GameEventsManager.Instance == null || GameEventsManager.Instance.CurrentGameState == GameState.GAMEOVER || GameEventsManager.Instance.CurrentGameState == GameState.PAUSE || GameEventsManager.Instance.CurrentGameState == GameState.DEATH || GameEventsManager.Instance.CurrentGameState == GameState.RESPAWN)
            return;

        if (_isTricking || _isJumping || IsTurning)
            return;

        _isTricking = true;
        if(_animator)
            _animator.SetTrigger("Trick 1");

    }
    private void OnTrickOneInputUp(InputAction.CallbackContext context)
    {

    }

    private void OnTrickTwoInputDown(InputAction.CallbackContext context)
    {

        if (GameEventsManager.Instance == null || GameEventsManager.Instance.CurrentGameState == GameState.GAMEOVER || GameEventsManager.Instance.CurrentGameState == GameState.PAUSE || GameEventsManager.Instance.CurrentGameState == GameState.DEATH || GameEventsManager.Instance.CurrentGameState == GameState.RESPAWN)
            return;

        if (_isTricking || _isJumping || IsTurning)
            return;

        _isTricking = true;
        _isJumping = true;
        if (_animator)
            _animator.SetTrigger("Trick 2");

    }
    private void OnTrickTwoInputUp(InputAction.CallbackContext context)
    {

    }
    #endregion
    #region Run Events
    private void OnRoomChanged(RoomController roomController)
    {
        turnCoyoteTimer = 0;
        if (roomController.roomType == RoomType.turnLeft || roomController.roomType == RoomType.turnRight)
        {
            yPreturnRotation = transform.localEulerAngles.y;
        }
    }
    private void OnGameStateChanged(GameState _newState)
    {
        enabled = (_newState == GameState.GAMEPLAY || _newState == GameState.DEATH || _newState == GameState.RESPAWN || _newState == GameState.PAUSE);

        switch (_newState)
        {
            case GameState.GAMEPLAY:
                _animator.speed = animatorSpeed;
                break;
            case GameState.PAUSE:
                _animator.speed = 0;
                break;
            case GameState.GAMEOVER:
                break;
            case GameState.DEATH:
                respawnCooldownTimer = _continuePressCooldownTime;
                break;
            case GameState.RESPAWN:
                ResetEverything();
                break;
            default:
                break;
        }
    }

    #endregion
    #region Turning Functions
    private void UpdateTurnHandling(float difficulty)
    {
        currentMaxTurnTime = Mathf.Lerp(_maxTurnTime, _minTurnTime, DifficultyManager.Instance.difficulty);
        warningTimeOne = currentMaxTurnTime / 2;
        warningTimeTwo = currentMaxTurnTime * 0.75f;
    }
    // Update is called once per frame
    private void HandleTurning(RoomType CurrentRoomType)
    {
        if (_startTurning)
        {
            Vector2 MovementInput = input.actions["Movement"].ReadValue<Vector2>();

            bool isTurningRight = (MovementInput.x > 0);
            bool isTurningLeft = (MovementInput.x < 0);

            turnTimer += Time.fixedDeltaTime;
            if (turnTimer >= currentMaxTurnTime)
            {
                _animator.SetTrigger("Fall");
            }
            else if (turnTimer >= warningTimeTwo)
            {
                _animator.SetTrigger("Warning 2");
            }
            else if (turnTimer >= warningTimeOne)
            {
                _animator.SetTrigger("Warning 1");
            }

            bool ShouldLoseLife = (turnTimer >= currentMaxTurnTime || (CurrentRoomType == RoomType.turnRight && isTurningRight && MovementInput.x < 0) || (CurrentRoomType == RoomType.turnLeft && isTurningLeft && MovementInput.x > 0) || MovementInput.x == 0) && !_isInvincible;
            // If the button has been held down too long
            if (ShouldLoseLife)
            {
                // Die
                LoseALife();
                _startTurning = false;
            }
        }
    }

    private void CheckAliveState(RoomType CurrentRoomType)
    {
        // If the player should be turning and they're not
        bool IsTurnRoom = (CurrentRoomType == RoomType.turnLeft || CurrentRoomType == RoomType.turnRight);
        if (IsTurnRoom && !_startTurning)
        {
            turnCoyoteTimer += Time.fixedDeltaTime;
            float currentRotation = Mathf.Abs(yPreturnRotation - transform.localEulerAngles.y);
            Debug.Log("Pre Turn Camera Rotation: " + yPreturnRotation + " Post Turn Camera Rotation: " + transform.localEulerAngles.y + " Difference: " + currentRotation);
            if (turnCoyoteTimer >= _turnCoyoteTime && currentRotation < 85 && _playerMotor.NormalizedTime > 0.1 && !_isInvincible)
            {
                // Die
                LoseALife(true);
            }
        }
    }
    #endregion
    #region Health System
    public bool LoseALife(bool playDeathAnim = false)
    {
        if (_isInvincible)
            return false;

        if (playDeathAnim)
            _animator.SetTrigger("Die");

        if (_isTricking)
        {
            _animator.ResetTrigger("Trick 1");
            _animator.ResetTrigger("Trick 2");
            _animator.ResetTrigger("Trick 3");
            _isTricking = false;
        }

        Debug.Log("Lost a life");
        _liveCount--;
        OnLostLife?.Invoke(_liveCount);
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.SetState(GameState.DEATH);
        }
        bool isDead = (_liveCount <= 0);
        if (!isDead)
        {
            BecomeInvincible();
        }
        else
        {
            if (GameEventsManager.Instance)
            {
                GameEventsManager.Instance.SetState(GameState.GAMEOVER);
            }
            //OnGameOver?.Invoke();
        }
        return isDead;
    }
    private void BecomeInvincible()
    {
        Debug.Log("Player turned invincible!");
        _isInvincible = true;
    }
    #endregion
    #region Animation Helpers
    private void ResetEverything()
    {
        _animator.ResetTrigger("Fall");
        _animator.ResetTrigger("Warning 2");
        _animator.ResetTrigger("Warning 1");
        _animator.ResetTrigger("Reset Turn");
        _animator.ResetTrigger("Jump");
        _animator.ResetTrigger("Fall Idle");

        _isJumping = false;
    }
    private void OnJumpLanded()
    {
        if (_isJumping)
        {
            _isJumping = false;
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySFX(jumpLandSplash);
            }
        }
    }

    private void OnJumpStarted()
    {
        _isJumping = true;
    }

    private void PlayDeathEffects()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySFX(deathSplash);
        }
    }
    void OnAnimationFinished()
    {
        if (!IsInvincible)
        {
            ScoreManager.Instance.AddScore(80, true);
        }
        _isTricking = false;
        _animator.ResetTrigger("Trick 1");
        _animator.ResetTrigger("Trick 2");
        _animator.ResetTrigger("Trick 3");
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySFX(NiceTrick);
        }
    }
    void OnFinishedRespawning()
    {
        GameEventsManager.Instance.SetState(GameState.GAMEPLAY);
    }

    #endregion
}

public struct TurnInfo
{
    public RoomController TurningRoom;
    public float completionPercentage;
}