using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PermIntelligenceBoostItem : Item
{
    public bool canUse()
    {
        return true;
    }

    public bool discardOnUse()
    {
        return false;
    }

    public string getName()
    {
        return "Book";
    }

   void stopItemDelay(GameObject user)
    {
        user.GetComponent<PlayerMovement>().itemDelay = 0;
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used intelligence item.");
        Stats stats = user.GetComponent<Stats>();
        stats.gainIntelligence(1);
        src.addServerEvent(1, user, stopItemDelay);
        user.GetComponent<PlayerMovement>().itemDelay = 2;
    }
}

