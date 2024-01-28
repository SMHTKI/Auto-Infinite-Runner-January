using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RoomController : MonoBehaviour
{
    public GameObject previousRoom;
    public Room nextRoom;
    public GameObject nextRoomGamobject;
    public RoomType roomType;
    [SerializeField] private GameObject followSplineObect;
    public SplineContainer followSpline;

    [SerializeField] public Transform NextRoomSpawnLocation;
    [SerializeField] public Transform ObsticalSpawnLocation;

    public void OnSpawnObject(GameObject _previousRoom, Room _nextRoom)
    {
        previousRoom = _previousRoom;
        nextRoom = _nextRoom;
        followSpline = followSplineObect.GetComponent<SplineContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

