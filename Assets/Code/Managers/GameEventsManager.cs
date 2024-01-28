using System;
using System.Collections;
using System.Collections.Generic;
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
    // Start is called before the first frame update
    public GameState CurrentGameState { get; private set; }
    public event Action<GameState> OnGameStateChanged;
    public void SetState(GameState newGameState)
    {
        if (newGameState == CurrentGameState)
            return;

        CurrentGameState = newGameState;
        OnGameStateChanged(newGameState);
    }

    public int CompletedRooms { get; private set; }
    public event Action OnCompletedRoom;
    public void CompleteRoom()
    {
        if (!player.IsInvincible)
        {
            CompletedRooms++;
        }
        OnCompletedRoom();
    }

    public event Action<float> OnDifficultyChanged;
    public void DifficultyChanged(float difficulty)
    {
        OnDifficultyChanged(difficulty);
    }
}

public enum GameState
{
    GAMEPLAY,
    PAUSE,
    GAMEOVER,
    DEATH,
    RESPAWN
}

