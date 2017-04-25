using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MedKit : Item
{
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
        return "MedKit";
    }

   void stopItemDelay(GameObject user)
    {
        user.GetComponent<PlayerMovement>().itemDelay = 0;
    }

    public void useItem(GameObject user, ServerRoundController src)
    {
        Debug.Log("Used Health item.");
        Stats stats = user.GetComponent<Stats>();
        stats.gainHealth(1);
        //src.addServerEvent(1, user, stopItemDelay);
        //user.GetComponent<PlayerMovement>().itemDelay = 2;
    }
}

