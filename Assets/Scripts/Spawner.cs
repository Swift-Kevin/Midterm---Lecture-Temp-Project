using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    [SerializeField] GameObject objToSpawn;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float timeBetweenSpawns;
    [SerializeField] int numToSpawn;
    
    int currentSpawnedIn;
    bool playerInRange;
    bool isSpawning;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    IEnumerator Spawn()
    {
        isSpawning = true;
        Instantiate(objToSpawn, spawnPoints[Random.Range(0, spawnPoints.Length)].position, transform.rotation);
        currentSpawnedIn++;
        yield return new WaitForSeconds(timeBetweenSpawns);
        isSpawning = false;
    }

    private void Start()
    {
        GameManager.instance.UpdateGameGoal(numToSpawn);
    }

    private void Update()
    {
        if (playerInRange && !isSpawning && currentSpawnedIn < numToSpawn)
        {
            StartCoroutine(Spawn());
        }
    }
}
