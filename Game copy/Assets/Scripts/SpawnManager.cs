using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public GameObject cloudPrefab;

    private float spawnPosZ = 100;
    private float startDelay = 2.0f;
    private float spawnInterval = 1.5f;
    private float minSpawnInterval = 0.3f;
    private float decreaseSpawnInterval = 0.2f;

    private float cloudSpawnInterval = 1.5f; // Adjust this value based on your requirements
    private float nextCloudSpawnTime = 0.0f;

    private float elapsedTime;
    private PlayerController playerControllerScript;
    private float globalSpeedIncrease = 10.0f;
    private float speedIncreaseInterval = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnRandomObstacle", startDelay, spawnInterval);
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();

        if (playerControllerScript == null)
        {
            Debug.LogError("PlayerController script not found on the Player GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= speedIncreaseInterval && playerControllerScript != null)
        {
            IncreaseObstacleSpeedGlobally();
            elapsedTime = 0.0f;
            spawnInterval = Mathf.Max(spawnInterval - decreaseSpawnInterval, minSpawnInterval);
            // Restart the invoke repeating with the updated spawn interval
            CancelInvoke("SpawnRandomObstacle");
            InvokeRepeating("SpawnRandomObstacle", 0.0f, spawnInterval);
        }

        SpawnCloud();
    }

    void SpawnRandomObstacle()
    {
        if (playerControllerScript != null && !playerControllerScript.gameOver)
        {
            int laneSelection = Random.Range(0, playerControllerScript.numberOfLanes);
            int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);

            float spawnPosX = (laneSelection - 1) * playerControllerScript.laneWidth;
            float spawnPosY = 0.7f; // Default spawn position on the ground

            // Check the obstacle type and adjust spawn position
            if (obstaclePrefabs[obstacleIndex].CompareTag("Coin") || obstaclePrefabs[obstacleIndex].CompareTag("PowerUp"))
            {
                // Spawn coins and power-ups on the ground
                spawnPosY = 1.0f;
            }
            else
            {
                // Randomly select a height for other obstacles
                spawnPosY = Random.Range(0.7f, 3.0f);
            }

            Vector3 spawnPos = new Vector3(spawnPosX, spawnPosY, spawnPosZ);

            GameObject newObstacle = Instantiate(obstaclePrefabs[obstacleIndex], spawnPos, obstaclePrefabs[obstacleIndex].transform.rotation);
            MoveForward moveForwardScript = newObstacle.GetComponent<MoveForward>();

            if (moveForwardScript != null)
            {
                moveForwardScript.SetObstacleSpeed(moveForwardScript.GetSpeed() + globalSpeedIncrease);
            }
        }
    }

  void SpawnCloud()
{
    if (Time.time >= nextCloudSpawnTime)
    {
        int laneSelection = Random.Range(0, playerControllerScript.numberOfLanes);
        float spawnPosX = (laneSelection - 1) * playerControllerScript.laneWidth;
        float spawnPosY = Random.Range(25.0f, 30.0f); // Adjust the range based on the desired height of cloud spawn

        Vector3 spawnPos = new Vector3(spawnPosX, spawnPosY, spawnPosZ);

        GameObject newCloud = Instantiate(cloudPrefab, spawnPos, Quaternion.Euler(0, -180, 0));

        MoveForward moveForwardScript = newCloud.GetComponent<MoveForward>();
        if (moveForwardScript != null)
        {
            moveForwardScript.SetObstacleSpeed(globalSpeedIncrease);
        }

        nextCloudSpawnTime = Time.time + cloudSpawnInterval;
    }
}

    void IncreaseObstacleSpeedGlobally()
    {
        MoveForward[] moveForwardScripts = FindObjectsOfType<MoveForward>();

        foreach (MoveForward moveForwardScript in moveForwardScripts)
        {
            moveForwardScript.IncreaseSpeed(globalSpeedIncrease);
        }
    }
}