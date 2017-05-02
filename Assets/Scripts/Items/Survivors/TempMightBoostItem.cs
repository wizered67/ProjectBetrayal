using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMightBoostItem : Item {
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
        return "MgtDrug";
    }

    //Bad solution
    private List<int> tmpEXBoost = new List<int>();

    void removeStats(GameObject user)
    {
        Stats stats = user.GetComponent<Stats>();

        stats.gainMight(-tmpEXBoost[0]);
        tmpEXBoost.RemoveAt(0);
        stats.gainMight(-tmpEXBoost[0]);
        tmpEXBoost.RemoveAt(0);

        stats.CmdUpdateStatsToQueued();
        stats.RpcUpdateStats();
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used Might item.");
        Stats stats = user.GetComponent<Stats>();

        int curVal = Stats.Mod(stats.getMight());

        tmpEXBoost.Add(curVal);
        stats.gainMight(tmpEXBoost[tmpEXBoost.Count - 1]);
        tmpEXBoost.Add(curVal+1);
        stats.gainMight(tmpEXBoost[tmpEXBoost.Count - 1]);

        src.addServerEvent(1, user, removeStats);
        //user.GetComponent<PlayerMovement>().itemDelay = 1;
    }
}
