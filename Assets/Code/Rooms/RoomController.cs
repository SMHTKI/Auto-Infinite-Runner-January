using UnityEngine;
using UnityEngine.Splines;

public class RoomController : MonoBehaviour
{

    [SerializeField] private GameObject followSplineObect;
    [SerializeField] public Transform NextRoomSpawnLocation;
    [SerializeField] public Transform ObsticalSpawnLocation;

    // The room that spawned this room/ comes before it in the sequence.
    public GameObject previousRoom;
    // The room to spawn after this one.
    public Room nextRoom;
    // The current rooms type
    public RoomType roomType;
    // The spline of the current room
    public SplineContainer followSpline;

    /// <summary>
    /// Initializes room object's variables on the room's spawn
    /// </summary>
    /// <param name="_previousRoom">The instantiated room that spawned this one</param>
    /// <param name="_nextRoom">The Room object to spawn from this room when the time comes</param>
    public void OnSpawnObject(GameObject _previousRoom, Room _nextRoom)
    {
        previousRoom = _previousRoom;
        nextRoom = _nextRoom;
        followSpline = followSplineObect.GetComponent<SplineContainer>();
    }
}

