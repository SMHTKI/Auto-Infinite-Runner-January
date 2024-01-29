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

    public float Difficulty => _difficulty;
    private float _difficulty;

    [SerializeField] private float _difficultyConstant = 10;

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
        _difficulty = Mathf.Sqrt(GameEventsManager.Instance.CompletedRooms / _difficultyConstant);
        if (_difficulty > 1)
        {
            _difficulty = 1;
        }

        GameEventsManager.Instance.DifficultyChanged(_difficulty);
    }
}
