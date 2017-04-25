using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempIntelligenceBoostItem : Item {
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
        return "Adderall";
    }

    //Bad solution
    private List<int> tmpEXBoost = new List<int>();

    void removeStats(GameObject user)
    {
        Stats stats = user.GetComponent<Stats>();

        stats.gainIntelligence(-tmpEXBoost[0]);
        tmpEXBoost.RemoveAt(0);

        stats.CmdUpdateStatsToQueued();
        stats.RpcUpdateStats();
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used intelligence item.");
        Stats stats = user.GetComponent<Stats>();

        tmpEXBoost.Add(Stats.Mod(stats.getIntelligence()));
        stats.gainIntelligence(tmpEXBoost[tmpEXBoost.Count-1]);

        src.addServerEvent(1, user, removeStats);
        //user.GetComponent<PlayerMovement>().itemDelay = 1;
    }
}
