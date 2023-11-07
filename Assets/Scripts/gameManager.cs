using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header(" --- Player Components --- ")]
    [Tooltip("A Game Object that references the Player Prefab. This will be found automatically and doesn't need to be assigned a component.")]
    public GameObject player;
    [Tooltip("The player script that is off of the player prefab. This will be found automatically if there is a 'Player' tagged object in the scene.")]
    public playerController playerScript;
    
    public GameObject playerSpawnPos;

    [Header(" --- UI Components --- ")]
    public GameObject activeMenu;
    public GameObject pauseMenu;
    public GameObject winMenu;
    public GameObject loseMenu;
    public GameObject checkpointNotification;
    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI maxAmmoText;
    public TextMeshProUGUI enemeisRemainingText;
    public Image healthBar;
    public Image playerFlashUI;

    [Header(" --- Game Goals --- ")]
    int enemiesRemaining;

    public bool isPaused;
    float timeScaleOriginal;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        timeScaleOriginal = Time.timeScale;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();

        playerSpawnPos = GameObject.FindGameObjectWithTag("Player Spawn Pos");

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel") && activeMenu == null)
        {
            statePaused();
            activeMenu = pauseMenu;
            activeMenu.SetActive(isPaused);
        }
    }

    public void statePaused()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        isPaused = !isPaused;
    }

    public void stateUnpaused()
    {
        Time.timeScale = timeScaleOriginal;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = !isPaused;
        activeMenu.SetActive(false);
        activeMenu = null;
    }

    public void UpdateGameGoal(int amount)
    {
        enemiesRemaining += amount;
        if (enemiesRemaining <= 0)
        {
            StartCoroutine(YouWin());
        }

        enemeisRemainingText.text = enemiesRemaining.ToString("F0");
    }

    IEnumerator YouWin()
    {
        yield return new WaitForSeconds(1.5f);
        activeMenu = winMenu;
        activeMenu.SetActive(true);
        statePaused();
    }

    public void YouLose()
    {
        statePaused();
        activeMenu = loseMenu;
        activeMenu.SetActive(true);
    }
}
