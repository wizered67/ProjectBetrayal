﻿using System.Collections;
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
    private WorldController worldController;
    private Dictionary<Vector2, List<GameObject>> roomPositionToPlayersList;
    // Use this for initialization
    void Start () {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }
        if (worldController == null)
        {
            worldController = GameObject.Find("RoomManager").GetComponent<WorldController>();
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
        roomPositionToPlayersList = new Dictionary<Vector2, List<GameObject>>();
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
                List<GameObject> playersInRoom = null;
                
                if (roomPositionToPlayersList.ContainsKey(pm.roomPosition))
                {
                    playersInRoom = roomPositionToPlayersList[pm.roomPosition];
                }
                if (playersInRoom != null && pm.currentMove != Vector2.zero)
                {
                    playersInRoom.Remove(player);
                    foreach (GameObject p in playersInRoom)
                    {
                        PlayerMovement ppm = p.GetComponent<PlayerMovement>();
                        int index = playersInRoom.LastIndexOf(p);
                        ppm.internalPosition = worldController.getRoom((int)ppm.roomPosition.x, (int)ppm.roomPosition.y)
                    .GetComponent<RoomData>().getInternalPosition(index).localPosition;
                    }
                }
                
               
                //process move server side
                pm.processMove();
                //remove from old room and add to new
                if (pm.currentMove != Vector2.zero)
                {
                    playersInRoom = null;
                    if (roomPositionToPlayersList.ContainsKey(pm.roomPosition))
                    {
                        playersInRoom = roomPositionToPlayersList[pm.roomPosition];
                    }
                    if (playersInRoom == null)
                    {
                        playersInRoom = new List<GameObject>();
                        roomPositionToPlayersList[pm.roomPosition] = playersInRoom;
                    }
                    playersInRoom.Add(player);
                    pm.RpcPlayDoorSound();
                }
                //reset local variables
                pm.RpcMove();
                int indexInRoomList = playersInRoom.LastIndexOf(player);
                pm.internalPosition = worldController.getRoom((int)pm.roomPosition.x, (int)pm.roomPosition.y)
            .GetComponent<RoomData>().getInternalPosition(indexInRoomList).localPosition;

                crc.sentMove = false;
                pm.currentMove.Set(0, 0);
                print("Cleared sent move.");
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

    public int numPlayersInRoom(Vector2 roomPosition)
    {
        int num = 0;
        foreach (GameObject player in players.Values)
        {
            if (player.GetComponent<PlayerMovement>().roomPosition == roomPosition)
            {
                num += 1;
            }
        }
        return num;
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
        //Add to room list and update position
        List<GameObject> playersInRoom = null;
        if (roomPositionToPlayersList.ContainsKey(pm.roomPosition))
        {
            playersInRoom = roomPositionToPlayersList[pm.roomPosition];
        }
        
        if (playersInRoom == null)
        {
            playersInRoom = new List<GameObject>();
            roomPositionToPlayersList[pm.roomPosition] = playersInRoom;
        }
        playersInRoom.Add(player);
        int indexInRoomList = playersInRoom.LastIndexOf(player);
        GetComponent<ServerWorldController>().makeWorld();
        if (worldController == null)
        {
            worldController = GameObject.Find("RoomManager").GetComponent<WorldController>();
        }
        pm.internalPosition = worldController.getRoom((int)pm.roomPosition.x, (int)pm.roomPosition.y)
    .GetComponent<RoomData>().getInternalPosition(indexInRoomList).localPosition;

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
        //remove from the room they were in
        if (players.ContainsKey(netId)) //why would this originally give a key not found exception...
        {
            GameObject player = players[netId];
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            List<GameObject> playersInRoom = null;
            if (roomPositionToPlayersList.ContainsKey(pm.roomPosition))
            {
                playersInRoom = roomPositionToPlayersList[pm.roomPosition];
            }
            if (playersInRoom != null)
            {
                playersInRoom.Remove(player);
            }
        }
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

    public void sendAnimations()
    {
        PlayerMovement pmOne = playerOne.GetComponent<PlayerMovement>();
        PlayerMovement pmTwo = playerTwo.GetComponent<PlayerMovement>();
        pmOne.isAttacking = true;
        pmTwo.isAttacking = true;
        pmOne.attackAnimationTarget = (playerOne.transform.position + playerTwo.transform.position) / 2;
        pmTwo.attackAnimationTarget = pmOne.attackAnimationTarget;
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
        sendAnimations();
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
