using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PermMightBoostItem : Item
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
        return "Dumbbell";
    }

   void stopItemDelay(GameObject user)
    {
        user.GetComponent<PlayerMovement>().itemDelay = 0;
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used Might item.");
        Stats stats = user.GetComponent<Stats>();
        stats.gainMight(1);
        src.addServerEvent(1, user, stopItemDelay);
        user.GetComponent<PlayerMovement>().itemDelay = 1;
    }
}

