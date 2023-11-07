using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void Resume()
    {
        gameManager.instance.stateUnpaused();
    }

    public void Restart()
    {
        // Loads the singleton Game Managers instance and runs the stateUnpaused function
        gameManager.instance.stateUnpaused();
        // This reloads the scene with the current scenes name
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Exiting...");
    }

    public void RespawnPlayer()
    {
        gameManager.instance.stateUnpaused();
        gameManager.instance.playerScript.SpawnPlayer();
    }
}
