using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour

{
    public GameObject player;
    private Vector3 offset = new Vector3(0, 5, -8);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        //offset camera behind player by adding to players position
        transform.position = player.transform.position + offset;

    }
}
