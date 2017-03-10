using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
//todo how to process battles? Need some way to make sure all clients have sent battle requests before
//they can be processed.
public class ServerRoundController : NetworkBehaviour {
    //Mapping of network ids to GameObjects
    private Dictionary<NetworkInstanceId, GameObject> players;
    private HashSet<Battle> battleSet;
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
        battleSet = new HashSet<Battle>();
        serverData = gameObject.GetComponent<ServerDataManager>();
    }

    // Update is called once per frame
    void Update () {

		if (readyToProcess())
        {
            foreach (Battle battle in battleSet)
            {
                battle.process();
            }
            battleSet.Clear();
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

    public void addBattle(GameObject playerOne, GameObject playerTwo)
    {
        battleSet.Add(new Battle(playerOne, playerTwo));
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
                //print("Waiting on move from " + player.GetComponent<NetworkIdentity>().netId);
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

class Battle
{
    private GameObject playerOne;
    private GameObject playerTwo;

    public Battle(GameObject p1, GameObject p2)
    {
        playerOne = p1;
        playerTwo = p2;
    }

    public void process()
    {
        Stats playerOneStats = playerOne.GetComponent<Stats>();
        Stats playerTwoStats = playerTwo.GetComponent<Stats>();
        int playerOneRoll = roll(playerOneStats.getMight());
        int playerTwoRoll = roll(playerTwoStats.getMight());
        int statLoss = calculateStatLoss(playerOneRoll, playerTwoRoll);
        Debug.Log("Player " + playerOne.GetComponent<NetworkIdentity>().netId + " rolled " + playerOneRoll);
        Debug.Log("Player " + playerTwo.GetComponent<NetworkIdentity>().netId + " rolled " + playerTwoRoll);
        if (playerOneRoll > playerTwoRoll)
        {
            Debug.Log("Player " + playerOne.GetComponent<NetworkIdentity>().netId + " won the fight!");
            playerTwoStats.gainMight(statLoss);
        } else
        {
            Debug.Log("Player " + playerTwo.GetComponent<NetworkIdentity>().netId + " won the fight!");
            playerOneStats.gainMight(statLoss);
        }
    }

    int calculateStatLoss(int roll1, int roll2)
    {
        return -Mathf.CeilToInt(Mathf.Abs(roll1 - roll2) / 2f);
    }

    int roll(int numDice)
    {
        int total = 0;
        for (int i = 0; i < numDice; i += 1)
        {
            total += Random.Range(0, 3); //second number is not inclusive when called with ints for some reason
        }
        return total;
    }

    public override bool Equals(object o)
    {
        Battle other = o as Battle;
        if (other == null)
        {
            return false;
        }
        return (other.playerOne == playerOne && other.playerTwo == playerTwo)
            || (other.playerTwo == playerOne && other.playerOne == playerTwo);
    }

    public override int GetHashCode()
    {
        return playerOne.GetHashCode() + playerTwo.GetHashCode();
    }
}
