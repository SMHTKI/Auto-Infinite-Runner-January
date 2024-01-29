using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsticalController : MonoBehaviour
{
    [SerializeField] private int ScoreToAdd;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.transform.root.GetComponent<PlayerController>();
        if (player && !player.IsInvincible)
        {
            if (player.IsJumping)
            {
                ScoreManager.Instance.AddScore(ScoreToAdd, true);
            }
            else
            {
                player.LoseALife(true);
            }
        }
    }
}