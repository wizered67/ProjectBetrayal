using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RoomData : NetworkBehaviour {
    [SyncVar(hook = "colorChange")]
    public Color color;
    [SyncVar]
    public int roomX;
    [SyncVar]
    public int roomY;
	// Use this for initialization
	void Start () {
        GetComponent<SpriteRenderer>().color = color;
        GameObject.Find("ServerData").GetComponent<ServerWorldController>().addRoom(roomX, roomY, gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void colorChange(Color newColor)
    {
        color = newColor;
        GetComponent<SpriteRenderer>().color = newColor;
    }
}
