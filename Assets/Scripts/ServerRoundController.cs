using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerRoundController : NetworkBehaviour {
    //Mapping of network ids to GameObjects
    private Dictionary<NetworkInstanceId, GameObject> players;
    private ServerDataManager serverData;
    // Use this for initialization
    void Start () {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }
    }

    void init()
    {
        // Only run this code on the server, so set enabled to false if this is a client!
        if (!isServer)
        {
            this.enabled = false;
            return;
        }
        print("Init players list.");
        players = new Dictionary<NetworkInstanceId, GameObject>();
        serverData = gameObject.GetComponent<ServerDataManager>();
    }
	
	// Update is called once per frame
	void Update () {
		if (readyToProcess())
        {
            bool canAnyMove = false;
            foreach (GameObject player in players.Values)
            {
                PlayerMovement pm = player.GetComponent<PlayerMovement>();
                ClientRoundController crc = player.GetComponent<ClientRoundController>();
                Stats stats = player.GetComponent<Stats>();
                pm.RpcMove();
                crc.sentMove = false;
                if (stats.getSpeed() >= serverData.subroundNumber + 1)
                {
                    pm.canMoveThisSubround = true;
                    pm.RpcStartRound();
                    canAnyMove = true;
                } else
                {
                    pm.canMoveThisSubround = false;
                }
            }
            if (canAnyMove)
            {
                serverData.subroundNumber += 1;
            } else
            {
                serverData.roundNumber += 1;
                serverData.subroundNumber = 1;
                foreach (GameObject player in players.Values)
                {
                    PlayerMovement pm = player.GetComponent<PlayerMovement>();
                    pm.canMoveThisSubround = true;
                    pm.RpcStartRound();
                }
            }
            
        }
	}
   
    bool readyToProcess()
    {
        if (players.Count <= 0)
        {
            return false;
        }
        foreach (GameObject player in players.Values)
        {
            if (!player.GetComponent<ClientRoundController>().hasSentMove() || !player.GetComponent<Stats>().isReady())
            {
                return false;
            }
        }
        print("Ready to process on server!");
        return true;
    }

    void playerJoin(GameObject player)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        ClientRoundController crc = player.GetComponent<ClientRoundController>();
        Stats stats = player.GetComponent<Stats>();
        stats.RpcUpdateStats();
        if (stats.isReady() && stats.getSpeed() >= serverData.subroundNumber)
        {
            pm.canMoveThisSubround = true;
            pm.RpcStartRound();
        } else
        {
            pm.canMoveThisSubround = false;
        }
        
    }

    public void addPlayer(NetworkInstanceId netId, GameObject player)
    {
        if (players == null)
        {
            init();
        }
        print("Adding player with id of " + netId);
        players.Add(netId, player);
        playerJoin(player);
    }

    public void removePlayer(NetworkInstanceId netId)
    {
        players.Remove(netId);
    }
}
