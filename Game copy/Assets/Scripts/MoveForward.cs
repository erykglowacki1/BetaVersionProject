using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed;
    public ParticleSystem fire;
    private PlayerController playerControllerScript;
    private bool isMovingForward = true; // Added variable to determine the direction

    // Start is called before the first frame update
    void Start()
    {
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        SetObstacleSpeed(speed);
        fire.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerControllerScript.gameOver == false)
        {
            // Use the direction to determine movement along the z-axis
            float moveDirection = isMovingForward ? 1.0f : -1.0f;
            
            transform.Translate(Vector3.forward * Time.deltaTime * speed * moveDirection);
            fire.Stop();
        }
    }

    public void SetObstacleSpeed(float initialSpeed)
    {
        speed = initialSpeed;
    }

    public void IncreaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }

    public float GetSpeed()
    {
        return speed;
    }

    // New function to set the movement direction
    public void SetMovementDirection(bool moveForward)
    {
        isMovingForward = moveForward;
    }
}
