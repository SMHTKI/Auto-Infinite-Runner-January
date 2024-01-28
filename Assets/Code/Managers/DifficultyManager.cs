using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    #region SINGLETON
    private static DifficultyManager _instance = null;
    public static DifficultyManager Instance { get { return _instance; } }
    private DifficultyManager() { }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    #endregion

    public float difficulty;

    [SerializeField] private float difficultyConstant = 10;

    void OnEnable()
    {
        if (GameEventsManager.Instance)
            GameEventsManager.Instance.OnCompletedRoom += PlayerFinishedRoom;
    }

    void OnDisable()
    {
        if (GameEventsManager.Instance)
            GameEventsManager.Instance.OnCompletedRoom -= PlayerFinishedRoom;
    }

    private void Start()
    {
        if (GameEventsManager.Instance)
            GameEventsManager.Instance.OnCompletedRoom += PlayerFinishedRoom;
    }

    private void PlayerFinishedRoom()
    {
        AdjustDifficulty();
    }

    private void AdjustDifficulty()
    {
        difficulty = Mathf.Sqrt(GameEventsManager.Instance.CompletedRooms / difficultyConstant);
        if (difficulty > 1)
        {
            difficulty = 1;
        }

        GameEventsManager.Instance.DifficultyChanged(difficulty);
    }
}
