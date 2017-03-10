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

    Color oldColor;

    public Transform[] internalPositions = new Transform[5];

	// Use this for initialization
	void Start () {
        GameObject.Find("RoomManager").GetComponent<WorldController>().addRoom(roomX, roomY, gameObject);
        GetComponent<SpriteRenderer>().color = color;
    }

    public void init()
    {
        GameObject.Find("RoomManager").GetComponent<WorldController>().addRoom(roomX, roomY, gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void colorChange(Color newColor)
    {
        color = newColor;
        GetComponent<SpriteRenderer>().color = newColor;
    }

    void OnMouseDown()
    {
        PlayerMovement.localPlayer.GetComponent<PlayerMovement>().setDestination(new Vector2(roomX, roomY));
    }

    void OnMouseEnter()
    {
        oldColor = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = oldColor;
    }

    public Transform getInternalPosition(int num)
    {
        return internalPositions[num];
    }
}
