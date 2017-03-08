﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
//todo switch over to new RoomManager to make sure rooms are stored because server data may not have been spawned yet.
public class ServerWorldController : NetworkBehaviour {
    public GameObject roomPrefab;
    public float roomSize;
	// Use this for initialization
	void Start () {
        if (isServer)
        {
            makeWorld();
        }
    }

    void makeWorld()
    {
        for (int x = 0; x < 5; x += 1)
        {
            for (int y = 0; y < 5; y += 1)
            {
                if (Random.Range(0f, 1f) > 0.2)
                {
                    GameObject room = Instantiate(roomPrefab);
                    room.transform.position = new Vector3(x * roomSize, y * roomSize, 0);
                    RoomData roomData = room.GetComponent<RoomData>();
                    roomData.color = Random.ColorHSV();
                    roomData.roomX = x;
                    roomData.roomY = y;
                    NetworkServer.Spawn(room);
                }
            }
        }
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
