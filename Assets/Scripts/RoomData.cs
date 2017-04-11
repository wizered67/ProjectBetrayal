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
    [SyncVar(hook="roomTypeChange")]
    public int roomType;

    private VisibilityStatus visibility = VisibilityStatus.HIDDEN;
    public bool hasBeenSeen = false;

    public Sprite[] roomTypeSprites;

    public Color visibleColor;
    public Color hiddenColor;
    public Color fadedColor;

    public float raisedZ;
    public float loweredZ;
    public int raisedLayerOrder;
    public int loweredLayerOrder;
    Color oldColor;
    SpriteRenderer spriteRenderer;
    public Transform[] internalPositions = new Transform[5];

	// Use this for initialization
	void Start () {
        GameObject.Find("RoomManager").GetComponent<WorldController>().addRoom(roomX, roomY, gameObject);
        spriteRenderer = GetComponent<SpriteRenderer>();
        setSpriteToType(roomType);
        GetComponent<SpriteRenderer>().color = color;
        hideRoom();
    }

    public void init()
    {
        GameObject.Find("RoomManager").GetComponent<WorldController>().addRoom(roomX, roomY, gameObject);
    }

    public void roomTypeChange(int newType)
    {
        roomType = newType;
        setSpriteToType(newType);
    }

    public void setSpriteToType(int type)
    {
        GetComponent<SpriteRenderer>().sprite = roomTypeSprites[type];
    }
	
	// Update is called once per frame
	void Update () {
        lowerRoom();
	}
    //Raise this room so that it's above players
    public void raiseRoom()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, raisedZ);
        spriteRenderer.sortingOrder = raisedLayerOrder;
    }
    //Lower the room so that it's below players.
    public void lowerRoom()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, loweredZ);
        spriteRenderer.sortingOrder = loweredLayerOrder;
    }

    public void hideRoom()
    {
        raiseRoom();
        spriteRenderer.color = hiddenColor;
    }

    public void showRoom()
    {
        lowerRoom();
        spriteRenderer.color = visibleColor;
    }

    public void fadeRoom()
    {
        raiseRoom();
        spriteRenderer.color = fadedColor;
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
    /*
    void OnMouseEnter()
    {
        //oldColor = GetComponent<SpriteRenderer>().color;
        PlayerMovement pm = PlayerMovement.localPlayer.GetComponent<PlayerMovement>();
        Vector2 diffVector = new Vector2(roomX, roomY) - pm.roomPosition;
        if (pm.isValidMove(diffVector))
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;
        }

}*/

    void OnMouseExit()
    {
        switch (visibility)
        {
            case VisibilityStatus.HIDDEN:
                hideRoom();
                break;
            case VisibilityStatus.FADED:
                fadeRoom();
                break;
            case VisibilityStatus.VISIBLE:
                showRoom();
                hasBeenSeen = true;
                break;
        }
        //GetComponent<SpriteRenderer>().color = oldColor;
    }

    public Transform getInternalPosition(int num)
    {
        return internalPositions[num];
    }
}

public enum VisibilityStatus
{
    VISIBLE, HIDDEN, FADED
}
