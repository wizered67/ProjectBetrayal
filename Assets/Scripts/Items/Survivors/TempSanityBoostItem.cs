using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSanityBoostItem : Item {
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
        return "PerDrug";
    }

    //Bad solution
    private List<int> tmpEXBoost = new List<int>();

    void removeStats(GameObject user)
    {
        Stats stats = user.GetComponent<Stats>();

        stats.gainSanity(-tmpEXBoost[0]);
        Debug.Log(-tmpEXBoost[0]);
        tmpEXBoost.RemoveAt(0);
        stats.gainSanity(-tmpEXBoost[0]);
        Debug.Log(-tmpEXBoost[0]);
        tmpEXBoost.RemoveAt(0);

        stats.CmdUpdateStatsToQueued();
        stats.RpcUpdateStats();
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used Speed item.");
        Stats stats = user.GetComponent<Stats>();

        int curVal = Stats.Mod(stats.getSanity());

        tmpEXBoost.Add(curVal);
        stats.gainSanity(tmpEXBoost[tmpEXBoost.Count - 1]);
        tmpEXBoost.Add(curVal+1);
        stats.gainSanity(tmpEXBoost[tmpEXBoost.Count - 1]);

        src.addServerEvent(1, user, removeStats);
        //user.GetComponent<PlayerMovement>().itemDelay = 1;
    }
}
