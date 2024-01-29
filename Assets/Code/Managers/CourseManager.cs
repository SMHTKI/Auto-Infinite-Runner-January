using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

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
            _spawnedRooms = new List<RoomController> ();
            for (int i = 0; i < _startingRooms.Length; i++)
            {
                _spawnedRooms.Add(_startingRooms[i]);
            }
        }
    }
    #endregion

    void OnEnable()
    {
        if(GameEventsManager.Instance)
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

    void RevokeRoomScore(int currentLives)
    {
        shouldGrantScore = false;
    }

    [SerializeField] Dictionary<RoomType, Room> _rooms;
    [SerializeField] RoomController[] _startingRooms;
    List<RoomController> _spawnedRooms;
    [SerializeField] Collider PlayerCollider;
    [SerializeField] GameObject Player;
    [SerializeField] Collider SpawnCollider;

    public bool IsTurnRoom = false;
    private RoomController previousRoom = null;
    public UnityEvent<RoomController> OnRoomChanged;
    public RoomController CurrentRoom;
    public RoomType CurrentRoomType = RoomType.generic;
    private bool shouldGrantScore = true;

    // Start is called before the first frame update
    void Start()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnCompletedRoom += PlayerFinishedRoom;
            GameEventsManager.Instance.OnPlayerDeath += RevokeRoomScore;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
                if (ScoreManager.Instance )
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

    void SpawnNextRoom()
    {
        if (_spawnedRooms.Count > 0)
        {
            RoomController LastRoom = _spawnedRooms[_spawnedRooms.Count - 1];
            RoomController FirstRoom = _spawnedRooms[0];
            RoomController SecondRoom = _spawnedRooms[1];

            // Put the player on its new path
            PlayerMotor motor = Player.GetComponent<PlayerMotor>();
            PlayerController PlayerController = Player.GetComponent<PlayerController>();

            if (motor != null)
            {
                OnRoomChanged?.Invoke(SecondRoom);
                CurrentRoom = SecondRoom;
                CurrentRoomType = SecondRoom.roomType;
                motor.CurrentSpline = SecondRoom.followSpline;
                IsTurnRoom = (SecondRoom.roomType == RoomType.turnLeft || SecondRoom.roomType == RoomType.turnRight);
            }

            // Spawn new Room
            GameObject newRoom = Instantiate(LastRoom.nextRoom.RoomPrefab, transform);
            RoomController controller = newRoom.GetComponent<RoomController>();
            controller.OnSpawnObject(LastRoom.gameObject, EvaluateNextRoom(LastRoom.nextRoom));
            LastRoom.nextRoomGamobject = newRoom;

            // Move room where it needs to be.
            newRoom.transform.SetPositionAndRotation(LastRoom.NextRoomSpawnLocation.position, LastRoom.NextRoomSpawnLocation.rotation);
            //SpawnCollider.transform.SetPositionAndRotation(FirstRoom.nextRoomGamobject.transform.position, Quaternion.identity);

            // Spawn Any Obsticals
            TrySpawnObsticals(LastRoom.nextRoom, controller);

            // If true this is the first room
            if (previousRoom != null)
            {
                Destroy(previousRoom.gameObject);
            }

            previousRoom = FirstRoom;
            _spawnedRooms.Remove(FirstRoom);
            // Add it to the list
            _spawnedRooms.Add(newRoom.GetComponent<RoomController>());

        }
    }

    private void TrySpawnObsticals(Room newRoom, RoomController roomController)
    {
        for (int i = 0; i < newRoom.Obsticals.Count; i++)
        {
            if(Random.value * 100 <= newRoom.Obsticals[i].SpawnChance)
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == PlayerCollider)
        {
            SpawnNextRoom();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == PlayerCollider)
        {
            SpawnNextRoom();
        }
    }
}
