using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FuckingSealGame/Rooms/Generic")]

public class Room : ScriptableObject
{
    /// <summary>
    /// The gameobject to Instantiate
    /// </summary>
    [Tooltip("The gameobject to Instantiate")]
    public GameObject RoomPrefab;

    /// <summary>
    /// The type of room this represents
    /// </summary>
    [Tooltip("The type of room this represents")]
    public RoomType RoomType;

    /// <summary>
    /// A list of potential rooms to spawn next for this room. Chosen on instantation.
    /// </summary>
    [Tooltip("A list of potential rooms to spawn next for this room. Chosen on instantation.")]
    public List<Room> PotentialNextRoomList;

    /// <summary>
    /// The list of potential objects to spawn in this room. Chosen on instantiation with a chance of none being spawned.
    /// </summary>
    [Tooltip("The list of potential objects to spawn in this room. Chosen on instantiation with a chance of none being spawned.")]
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
