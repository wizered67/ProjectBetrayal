using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class RoundController : NetworkBehaviour {
    //The number of unique clients that the server has received moves from.
    public static int numMoves = 0;
    //Whether the server has begun processing moves.
    public static bool processing = false;
    //Server side variable for each client. Whether it has sent a move yet.
    public bool sentMove = false;
    //Whether this client has started the current subround yet.
    [SyncVar]
    public int startedSubround = 0;
    //The number of rounds remaining in this phase.
    public static int numRoundsRemaining = 5;
    //Reference to PlayerMovement, used for locally handling movement.
    public PlayerMovement playerMovement;
    public GameObject serverDataPrefab;
    private Stats stats;
    private static ServerData serverData;
    public static List<GameObject> players;
    public static bool init = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!init)
        {
            GameObject serverDataObject = (GameObject)Instantiate(serverDataPrefab, transform.position, transform.rotation);
            serverData = serverDataObject.GetComponent<ServerData>();
            NetworkServer.Spawn(serverDataObject);
            players = new List<GameObject>();
            init = true;
        }
    }


    // Use this for initialization
    void Start () {
        playerMovement = GetComponent<PlayerMovement>();
        stats = GetComponent<Stats>();
        if (isServer)
        {
            players.Add(gameObject);
        }
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
        if (!processing && startedSubround != serverData.subroundNumber && stats.isReady())
        {
            startedSubround = serverData.subroundNumber;
            if (stats.getSpeed() < serverData.subroundNumber) //not enough speed remaining to keep moving
            {
                print("Not enough speed for " + netId);
                playerMovement.canMoveThisSubround = false;
                sentMove = true;
                numMoves += 1;
            }
            else
            {
                print("Has enough speed for " + netId);
                playerMovement.canMoveThisSubround = true;
            }
            playerMovement.RpcStartRound();
        }
        if (processing || numMoves == players.Count)
        {
            print(players.Count);
            print("All moves received by server. Processing moves for " + netId + "!");
            if (!processing) //first time the server starts processing moves
            {
                initialProcess();
            }
            processing = true;
            playerMovement.RpcMove();
            numMoves -= 1;
            if (numMoves <= 0)
            {
                processing = false;
                postProcess();
            }
        }
    }
    //Called the first time each subround that moves start processing.
    void initialProcess()
    {
        //numRoundsRemaining -= 1;
        serverData.subroundNumber += 1;
    }

    void postProcess()
    {
        bool ongoingRound = false;
        foreach (GameObject player in players)
        {
            if (player.GetComponent<Stats>().getSpeed() >= serverData.subroundNumber)
            {
                ongoingRound = true;
                break;
            }
        }
        if (!ongoingRound)
        {
            serverData.subroundNumber = 1;
            serverData.roundNumber += 1;
        }
    }
    //Receive a move from a player. If that player has not sent a move, increment the number of moves received and
    //mark them as having sent a move.
    [Command]
    public void CmdSentMove()
    {
        if (!sentMove)
        {
            numMoves += 1;
            print("There are now " + numMoves + " moves received.");
        }
        sentMove = true;

    }
    //Reset server-side client information once they have processed their move. 
    //Reset sent move and hasStartedSubround to false.
    [Command]
    public void CmdPostMove()
    {
        sentMove = false;
    }

    
}
