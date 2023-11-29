using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward1 : MonoBehaviour
{
    private float speed = 60.0f;

    private PlayerController playerControllerScript;
    
    private

    // Start is called before the first frame update
    void Start()
    {
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        //speed = GameObject.Find("ObstacleManager").GetComponent<ObstacleSpeedManager>().speed;
        
    }




    // Update is called once per frame
    void Update()
    {
        if (playerControllerScript.gameOver == false)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);

        }

    }
}