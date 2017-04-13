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
    private WorldController worldController;
    private Dictionary<Vector2, List<GameObject>> roomPositionToPlayersList;

    //Spawnpoints { 1,10; 6,3; 11,4; 16,3; 20,10; 17,13; 11,16; 7,13}
    static List<Vector2> mySpawnPoints = new List<Vector2>(new Vector2[]
    {
        new Vector2(1, 10),
        new Vector2(6, 3),
        new Vector2(11, 4),
        new Vector2(16, 3),
        new Vector2(20, 10),
        new Vector2(17, 13),
        new Vector2(11, 16),
        new Vector2(7, 13)
    }
    );

    //Public Vars for setting player images
    public List<Sprite> playerSprites = new List<Sprite>();

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
    void Update ()
    {
		if (readyToProcess())
        {
            //Updating lights for bloodscent
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (obj.transform.FindChild("2DLightEx").gameObject.activeInHierarchy)
                {
                    obj.transform.FindChild("2DLightEx").GetComponent<MeshRenderer>().enabled = false;
                    StartCoroutine(DelayedLightUpdate(obj));
                }
            }

            foreach (Battle battle in battleSet)
            {
                battle.process();
            }
            battleSet.Clear();

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
                //playersInRoom = roomPositionToPlayersList[pm.roomPosition];
                int indexInRoomList = playersInRoom.LastIndexOf(player);
                pm.internalPosition = worldController.getRoom((int)pm.roomPosition.x, (int)pm.roomPosition.y)
            .GetComponent<RoomData>().getInternalPosition(indexInRoomList).localPosition;

                crc.sentMove = false;
                pm.currentMove.Set(0, 0);
                print("Cleared sent move.");
                if (Stats.Mod(stats.getSpeed()) >= serverData.subroundNumber - 1)
                {
                    pm.canMoveThisSubround = true;
                    pm.RpcStartRound();
                } else
                {
                    pm.canMoveThisSubround = false;
                }
            }
            if (serverData.subroundNumber != 1)
            {
                serverData.subroundNumber -= 1;
            }
            else
            {
                serverData.roundNumber += 1;
                serverData.subroundNumber = Stats.maxSpdMod;
                foreach (GameObject player in players.Values)
                {
                    PlayerMovement pm = player.GetComponent<PlayerMovement>();
                    pm.canMoveThisSubround = Stats.Mod(pm.GetComponent<Stats>().getSpeed()) >= serverData.subroundNumber;
                    pm.RpcStartRound();
                }

                //To incrememt the values of monster
                PlayerMovement.localPlayer.GetComponent<Stats>().RpcDisplayMonsterLevelUp();
            }
        }
	}

    IEnumerator DelayedLightUpdate(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);

        obj.transform.FindChild("2DLightEx").GetComponent<MeshRenderer>().enabled = true;
        obj.transform.FindChild("2DLightEx").GetComponent<DynamicLight2D.DynamicLight>().StaticUpdate();
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

    public void addBattle(GameObject playerOne, GameObject playerTwo, bool isRanged)
    {
        battleSet.Add(new Battle(playerOne, playerTwo, isRanged, this));
    }

    bool wait = false;

    IEnumerator Wait(float t)
    {
        yield return new WaitForSeconds(t);
        wait = false;
    }

    bool readyToProcess()
    {
        if (players.Count <= 0 || wait)
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
        wait = true;
        StartCoroutine(Wait(0.5f));

        print("Ready to process on server!");
        return true;
    }

    public void makeBullet(Vector2 origin, Vector2 target, uint t1, uint t2)
    {
        foreach (GameObject player in players.Values) {
            player.GetComponent<ClientRoundController>().RpcSpawnLocalBullet(origin, target, t1, t2);
        }
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
        //print("Added player to players in room.");
        int indexInRoomList = playersInRoom.LastIndexOf(player);
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
        spawnPlayer(player);
        playerJoin(player);
    }

    public void spawnPlayer(GameObject player)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();

        if (PlayerMovement.localPlayer == player)
        {
            //position
            pm.isWerewolf = true;
            pm.roomPosition = new Vector2(11,8);
            pm.transform.GetComponent<Stats>().set(2,2,2,2);

            //sprite
            pm.transform.GetComponent<SpriteRenderer>().sprite = playerSprites[0];
        }
        else
        {
            //position
            int i = Random.Range(0, mySpawnPoints.Count);
            pm.roomPosition = mySpawnPoints[i];
            //todo uncomment below, just for testing
            mySpawnPoints.RemoveAt(i);

            //sprite
            i = Random.Range(1, playerSprites.Count);
            pm.transform.GetComponent<SpriteRenderer>().sprite = playerSprites[i];
            playerSprites.RemoveAt(i);

            pm.transform.GetComponent<Stats>().set(4, 4, 4, 4);
        }

        //todo identify bug
        pm.RpcSetCamera(pm.roomPosition);
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
    private bool isRanged;
    private ServerRoundController src;

    public Battle(GameObject p1, GameObject p2, bool ranged, ServerRoundController src)
    {
        playerOne = p1;
        playerTwo = p2;
        isRanged = ranged;
        this.src = src;
    }

    public void sendAnimations()
    {
        PlayerMovement pmOne = playerOne.GetComponent<PlayerMovement>();
        PlayerMovement pmTwo = playerTwo.GetComponent<PlayerMovement>();
        pmOne.isAttacking = true;
        pmTwo.isAttacking = true;
        if (!isRanged)
        {
            pmOne.rangedAttack = false;
            pmTwo.rangedAttack = false;
            pmOne.attackAnimationTarget = (playerOne.transform.position + playerTwo.transform.position) / 2;
            pmTwo.attackAnimationTarget = pmOne.attackAnimationTarget;
        } else
        {
            pmOne.rangedAttack = true;
            pmTwo.rangedAttack = true;
            pmOne.attackAnimationTarget = playerOne.transform.position;
            pmTwo.attackAnimationTarget = playerTwo.transform.position;
            src.makeBullet(playerOne.transform.position, playerTwo.transform.position, pmOne.netId.Value, pmTwo.netId.Value);
        }
        
    }

    public void process()
    {
        Stats playerOneStats = playerOne.GetComponent<Stats>();
        Stats playerTwoStats = playerTwo.GetComponent<Stats>();
        if (!isRanged)
        {
            int playerOneRoll = roll(playerOneStats.getMight());
            int playerTwoRoll = roll(playerTwoStats.getMight());
            int statLoss = calculateStatLoss(playerOneRoll, playerTwoRoll);
            Debug.Log("Player " + playerOne.GetComponent<NetworkIdentity>().netId + " rolled " + playerOneRoll);
            Debug.Log("Player " + playerTwo.GetComponent<NetworkIdentity>().netId + " rolled " + playerTwoRoll);
            if (playerOneRoll > playerTwoRoll)
            {
                Debug.Log("Player " + playerOne.GetComponent<NetworkIdentity>().netId + " won the fight!");
                playerTwoStats.gainMight(statLoss);
            }
            else
            {
                Debug.Log("Player " + playerTwo.GetComponent<NetworkIdentity>().netId + " won the fight!");
                playerOneStats.gainMight(statLoss);
            }
        } else
        {
            int playerOneRoll = roll(playerOneStats.getSpeed());
            int playerTwoRoll = roll(playerTwoStats.getSpeed());
            Debug.Log("Player " + playerOne.GetComponent<NetworkIdentity>().netId + " rolled " + playerOneRoll);
            Debug.Log("Player " + playerTwo.GetComponent<NetworkIdentity>().netId + " rolled " + playerTwoRoll);
            if (playerOneRoll > playerTwoRoll)
            {
                playerTwoStats.loseHighest(1);
            }
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
