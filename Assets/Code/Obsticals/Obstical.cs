using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FuckingSealGame/Obsticals/Generic")]
public class Obstical : ScriptableObject
{
    /// <summary>
    /// The gameobject to Instantiate
    /// </summary>
    [Tooltip("The gameobject to Instantiate")]
    public GameObject ObsticalPrefab;

    /// <summary>
    /// The percentage chance for this obstical to spawn. Between 0 - 100
    /// </summary>
    [Tooltip("The percentage chance for this obstical to spawn")]
    public float SpawnChance;
}
