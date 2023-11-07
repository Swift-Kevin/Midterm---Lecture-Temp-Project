using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Renderer model;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameManager.instance.playerSpawnPos.transform.position != transform.position)
        {
            gameManager.instance.playerSpawnPos.transform.position = transform.position;

            StartCoroutine(FlashColor());
        }
    }

    IEnumerator FlashColor()
    {
        model.material.color = Color.green;
        gameManager.instance.checkpointNotification.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        gameManager.instance.checkpointNotification.SetActive(false);
        model.material.color = Color.white;
    }
}
