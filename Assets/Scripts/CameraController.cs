using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    GameObject player;
    float spd = 0.5f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float xMovement = 0;
        float yMovement = 0;

        float zoomCo = (GetComponent<Camera>().orthographicSize / 48) / (GetComponent<Camera>().orthographicSize / 48);

        if (Input.GetKey(KeyCode.W))
        {
            yMovement = spd * zoomCo;
        } else if (Input.GetKey(KeyCode.S))
        {
            yMovement = -spd * zoomCo;
        }

        if (Input.GetKey(KeyCode.A))
        {
            xMovement = -spd * zoomCo;
        } else if (Input.GetKey(KeyCode.D))
        {
            xMovement = spd * zoomCo;
        }

        xMovement += (Input.mousePosition.x < 30 ? -spd : Input.mousePosition.x - Screen.width > -30 ? spd : 0) * zoomCo;
        yMovement += (Input.mousePosition.y < 30 ? -spd : Input.mousePosition.y - Screen.height > -30 ? spd : 0) * zoomCo;

        transform.Translate(xMovement, yMovement, 0);

        if (Input.GetKey(KeyCode.Space) && player != null)
        {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize - scroll * 32, 8, 48);

        //Go HOME
        if (Input.GetKey(KeyCode.Space))
        {
            if(player!=null)
            {
                transform.position = player.transform.position - Vector3.forward * 10;
            }
        }
    }

    public void setPlayer(GameObject p)
    {
        player = p;
    }
}
