using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsticalController : MonoBehaviour
{
    public int ScoreToAdd;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
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