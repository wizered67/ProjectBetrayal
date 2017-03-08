using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour {
    public float roomSize;
    public GameObject[,] rooms;
    public int width, height;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void addRoom(int x, int y, GameObject room)
    {
        if (rooms == null)
        {
            rooms = new GameObject[5, 5];
            width = 5;
            height = 5;
        }
        rooms[x, y] = room;
    }
}
