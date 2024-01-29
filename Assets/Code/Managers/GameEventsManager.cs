using System;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{

    #region SINGLETON
    private static GameEventsManager _instance = null;
    public static GameEventsManager Instance { get { return _instance; } }
    private GameEventsManager() { }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    #endregion

    [SerializeField] PlayerController player;

    /// <summary>
    /// Changes the current state of the Game as well as broadcasts the event OnGameStateChanged
    /// </summary>
    /// <param name="newGameState">The state to change to.</param>
    /// <Remarks> Will not call event if its being changed to the same event.</Remarks>
    public void SetState(GameState newGameState)
    {
        if (newGameState == CurrentGameState)
            return;

        CurrentGameState = newGameState;
        OnGameStateChanged(newGameState);
    }
    public GameState CurrentGameState { get; private set; }
    public event Action<GameState> OnGameStateChanged;

    /// <summary>
    /// Increments the current completed rooms count of the current run as well as broadcasts the event OnCompletedRooms.
    /// </summary>
    /// <Remarks> Will not call increment of the player is currently dead/invincible</Remarks>
    public void CompleteRoom()
    {
        if (!player.IsInvincible)
        {
            CompletedRooms++;
        }
        OnCompletedRoom();
    }
    public int CompletedRooms { get; private set; }
    public event Action OnCompletedRoom;

    /// <summary>
    /// Broadcasts the event OnDifficultyChanged whenever the difficulty of a current run updates from the Difficulty Manager
    /// </summary>
    /// <param name="difficulty">The updated difficulty value</param>
    public void DifficultyChanged(float difficulty)
    {
        OnDifficultyChanged(difficulty);
    }
    public event Action<float> OnDifficultyChanged;

    /// <summary>
    /// Broadcasts the event OnScoreAdded whenever an action adds additional score to the score manager
    /// </summary>
    /// <param name="currentScore">The current score before the additional score is added</param>
    /// <param name="additionalScore">The score to add to the current score of the run</param>
    /// <param name="isObstical">Modifier for obstical added score as apposed to turns.</param>
    public void ScoreAdded(int currentScore, int scoreToAdd, bool isObstical)
    {
         OnScoreAdded(currentScore, scoreToAdd, isObstical);
    }
    public event Action<int, int, bool> OnScoreAdded;

    /// <summary>
    /// Broadcasts the event OnPlayerDeath as well as updates the current state to death or GameOver depending on live count
    /// </summary>
    /// <param name="currentLives">The current amount of live the player has left</param>
    public void PlayerDeath(int currentLives)
    {
        OnPlayerDeath(currentLives);
        if (currentLives > 0)
        {
            SetState(GameState.DEATH);
        }
        else
        {
            GameOver();
        }
    }
    public event Action<int> OnPlayerDeath;

    /// <summary>
    /// Broadcasts the event OnGameOver as well as updates the current state to gameover
    /// </summary>
    /// <param name="currentLives">The current amount of live the player has left</param>
    public void GameOver()
    {
        OnGameOver();
        SetState(GameState.GAMEOVER);
    }
    public event Action OnGameOver;

    /// <summary>
    /// Broadcasts the event OnPlayerRespawned as well as updates the current state to respawn
    /// </summary>
    public void PlayerRespawned()
    {
        OnPlayerRespawned();
        SetState(GameState.RESPAWN);
    }
    public event Action OnPlayerRespawned;

    /// <summary>
    /// Calls the event OnRoomChanged whenever the player has succesfully moved from their previous room to a new one.
    /// </summary>
    /// <param name="newRoom">The room to be traversed by the player next</param>
    public void RoomChanged(RoomController newRoom)
    {
        OnRoomChanged(newRoom);
    }
    public event Action<RoomController> OnRoomChanged;
}

public enum GameState
{
    GAMEPLAY,
    PAUSE,
    GAMEOVER,
    DEATH,
    RESPAWN
}

