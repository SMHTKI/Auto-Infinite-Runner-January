using System.Collections.Generic;
using UnityEngine;


public class CourseManager : MonoBehaviour
{

    #region SINGLETON  
    private static CourseManager _instance = null;
    public static CourseManager Instance { get { return _instance; } }
    private CourseManager() { }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _spawnedRooms = new List<RoomController>();
            for (int i = 0; i < _startingRooms.Length; i++)
            {
                _spawnedRooms.Add(_startingRooms[i]);
            }
        }
    }
    #endregion

    [SerializeField] private Dictionary<RoomType, Room> _rooms;
    [SerializeField] private RoomController[] _startingRooms;
    [SerializeField] private GameObject Player;

    private List<RoomController> _spawnedRooms;
    private RoomController previousRoom = null;
    public RoomController CurrentRoom;
    public RoomType CurrentRoomType = RoomType.generic;
    private bool shouldGrantScore = true;

    #region Unity Messages
    void OnEnable()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnCompletedRoom += PlayerFinishedRoom;
            GameEventsManager.Instance.OnPlayerDeath += RevokeRoomScore;
        }
    }

    void OnDisable()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnCompletedRoom -= PlayerFinishedRoom;
            GameEventsManager.Instance.OnPlayerDeath -= RevokeRoomScore;
        }
    }
    void Start()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnCompletedRoom += PlayerFinishedRoom;
            GameEventsManager.Instance.OnPlayerDeath += RevokeRoomScore;
        }
    }
    #endregion
    #region Room Generation
    void SpawnNextRoom()
    {
        if (_spawnedRooms.Count > 0)
        {
            RoomController LastRoom = _spawnedRooms[_spawnedRooms.Count - 1];
            RoomController FirstRoom = _spawnedRooms[0];

            // Put the player on its new path
            if (GameEventsManager.Instance)
            {
                CurrentRoom = _spawnedRooms[1];
                GameEventsManager.Instance.RoomChanged(CurrentRoom);

                CurrentRoomType = CurrentRoom.roomType;
                // Spawn new Room
                GameObject newRoom = Instantiate(LastRoom.nextRoom.RoomPrefab, transform);
                RoomController controller = newRoom.GetComponent<RoomController>();
                controller.OnSpawnObject(LastRoom.gameObject, EvaluateNextRoom(LastRoom.nextRoom));

                // Move room where it needs to be.
                newRoom.transform.SetPositionAndRotation(LastRoom.NextRoomSpawnLocation.position, LastRoom.NextRoomSpawnLocation.rotation);

                // Spawn Any Obsticals
                TrySpawnObsticals(LastRoom.nextRoom, controller);

                // Add it to the list
                _spawnedRooms.Add(newRoom.GetComponent<RoomController>());

                // Remove the First room in the list 
                if (previousRoom != null)
                {
                    Destroy(previousRoom.gameObject);
                }
                previousRoom = FirstRoom;
                _spawnedRooms.Remove(FirstRoom);

              
            }
        }
    }
    private void TrySpawnObsticals(Room newRoom, RoomController roomController)
    {
        for (int i = 0; i < newRoom.Obsticals.Count; i++)
        {
            if (Random.value * 100 <= newRoom.Obsticals[i].SpawnChance)
            {
                Instantiate(newRoom.Obsticals[i].ObsticalPrefab, roomController.ObsticalSpawnLocation);
                break;
            }
        }
    }
    private Room EvaluateNextRoom(Room nextRoom)
    {
        // TO:DO Flesh out later
        int roomChoice = Random.Range(0, nextRoom.PotentialNextRoomList.Count);
        return nextRoom.PotentialNextRoomList[roomChoice];
    }
    #endregion
    #region Event Functions
    private void PlayerFinishedRoom()
    {
        AddTurnScore();
        SpawnNextRoom();
    }
    public void AddTurnScore()
    {
        if (shouldGrantScore)
        {
            if (CurrentRoomType == RoomType.turnRight || CurrentRoomType == RoomType.turnLeft)
            {
                if (ScoreManager.Instance)
                {
                    PlayerController playerController = Player.GetComponent<PlayerController>();
                    if (!playerController.IsInvincible && playerController.StartTurnRoom.TurningRoom != null)
                    {
                        int baseScore = 100;
                        int scoreToAdd = baseScore;

                        if (playerController.StartTurnRoom.TurningRoom.roomType == RoomType.turnRight || playerController.StartTurnRoom.TurningRoom.roomType == RoomType.turnLeft)
                        {
                            if (playerController.StartTurnRoom.completionPercentage > .05)
                            {
                                scoreToAdd = (int)(baseScore * (1 - playerController.StartTurnRoom.completionPercentage));
                            }
                        }
                        else
                        {
                            if (playerController.StartTurnRoom.completionPercentage < .95)
                            {
                                scoreToAdd = (int)(baseScore * (playerController.StartTurnRoom.completionPercentage));
                            }
                        }

                        ScoreManager.Instance.AddScore(scoreToAdd);
                    }
                }
            }
        }
        else
        {
            shouldGrantScore = true;
        }

    }
    void RevokeRoomScore(int currentLives)
    {
        shouldGrantScore = false;
    }

    #endregion

}
