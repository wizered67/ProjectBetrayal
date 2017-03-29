using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour {
    public float roomSize;
    public GameObject[,] rooms;

    public GameObject roomPrefab;
    bool madeWorld = false;
    public int worldWidth;
    public int worldHeight;
    public float xRoomOffset;
    public float yRoomOffset;
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void makeWorld()
    {
        for (int x = 0; x < worldWidth; x += 1)
        {
            for (int y = 0; y < worldHeight; y += 1)
            {
                GameObject room = Instantiate(roomPrefab);
                room.transform.position = new Vector3(x * roomSize + xRoomOffset * roomSize, y * roomSize + yRoomOffset * roomSize, 0);
                RoomData roomData = room.GetComponent<RoomData>();
                roomData.roomX = x;
                roomData.roomY = y;
                roomData.roomType = Random.Range(0, roomData.roomTypeSprites.Length);
                //roomData.setSpriteToType(roomData.roomType);
                roomData.init();
            }
        }
    }

    public GameObject getRoom(int x, int y)
    {
        return rooms[x, y];
    }

    public void addRoom(int x, int y, GameObject room)
    {
        if (rooms == null)
        {
            rooms = new GameObject[worldWidth, worldHeight];
        }
        rooms[x, y] = room;
    }

    public Vector2 getWorldCoordinates(Vector2 roomPosition)
    {
        return (roomPosition + new Vector2(xRoomOffset, yRoomOffset)) * roomSize;
    }
}
