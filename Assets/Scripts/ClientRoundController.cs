using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ClientRoundController : NetworkBehaviour {
    //Server side variable for each client. Whether it has sent a move yet.
    public bool sentMove = false;
    
    //Reference to PlayerMovement, used for locally handling movement.
    public PlayerMovement playerMovement;
    public GameObject serverDataPrefab;
    private Stats stats;
    private static ServerDataManager serverData;
    private static ServerRoundController serverRoundController;
    public static bool init = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
    }


    // Use this for initialization
    void Start () {
        if (!init && isServer)
        {
            GameObject serverDataObject = (GameObject)Instantiate(serverDataPrefab, transform.position, transform.rotation);
            serverData = serverDataObject.GetComponent<ServerDataManager>();
            serverRoundController = serverDataObject.GetComponent<ServerRoundController>();
            NetworkServer.Spawn(serverDataObject);
            init = true;
        }
        playerMovement = GetComponent<PlayerMovement>();
        stats = GetComponent<Stats>();
        stats.init();
        
        if (isServer)
        {
            print(gameObject);
            serverRoundController.addPlayer(netId, gameObject);
        }
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        serverRoundController.removePlayer(netId);
    }

    // Update is called once per frame
    void Update () {
        if (isLocalPlayer)
        {
            localUpdate();
        }
        if (isServer)
        {
            serverUpdate();
        }
    }

    public bool hasSentMove()
    {
        print("can move is " + playerMovement.canMoveThisSubround);
        print("sent move is " + sentMove);
        return !playerMovement.canMoveThisSubround || sentMove;
    }

    //Update that only happens if this is the local player.
    void localUpdate()
    {

    }
    //Update that only happens if this is on the server. Run on each client gameobject, but only on server side.
    //If there are remaining rounds but this client has not started one, start it and tell them to start their timer.
    //If processing has already begun or all moves have been received, then start processing and tell the player associated
    //with this client to act out their move. Decrements number of moves and only continues processing while it's above 0,
    //so that each client only has moves processed once.
    void serverUpdate()
    {
           
    }
    
    //Receive a move from a player. If that player has not sent a move, increment the number of moves received and
    //mark them as having sent a move.
    [Command]
    public void CmdSentMove()
    {
        sentMove = true;
    }
 
}
