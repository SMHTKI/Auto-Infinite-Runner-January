using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FuckingSealGame/Rooms/Generic")]

public class Room : ScriptableObject
{
    public GameObject RoomPrefab;
    public RoomType RoomType;
    public List<Room> PotentialNextRoomList;
    public List<Obstical> Obsticals;
}

public enum RoomType
{
    generic,
    turnRight,
    turnLeft,
    down,
    Count
}
