using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSpeedBoostItem : Item {
    public bool canUse()
    {
        return true;
    }

    public bool discardOnUse()
    {
        return true;
    }

    public string getName()
    {
        return "SpdDrug";
    }

    //Bad solution
    private List<int> tmpEXBoost = new List<int>();

    void removeStats(GameObject user)
    {
        Stats stats = user.GetComponent<Stats>();

        stats.gainSpeed(-tmpEXBoost[0]);
        tmpEXBoost.RemoveAt(0);
        stats.gainSpeed(-tmpEXBoost[0]);
        tmpEXBoost.RemoveAt(0);

        stats.CmdUpdateStatsToQueued();
        stats.RpcUpdateStats();
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used Speed item.");
        Stats stats = user.GetComponent<Stats>();

        int curVal = Stats.Mod(stats.getSpeed());

        tmpEXBoost.Add(curVal);
        stats.gainSpeed(tmpEXBoost[tmpEXBoost.Count - 1]);
        tmpEXBoost.Add(curVal+1);
        stats.gainSpeed(tmpEXBoost[tmpEXBoost.Count - 1]);

        src.addServerEvent(1, user, removeStats);
        //user.GetComponent<PlayerMovement>().itemDelay = 1;
    }
}
