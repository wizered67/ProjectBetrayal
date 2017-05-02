using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour {
    public int defaultZ;
    public float lerpRate;
    //The move the local player will make once told by server to.
    public Vector2 currentMove = new Vector2(0, 0);
    //position in room coordinates, process on server
    [SyncVar]
    public Vector2 roomPosition = new Vector2(0, 0);
    [SyncVar]
    public Vector2 internalPosition = new Vector2(0, 0);
    public GameObject attackTarget = null;
    public float roomSize;
    public GameObject nextMovePrefab;
    private GameObject nextMoveMarker;
    private ClientRoundController roundController;
    public WorldController worldController;
    private GameObject text;

    //whether can't move because of item. Set to 1 for 1 subround temp or 2 for full round delay.
    [SyncVar]
    public int itemDelay = 0;

    [SyncVar]
    public Vector2 attackAnimationTarget = new Vector2(0, 0);
    [SyncVar]
    public bool isAttacking = false;
    [SyncVar]
    public bool rangedAttack = false;

    //Public Vars for setting player images
    public List<Sprite> playerSprites = new List<Sprite>();

    [SyncVar(hook = "SpriteSet")]
    public int playerNum = 0;

    public void SetPlayerNum(int val)
    {
        CmdSetPlayerNum(val);
    }

    [Command]
    void CmdSetPlayerNum(int val)
    {
        playerNum = val;
    }

    public void SpriteSet(int val)
    {
        GetComponent<SpriteRenderer>().sprite = playerSprites[val];
        playerNum = val;
    }

    public float shootingRange
    {
        get
        {
            return (Stats.Mod(GetComponent<Stats>().getSanity()) * 8) / 1.5f;
        }
    }

    [SyncVar]
    public bool canMoveThisSubround;

    //for local only, sorry for the shoddy coding
    public ServerDataManager serverData;
    public static GameObject localPlayer;
    //me too
    public bool openingDoor = false;

    [SyncVar]
    public bool isWerewolf = false;

    // Use this for initialization
    void Start ()
    {
        GetComponent<SpriteRenderer>().sprite = playerSprites[playerNum];

        if (isLocalPlayer)
        {
            worldController = GameObject.Find("RoomManager").GetComponent<WorldController>();
            worldController.makeWorld();
            CmdSetupPlayerForWerewolf();
        }
    }

    [ClientRpc]
    public void RpcSetCamera(Vector2 rmPos)
    {
        if (isLocalPlayer)
        {
            print("Starting room position is " + rmPos);
            //print("World controller is " + worldController);
            Debug.Log("Setting Cam pos");
           // if (worldController != null)
           // {
            Vector2 worldPos = WorldController.getWorldCoordinates(rmPos);
            Camera.main.transform.position = new Vector3(worldPos.x, worldPos.y, Camera.main.transform.position.z);
            Transform marker = PlayerMovement.localPlayer.GetComponent<PlayerMovement>().nextMoveMarker.transform;
            marker.position = new Vector3(worldPos.x, worldPos.y, marker.position.z);
            // }
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        localPlayer = gameObject;
        text = GameObject.Find("Text");
        setTimerText("Press Enter to Start");
        roundController = GetComponent<ClientRoundController>();
        nextMoveMarker = Instantiate(nextMovePrefab);
        GameObject.Find("Main Camera").GetComponent<CameraController>().setPlayer(gameObject);
        transform.position = new Vector3(transform.position.x, transform.position.y, defaultZ);
        //todo actually check if is werewolf, but make sure it is set on server side by this point.
        //may need to have server call Rpc for werewolf.

        //Made Obsulete by abilities
        if (isServer)
         {
             GameObject.Find("RenderingObjs").transform.FindChild("MansionBeta").gameObject.SetActive(false);
             GameObject.Find("RenderingObjs").transform.FindChild("Contour").GetComponent<SpriteRenderer>().color = new Color(0.6f,0.4f,0.4f);
         }
         else
         {
        //Turn on the local Light source
        transform.FindChild("2DLightEx").gameObject.SetActive(true);
        }
    }

    [Command]
    void CmdSetupPlayerForWerewolf()
    {
        //Set LOS for bloodscent
        if (PlayerMovement.localPlayer != gameObject)
        {
            Transform lt = transform.FindChild("2DLightEx");
            lt.GetComponent<DynamicLight2D.DynamicLight>().isStatic = true;
            //lt.GetComponent<DynamicLight2D.DynamicLight>().StaticUpdate();
            lt.gameObject.SetActive(true);
            lt.GetChild(0).gameObject.SetActive(false);
        }
    }

    //Timer started once the round begins for this player. If time runs out, tell the server you've selected a move,
    //even if you haven't so that processing begins.
    IEnumerator MoveTimer()
    {
        float totsTime = 5.5f - serverData.subroundNumber * 0.5f;

        // suspend execution for 5 seconds
        for (float i = 0; i < (totsTime*10); i += 1) {
            if (itemDelay == 0)
            {
                setTimerText((totsTime - (i/10)).ToString("F1"));
            }
            yield return new WaitForSeconds(0.1f);
        }
        roundController.sendMove();
    }


    void setTimerText(string timeString)
    {
        text.GetComponent<UnityEngine.UI.Text>().text = timeString;
    }
   
    // Update is called once per frame
    void Update () {
        Vector2 targetPosition = WorldController.getWorldCoordinates(roomPosition) + internalPosition;
        //(isLocalPlayer ? Vector2.zero : internalPosition);
        float rate = lerpRate;
        if (isAttacking)
        {
            Vector2 position2d = new Vector2(transform.position.x, transform.position.y);
            if (!rangedAttack && Vector2.Distance(position2d, attackAnimationTarget) < 0.05)
            {
                isAttacking = false;
                if (isServer) { RpcMeeleAttackAudio(); }
                GetComponent<Stats>().CmdUpdateStatsToQueued();
            } else
            {
                targetPosition = attackAnimationTarget;
            }
        }
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), rate);
        if (isLocalPlayer)
        {
            localUpdate();
        }
        if (isServer)
        {
            serverUpdate();
        }
	}

    [ClientRpc]
    void RpcMeeleAttackAudio()
    {
        transform.FindChild("DamageText").GetComponent<AudioSource>().Play();
    }

    public bool hasStarted = false;

    //Update for the local player. If this player has started the round, then start getting input. If there is new input,
    //tell the server that you've selected a move.
    void localUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GetComponent<Stats>().CmdGainDiscoveryProgress(5);

            if (isWerewolf && !hasStarted)
            {
                foreach (PlayerMovement pm in FindObjectsOfType(typeof(PlayerMovement)))
                {
                    pm.canMoveThisSubround = Stats.Mod(pm.transform.GetComponent<Stats>().getSpeed()) == Stats.maxSpdMod;
                    pm.RpcStartRound(pm.roomPosition);
                }

                hasStarted = true;
            }
        }
        //temp cheating
        /*
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetComponent<Stats>().CmdUseItem(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GetComponent<Stats>().CmdUseItem(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetComponent<Stats>().CmdUseItem(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GetComponent<Stats>().CmdUseItem(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            GetComponent<Stats>().CmdUseItem(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            GetComponent<Stats>().CmdUseItem(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            GetComponent<Stats>().CmdUseItem(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            GetComponent<Stats>().CmdUseItem(7);
        }*/
        /*if (Input.GetKeyDown(KeyCode.C))
        {

            GetComponent<Stats>().gainHealth(-1);
        }*/

        if (canMoveThisSubround)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                setDestination(new Vector2(roomPosition.x, roomPosition.y + 1));
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                setDestination(new Vector2(roomPosition.x, roomPosition.y - 1));
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                setDestination(new Vector2(roomPosition.x - 1, roomPosition.y));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                setDestination(new Vector2(roomPosition.x + 1, roomPosition.y));
            }
        }

        if (serverData == null || !canMoveThisSubround || itemDelay > 0)
        {
            if (!canMoveThisSubround)
            {
                if (!isServer || hasStarted)
                {
                    setTimerText("WAITING");
                }
            }
            if (itemDelay > 0)
            {
                setTimerText("Using Item");
            }
            nextMoveMarker.GetComponent<SpriteRenderer>().enabled = false;
            ResetMove();

            return;
        }
        else
        {
            nextMoveMarker.GetComponent<SpriteRenderer>().enabled = true;
            currentMove = WorldController.getRoomCoordinates(nextMoveMarker.transform.position) - roomPosition;
        }
    }

    void ResetMove()
    {
        currentMove.Set(0, 0);
        nextMoveMarker.transform.position =
            new Vector3
            (
                WorldController.getWorldCoordinates(roomPosition).x,
                WorldController.getWorldCoordinates(roomPosition).y,
                nextMoveMarker.transform.position.z
            );
    }

    public void setDestination(Vector2 dest)
    {
        if (!canMoveThisSubround)
        {
            return;
        }
        Vector2 intendedMovement = dest - roomPosition;
        if (isValidMove(intendedMovement))
        {
            attackTarget = null;
            currentMove = intendedMovement;
            Vector2 newWorldPos = WorldController.getWorldCoordinates(roomPosition + intendedMovement);
            nextMoveMarker.transform.position = new Vector3(newWorldPos.x, newWorldPos.y, -3);
            print("set move marker position");
        }
    }

    void serverUpdate()
    {
    }
    
    //Message from the server to this client that the round has been started. Once received, the timer must start.
    [ClientRpc]
    public void RpcStartRound(Vector2 newRoom)
    {
        if (isLocalPlayer)
        {
            print("Started timer.");
            StartCoroutine("MoveTimer");
            attackTarget = null;
            currentMove.Set(0, 0);
            nextMoveMarker.transform.position =
                new Vector3
                (
                    WorldController.getWorldCoordinates(newRoom).x,
                    WorldController.getWorldCoordinates(newRoom).y,
                    nextMoveMarker.transform.position.z
                );
        }
    }
    //Message from the server that moves are being processed. If this is the local player, make moves, reset the next
    //move to a pass, and stop the timer if it's still going. Then, tell the server that this client has processed their
    //move.
    [ClientRpc]
    public void RpcMove()
    {
        if (isLocalPlayer)
        {
            print("Processing this player's move on local client!");
            //gameObject.transform.Translate(currentMove.x * roomSize, currentMove.y * roomSize, 0);
            //CmdSetRoomPosition(roomPosition + currentMove);
            
            //currentMove.Set(0, 0);
            StopCoroutine("MoveTimer");
            if (attackTarget != null)
            {
                //change target position here
            }
            attackTarget = null;

        }
    }

    [ClientRpc]
    public void RpcPlayDoorSound()
    {
        if (isLocalPlayer)
        {
            if (openingDoor)
            {
                print("Playing door sound.");
                AudioController.Play("DoorShort");
                openingDoor = false;
            }
        }
    }

    //server side only - actually processes move
    public void processMove()
    {
        print("Processing move!");
        roomPosition += currentMove;
       // worldController.getRoom((int)roomPosition.x,(int) roomPosition.y).GetComponent<RoomData>().hasBeenSeen = true;
    }
    /* //No longer needed now that moves processed server side
    [Command]
    void CmdSetRoomPosition(Vector2 newPosition)
    {
        roomPosition = newPosition;
        ServerRoundController src = ClientRoundController.serverData.GetComponent<ServerRoundController>();
        int numPlayersInRoom = src.numPlayersInRoom(roomPosition);
        internalPosition = worldController.getRoom((int)newPosition.x, (int)newPosition.y)
            .GetComponent<RoomData>().getInternalPosition(numPlayersInRoom - 1).localPosition;
    } */
    [Command]
    void CmdSetRoomPosition(Vector2 newPosition)
    {
        roomPosition = newPosition;
    }

    public GameObject getAttackTarget()
    {
        return attackTarget;
    }

    public void setAttackTarget(GameObject target)
    {
        if (!canMoveThisSubround)
        {
            return;
        }
        if (!canLocalPlayerAttack(target))
        {
            return;
        }
        print("attack target set.");
        attackTarget = target;
        //currentMove.Set(0, 0);
        ResetMove();
    }

    void OnMouseDown()
    {
        print("Clicked player.");
        if (gameObject != PlayerMovement.localPlayer)
        {
            PlayerMovement.localPlayer.GetComponent<PlayerMovement>().setAttackTarget(gameObject);
        } else
        {
            //todo reset movement probably
        }
    }

    void OnMouseEnter()
    {
        print("Hovered a player.");
        if (!canLocalPlayerAttack(gameObject))
        {
            return;
        }
        if (gameObject != localPlayer)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public bool canLocalPlayerAttack(GameObject target)
    {
        PlayerMovement targetPm = target.GetComponent<PlayerMovement>();
        PlayerMovement localPm = localPlayer.GetComponent<PlayerMovement>();
        if (!localPm.canMoveThisSubround || localPm.itemDelay > 0)
        {
            print("local player can't attack because they can't move.");
            return false;
        }
        return target != localPlayer && (targetPm.roomPosition == localPm.roomPosition || canLocalPlayerRangedAttack(target)); 
    }

    public bool canLocalPlayerRangedAttack(GameObject target)
    {
        PlayerMovement localPm = localPlayer.GetComponent<PlayerMovement>();
        PlayerMovement targetPm = target.GetComponent<PlayerMovement>();
        if (localPm.isWerewolf)
        {
            print("local player can't ranged attack because they are the werewolf.");
            return false;
        }
        Vector2 playerRoomPosition = WorldController.getWorldCoordinates(localPm.roomPosition);
        Vector2 targetRoomPosition = WorldController.getWorldCoordinates(targetPm.roomPosition);
        float distance = (playerRoomPosition - targetRoomPosition).magnitude;
        if (distance > shootingRange)
        {
            print(shootingRange);
            print("local player can't ranged attack because the distance is too large. Distance is " + distance);
            return false;
        }
        int layerMask = ~(1 << 9);
        RaycastHit2D hit = Physics2D.Raycast(playerRoomPosition, targetRoomPosition - playerRoomPosition, distance, layerMask);
        if (hit.collider != null)
        {
            print("local player can't ranged attack because they hit something.");
            return false;
        }
        return true;
    }

    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    public VisibilityStatus canSee(RoomData room)
    {
        Vector2 distance = new Vector2(room.roomX, room.roomY) - roomPosition;
        if (distance.magnitude <= 1)
        {
            return VisibilityStatus.VISIBLE;
        } else if (room.hasBeenSeen)
        {
            return VisibilityStatus.FADED;
        } else
        {
            return VisibilityStatus.HIDDEN;
        }
    }

    //Checks whether a move is valid, ie there's a door to go through.
    public bool isValidMove(Vector2 move)
    {
        if (!canMoveThisSubround || itemDelay > 0 || !(move.magnitude <= 1))
        {
            return false;
        }
        Vector2 newWorldPosition = WorldController.getWorldCoordinates(roomPosition);

        RaycastHit2D hit = Physics2D.Raycast(newWorldPosition, move, 8);

        openingDoor = false;

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.tag != "Door")
            {
                return false;
            }
            else
            {
                openingDoor = true;
            }
        }
        return true;
        /*
        if (move.magnitude > 1)
        {
            print("Invalid move - magnitude is " + move.magnitude);
            return false;
        }
        int newX = (int)roomPosition.x + (int) move.x;
        int newY = (int)roomPosition.y + (int)move.y;
        GameObject[,] rooms = worldController.rooms;
        print("Attempting to move to " + newX + ", " + newY);
        return newX >= 0 && newY >= 0 && newX < worldController.worldWidth && newY < worldController.worldHeight && rooms[newX, newY] != null
        */
    }
    
}
