﻿using System;
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

    void removeStats(GameObject user)
    {
        Stats stats = user.GetComponent<Stats>();
        stats.gainSanity(-8);
        stats.CmdUpdateStatsToQueued();
        stats.RpcUpdateStats();
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used Speed item.");
        Stats stats = user.GetComponent<Stats>();
        stats.gainSanity(8);
        src.addServerEvent(1, user, removeStats);
        user.GetComponent<PlayerMovement>().itemDelay = 1;
    }
}
