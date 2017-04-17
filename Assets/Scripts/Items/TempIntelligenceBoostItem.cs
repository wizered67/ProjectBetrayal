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

    void removeStats(GameObject user)
    {
        Stats stats = user.GetComponent<Stats>();
        stats.gainIntelligence(-8);
        stats.CmdUpdateStatsToQueued();
        stats.RpcUpdateStats();
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used intelligence item.");
        Stats stats = user.GetComponent<Stats>();
        stats.gainIntelligence(8);
        src.addServerEvent(1, user, removeStats);
        user.GetComponent<PlayerMovement>().itemDelay = 1;
    }
}
