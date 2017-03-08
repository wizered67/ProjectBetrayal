using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    GameObject player;
    public float spd = 0.5f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float xMovement = 0;
        float yMovement = 0;
		if (Input.GetKey(KeyCode.W))
        {
            yMovement = spd;
        } else if (Input.GetKey(KeyCode.S))
        {
            yMovement = -spd;
        }

        if (Input.GetKey(KeyCode.A))
        {
            xMovement = -spd;
        } else if (Input.GetKey(KeyCode.D))
        {
            xMovement = spd;
        }
        transform.Translate(xMovement, yMovement, 0);

        if (Input.GetKey(KeyCode.Space) && player != null)
        {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize - scroll * 8, 1, 25);

    }

    public void setPlayer(GameObject p)
    {
        player = p;
    }
}
