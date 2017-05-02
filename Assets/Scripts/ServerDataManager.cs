using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerDataManager : NetworkBehaviour {
    //todo figure out how to spawn ServerData on server start!
    public int numRoundsRemaining = 5;
    [SyncVar(hook ="changeRound")]
    public int roundNumber = 1;
    [SyncVar(hook ="changeSubround")]
    public int subroundNumber = 5;
    private GameObject roundText;
    // Use this for initialization
    void Start () {
        roundText = GameObject.Find("RoundText");
        updateRoundText();
        PlayerMovement.localPlayer.GetComponent<PlayerMovement>().serverData = this;
        PlayerMovement.localPlayer.GetComponent<Stats>().serverData = this;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void changeRound(int newRound)
    {
        roundNumber = newRound;
        updateRoundText();
    }

    void changeSubround(int newSubround)
    {
        subroundNumber = newSubround;
        updateRoundText();
    }

    void updateRoundText()
    {
        roundText.GetComponent<UnityEngine.UI.Text>().text = "Speed Priority: " + subroundNumber;// + "/" + Stats.maxSpdMod;
    }
}
