using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour {
    //The move the local player will make once told by server to.
    public Vector2 currentMove = new Vector2(0, 0);
    private RoundController roundController;
    private GameObject text;
    [SyncVar]
    public bool canMoveThisSubround;
    public ServerData serverData;
    public static GameObject localPlayer;

	// Use this for initialization
	void Start () {
        
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        localPlayer = gameObject;
        text = GameObject.Find("Text");
        roundController = GetComponent<RoundController>();
    }
    //Timer started once the round begins for this player. If time runs out, tell the server you've selected a move,
    //even if you haven't so that processing begins.
    IEnumerator MoveTimer()
    {
        // suspend execution for 5 seconds
        for (int i = 0; i < 5; i += 1) {
            text.GetComponent<UnityEngine.UI.Text>().text = "" + (5 - i);
            yield return new WaitForSeconds(1);
        }
        
        roundController.CmdSentMove();
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
    //Update for the local player. If this player has started the round, then start getting input. If there is new input,
    //tell the server that you've selected a move.
    void localUpdate()
    {
        if (serverData == null || roundController.startedSubround < serverData.subroundNumber || !canMoveThisSubround)
        {
            return;
        }
        Vector2 intendedMovement = currentMove;
        float horiz = Input.GetKeyDown("right") ? 1 : (Input.GetKeyDown("left") ? -1 : 0);
        float vert = Input.GetKeyDown("up") ? 1 : (Input.GetKeyDown("down") ? -1 : 0);
        bool changed = false;
        if (horiz > 0)
        {
            intendedMovement.Set(1, 0);
            changed = true;
        }
        else if (horiz < 0)
        {
            intendedMovement.Set(-1, 0);
            changed = true;
        }
        else if (vert > 0)
        {
            intendedMovement.Set(0, 1);
            changed = true;
        }
        else if (vert < 0)
        {
            intendedMovement.Set(0, -1);
            changed = true;
        }
        if (changed && isValidMove(intendedMovement))
        {
            currentMove = intendedMovement;
            roundController.CmdSentMove();
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
            gameObject.transform.Translate(currentMove.x, currentMove.y, 0);
            roundController.CmdPostMove();
            currentMove.Set(0, 0);
            StopCoroutine("MoveTimer");
        }
    }

    //Checks whether a move is valid, ie there's a door to go through. For prototype, always true
    bool isValidMove(Vector2 move)
    {
        return true;
    }
}
