using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviour
{
    private Animator anim => GetComponent<Animator>();
    private int playersInTrigger;

    private bool CanFinishLevel()
    {
        if(playersInTrigger == PlayerManager.instance.playerCountWinCondition)
            return true;

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            playersInTrigger++;
            AudioManager.instance.PlaySFX(2);

            anim.SetTrigger("activate");

            if(CanFinishLevel())
                GameManager.instance.LevelFinished();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
            playersInTrigger--;
    }
}
