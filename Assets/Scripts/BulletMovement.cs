﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletMovement : NetworkBehaviour {
    public Vector3 target;
    public Vector3 origin;
    public float progress = 0f;
    public uint attackerId;
    public uint targetId;

    float speed = 5f;

	// Use this for initialization
	void Start ()
    {
        print("Bullet was spawned!");
        transform.up = target - origin;
    }

    public void setParticipants(uint attacker, uint target)
    {
        attackerId = attacker;
        targetId = target;
    }
	
	// Update is called once per frame
	void Update () {
        //Vector3 targetPos = new Vector3(target.x, target.y, transform.position.z);
        //transform.LookAt(new Vector3(target.x,target.y,transform.position.z));

        progress += speed*Time.deltaTime;
        transform.position = Vector3.Lerp(origin, target, progress);

        if (progress >= 1)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                PlayerMovement pm = player.GetComponent<PlayerMovement>();
                uint id = pm.netId.Value;
                if (id == attackerId || id == targetId)
                {
                    pm.isAttacking = false;                    
                }
            }
            Destroy(gameObject);
        }
	}
}
