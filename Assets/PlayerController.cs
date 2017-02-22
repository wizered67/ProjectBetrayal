using UnityEngine.Networking;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SyncVar(hook="colorChange")]
    Color myColor;
    Color[] colors = { Color.red, Color.blue, Color.green, Color.cyan };
    int i = 0;

    void Start()
    {
        if (!isLocalPlayer)
        {
            colorChange(myColor);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;
        var y = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;
        transform.Translate(x, y, 0);
        if (Input.GetKeyDown(KeyCode.C))
        {
            CmdChangeColor(colors[++i % colors.Length]);   
        }
    }

    [Command]
    void CmdChangeColor(Color newColor)
    {
        myColor = newColor;
    }

    void colorChange(Color newColor)
    {
        print("color was changed to " + newColor.ToString()); 
        myColor = newColor;
        GetComponent<SpriteRenderer>().color = newColor;
    }

    public override void OnStartLocalPlayer()
    {
        //GetComponent<SpriteRenderer>().color = Color.red;
        CmdChangeColor(Color.red);
    }
}
