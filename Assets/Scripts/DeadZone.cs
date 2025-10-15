using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();

        if (player != null)
        {
            player.Die();
            GameManager.instance.RespawnPlayer();
        }

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
            enemy.Die();
    }
}
