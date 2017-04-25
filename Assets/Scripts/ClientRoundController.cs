using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ClientRoundController : NetworkBehaviour {
    //Server side variable for each client. Whether it has sent a move yet.
    public bool sentMove = false;

    public GameObject bulletPrefab;
    
    //Reference to PlayerMovement, used for locally handling movement.
    public PlayerMovement playerMovement;
    public GameObject serverDataPrefab;
    private Stats stats;
    public static ServerDataManager serverData;
    public static ServerRoundController serverRoundController;
    public static bool init = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
    }


    // Use this for initialization
    void Start ()
    {
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
            //print(gameObject);
            serverRoundController.addPlayer(netId, gameObject);
        }
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        serverRoundController.removePlayer(netId);
    }

    // Update is called once per frame
    void Update ()
    {
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
        //print("can move is " + playerMovement.canMoveThisSubround);
        //print("sent move is " + sentMove);
        return !playerMovement.canMoveThisSubround || sentMove;
    }

    //Update that only happens if this is the local player.
    void localUpdate()
    {
        //Setting Local View Range
        if (stats.isReady())
        {
            transform.FindChild("2DLightEx").localScale = new Vector3(Stats.Mod(stats.getSanity()) * 0.333f, Stats.Mod(stats.getSanity()) * 0.333f, 1f);
        } else
        {
            transform.FindChild("2DLightEx").localScale = Vector3.zero;
        }
        
    }
    //Update that only happens if this is on the server. Run on each client gameobject, but only on server side.
    //If there are remaining rounds but this client has not started one, start it and tell them to start their timer.
    //If processing has already begun or all moves have been received, then start processing and tell the player associated
    //with this client to act out their move. Decrements number of moves and only continues processing while it's above 0,
    //so that each client only has moves processed once.
    void serverUpdate()
    {
        //Setting Server View Range
        /*if (stats.isReady() && !isLocalPlayer)
        {
            transform.FindChild("2DLightEx").localScale = new Vector3(stats.getSanity() * 0.333f, stats.getSanity() * 0.333f, 1f);
        }*/
    }

    //client side code to send a move
    public void sendMove()
    {
        GameObject attackTarget = playerMovement.getAttackTarget();
        if (attackTarget == null)
        {
            CmdSentMove(playerMovement.currentMove);
        } else
        {
            print("Sent request to run attack command.");
            bool isRanged = false;
            if (playerMovement.roomPosition != attackTarget.GetComponent<PlayerMovement>().roomPosition)
            {
                isRanged = true;
            }
            CmdAttack(gameObject, attackTarget, isRanged);
        }
    }
    
    [ClientRpc]
    public void RpcSpawnLocalBullet(Vector2 origin, Vector2 target, uint t1, uint t2)
    {
        GetComponent<AudioSource>().Play();

        if (isLocalPlayer)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            BulletMovement bm = bullet.GetComponent<BulletMovement>();
            bm.setParticipants(t1, t2);
            bm.origin = new Vector3(origin.x, origin.y, transform.position.z);
            bm.transform.position = bm.origin;
            bm.target = new Vector3(target.x, target.y, transform.position.z);
            float angle = Mathf.Atan2(bm.target.y - bm.origin.y, bm.target.x - bm.origin.x) * Mathf.Rad2Deg + 90;
            bm.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    [Command]
    public void CmdSentMove(Vector2 move)
    {
        if (!sentMove)
        {
            Debug.Log("Sent: "+isLocalPlayer);
            Debug.Log(move);
            playerMovement.currentMove = move;
            sentMove = true;
        }
    }

    [Command]
    public void CmdAttack(GameObject playerOne, GameObject playerTwo, bool isRanged)
    {
        print("Running attack command.");
        playerMovement.currentMove = Vector2.zero;
        serverRoundController.addBattle(playerOne, playerTwo, isRanged);
        sentMove = true;
    }

}
