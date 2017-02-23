using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Stats : NetworkBehaviour {
    //Server side variables for stats, synchronized with local players
    private UpdateStatDisplays statDisplays;
    public SyncListInt stats = new SyncListInt();

    public const int SPEED_INDEX = 0;
    public const int MIGHT_INDEX = 1;
    public const int SANITY_INDEX = 2;
    public const int INTELLIGENCE_INDEX = 3;
    public const int NUM_STATS = 4;

    public int getSpeed()
    {
        return stats[SPEED_INDEX];
    }

    public int getMight()
    {
        return stats[MIGHT_INDEX];
    }

    public int getSanity()
    {
        return stats[SANITY_INDEX];
    }

    public int getIntelligence()
    {
        return stats[INTELLIGENCE_INDEX];
    }

    public void setSpeed(int value)
    {
        CmdSetStat(SPEED_INDEX, value);
    }

    public void setMight(int value)
    {
        CmdSetStat(MIGHT_INDEX, value);
    }

    public void setSanity(int value)
    {
        CmdSetStat(SANITY_INDEX, value);
    }

    public void setIntelligence(int value)
    {
        CmdSetStat(INTELLIGENCE_INDEX, value);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        statDisplays = GameObject.Find("StatDisplays").GetComponent<UpdateStatDisplays>();
        stats.Callback = onStatChange;
        for (int i = 0; i < NUM_STATS; i += 1)
        {
            CmdSetStat(i, Random.Range(2, 6));
        }
    }

    void onStatChange(SyncListInt.Operation op, int index)
    {
        if ((op == SyncListInt.Operation.OP_SET || op == SyncListInt.Operation.OP_INSERT) && isLocalPlayer && index < stats.Count)
        {
            statDisplays.updateDisplay(index, stats[index]);
        }
    }

    public bool isReady()
    {
        return stats.Count >= NUM_STATS;
    }

    [Command]
    void CmdSetStat(int index, int value)
    {
        if (stats.Count <= index)
        {
            stats.Insert(index, value);
        }
        else
        {
            stats[index] = value;
        }
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
