﻿using System.Collections;
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
    GameObject attackTarget = null;
    public float roomSize;
    public GameObject nextMovePrefab;
    private GameObject nextMoveMarker;
    private ClientRoundController roundController;
    private WorldController worldController;
    private GameObject text;

    [SyncVar]
    public Vector2 attackAnimationTarget = new Vector2(0, 0);
    [SyncVar]
    public bool isAttacking = false;

    [SyncVar]
    public bool canMoveThisSubround;

    //for local only, sorry for the shoddy coding
    public ServerDataManager serverData;
    public static GameObject localPlayer;
    //me too
    public bool openingDoor = false;

    // Use this for initialization
    void Start () {
        worldController = GameObject.Find("RoomManager").GetComponent<WorldController>();
        worldController.makeWorld();
        CmdSetRoomPosition(new Vector2(11, 8));
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        localPlayer = gameObject;
        text = GameObject.Find("Text");
        roundController = GetComponent<ClientRoundController>();
        nextMoveMarker = Instantiate(nextMovePrefab);
        GameObject.Find("Main Camera").GetComponent<CameraController>().setPlayer(gameObject);
        transform.position = new Vector3(transform.position.x, transform.position.y, defaultZ);

        //Turn on the local Light source
        transform.FindChild("2DLightEx").gameObject.SetActive(true);
    }
    //Timer started once the round begins for this player. If time runs out, tell the server you've selected a move,
    //even if you haven't so that processing begins.
    IEnumerator MoveTimer()
    {
        // suspend execution for 5 seconds
        for (int i = 0; i < 5; i += 1) {
            setTimerText("" + (5 - i));
            yield return new WaitForSeconds(1);
        }
        roundController.sendMove();
    }


    void setTimerText(string timeString)
    {
        text.GetComponent<UnityEngine.UI.Text>().text = timeString;
    }
   
    // Update is called once per frame
    void Update () {
        
        Vector2 targetPosition = worldController.getWorldCoordinates(roomPosition) + internalPosition;
        //(isLocalPlayer ? Vector2.zero : internalPosition);
        float rate = lerpRate;
        if (isAttacking)
        {
            Vector2 position2d = new Vector2(transform.position.x, transform.position.y);
            if (Vector2.Distance(position2d, attackAnimationTarget) < 0.05)
            {
                isAttacking = false;
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
    //Update for the local player. If this player has started the round, then start getting input. If there is new input,
    //tell the server that you've selected a move.
    void localUpdate()
    {
        if (serverData == null || !canMoveThisSubround)
        {
            if (!canMoveThisSubround)
            {
                setTimerText("WAITING");
            }
            return;
        }

        Vector2 intendedMovement = currentMove;
        float horiz = Input.GetKeyDown("right") ? 1 : (Input.GetKeyDown("left") ? -1 : 0);
        float vert = Input.GetKeyDown("up") ? 1 : (Input.GetKeyDown("down") ? -1 : 0);
        bool changed = false;
        float intendedX = intendedMovement.x;
        float intendedY = intendedMovement.y;
        if (horiz > 0)
        {
            intendedMovement.Set(Mathf.Min(1, intendedX + 1), 0);
            changed = true;
        }
        else if (horiz < 0)
        {
            intendedMovement.Set(Mathf.Max(-1, intendedX - 1), 0);
            changed = true;
        }
        else if (vert > 0)
        {
            intendedMovement.Set(0, Mathf.Min(1, intendedY + 1));
            changed = true;
        }
        else if (vert < 0)
        {
            intendedMovement.Set(0, Mathf.Max(-1, intendedY - 1));
            changed = true;
        }
        if (changed && isValidMove(intendedMovement))
        {
            currentMove = intendedMovement;
            Vector2 newWorldPos = worldController.getWorldCoordinates(roomPosition + intendedMovement);
            nextMoveMarker.transform.position = new Vector3(newWorldPos.x, newWorldPos.y, -3);
            print("set move marker position");
            //roundController.CmdSentMove();
        }
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
            currentMove = intendedMovement;
            Vector2 newWorldPos = worldController.getWorldCoordinates(roomPosition + intendedMovement);
            nextMoveMarker.transform.position = new Vector3(newWorldPos.x, newWorldPos.y, -3);
            print("set move marker position");
        }
    }

    void serverUpdate()
    {
        
    }
    
    //Message from the server to this client that the round has been started. Once received, the timer must start.
    [ClientRpc]
    public void RpcStartRound()
    {
        if (isLocalPlayer)
        {
            print("Started timer.");
            StartCoroutine("MoveTimer");
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
            
            currentMove.Set(0, 0);
            StopCoroutine("MoveTimer");
            if (attackTarget != null)
            {
                //change target position here
            }
            attackTarget = null;

            //

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
        if (target.GetComponent<PlayerMovement>().roomPosition != roomPosition)
        {
            return;
        }
        print("attack target set.");
        attackTarget = target;
        currentMove.Set(0, 0);
        Vector2 newWorldPos = worldController.getWorldCoordinates(roomPosition);
        nextMoveMarker.transform.position = new Vector3(newWorldPos.x, newWorldPos.y * roomSize, -3);
    }

    void OnMouseDown()
    {
        print("Clicked player.");
        if (gameObject != PlayerMovement.localPlayer)
        {
            PlayerMovement.localPlayer.GetComponent<PlayerMovement>().setAttackTarget(gameObject);
        }
    }

    void OnMouseEnter()
    {
        if (localPlayer.GetComponent<PlayerMovement>().roomPosition != roomPosition)
        {
            return;
        }
        if (gameObject != localPlayer)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
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
        Vector2 newWorldPosition = worldController.getWorldCoordinates(roomPosition);

        RaycastHit2D hit = Physics2D.Raycast(newWorldPosition, move, 8);

        openingDoor = false;

        if (!canMoveThisSubround || (hit.collider != null) || !(move.magnitude <= 1))
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
