using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerEvent {
    private int roundsRemaining;
    private GameObject target;
    public delegate void Action(GameObject target);
    Action myAction;
    bool started = false;
    int startRound;
    int startSubround;
    public ServerEvent(int rounds, GameObject player, Action executeEvent)
    {
        roundsRemaining = rounds;
        target = player;
        myAction = executeEvent;
    }
    public bool eventUpdate(int roundNumber, int subroundNumber)
    {
        if (!started)
        {
            started = true;
            startRound = roundNumber;
            startSubround = subroundNumber;
        } else
        {
            Debug.Log("Start round = " + startRound + ", start sub = " + startSubround + ", current = " + roundNumber + "." + subroundNumber);
            if (roundNumber > startRound && (subroundNumber == startSubround || (subroundNumber < startSubround && roundsRemaining == 1)))
            {
                roundsRemaining -= 1;
            }
        }

        Debug.Log("Rounds remaining: " + roundsRemaining);
        if (roundsRemaining <= 0)
        {
            myAction(target);
            return true;
        }
        return false;
    }
}
