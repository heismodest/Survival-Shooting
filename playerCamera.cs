using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCamera : MonoBehaviour
{

    public Vector3 offset;
    public float followSpeed = 0.15f;
    private GameObject player;
    private GameObject enemy;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag ("Player");
    }

    void FixedUpdate()
    {

        if (player != null)
        {
        Vector3 camera_pos = player.transform.position + offset;
        Vector3 lerp_pos = Vector3.Lerp(transform.position, camera_pos, followSpeed);
        transform.position = lerp_pos;
        }
    
        else if (player = null)
        {
        enemy = GameObject.Find("Enemy");
        transform.position = enemy.transform.position + offset;
        }

    }
}
