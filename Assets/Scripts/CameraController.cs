using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    GameObject player;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (player == null)
        {
            return;
        }
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
	}

    public void setPlayer(GameObject p)
    {
        player = p;
    }
}
