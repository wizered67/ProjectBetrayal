using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Stats : NetworkBehaviour {

    private UpdateStatDisplays statDisplays;
    //Server side variables for stats, synchronized with local players
    public SyncListInt stats = new SyncListInt();
    public SyncListInt queuedStats = new SyncListInt();
    public ServerDataManager serverData;

    public Item[] allItems;
    //server side variable
    public HashSet<int> itemsLeftToFind = new HashSet<int>();

    public SyncListInt items = new SyncListInt();

    [SyncVar(hook ="onDiscoveryProgressChange")]
    public int discoveryProgress = 0;

    [SyncVar(hook = "onHealthChange")]
    public int curHealth = 5;

    public const int SPEED_INDEX = 0;
    public const int MIGHT_INDEX = 1;
    public const int SANITY_INDEX = 2;
    public const int INTELLIGENCE_INDEX = 3;
    public const int HEALTH_INDEX = 4;
    public const int NUM_STATS = 5;
    
    public static int maxSpdMod
    {
        get
        {
            int temp = 0;

            foreach (Stats stat in FindObjectsOfType<Stats>())
            {
                temp = Mathf.Max(temp,Mod(stat.getSpeed()));
            }

            return temp;
        }
    }

    public static Stats LocalStats()
    {
        foreach (Stats s in GameObject.FindObjectsOfType<Stats>())
        {
            if (s.isLocalPlayer)
            {
                return s;
            }
        }
        return null;
    }

    public static int Mod(int value)
    {
        return (value >= 15 ? 5 : value >= 10 ? 4 : value >= 6 ? 3 : value >= 3 ? 2 : 1) + 1;
    }

    public static int Remainder(int value)
    {
        return (value >= 15 ? value - 15 : value >= 10 ? value - 10 : value >= 6 ? value - 6 : value >= 3 ? value - 3 : value - 1);
    }

    public void onItemsChange(SyncListInt.Operation op, int index)
    {
        if (op == SyncListInt.Operation.OP_ADD && isLocalPlayer)
        {
            ExplorationGUI.UpdateItemDisplay();
        }
    }

    public void onDiscoveryProgressChange(int newValue)
    {
        discoveryProgress = newValue; //update any UI here

        if (isLocalPlayer)
        {
            ExplorationGUI.UpdateValue(newValue);
        }
    }

    public void onHealthChange(int newValue)
    {
        if (isLocalPlayer)
        {
            if (newValue < 1)
            {
                Application.Quit();
            }
            else
            {
                HealthGUI.UpdateValue(newValue);
            }
        }
    }

    [Command]
    public void CmdGainDiscoveryProgress(int amount)
    {
        discoveryProgress = discoveryProgress + amount;

        if (discoveryProgress >= 10)
        {
            if (8 >= items.Count + 1)//Mod(getIntelligence()) >= items.Count + 1)
            {
                int[] list = new int[itemsLeftToFind.Count];
                int i = 0;
                foreach (int item in itemsLeftToFind)
                {
                    list[i++] = item;
                }

                int newItemIndex = Random.Range(0, list.Length);

                if (list[newItemIndex] > 3)
                {
                    items.Add(4);
                    itemsLeftToFind.Remove(4);
                    items.Add(5);
                    itemsLeftToFind.Remove(5);
                    items.Add(6);
                    itemsLeftToFind.Remove(6);
                    items.Add(7);
                    itemsLeftToFind.Remove(7);
                }
                else
                {
                    int newItem = list[newItemIndex];

                    items.Add(newItem);
                    print("Gained item " + newItem);
                    itemsLeftToFind.Remove(newItem);
                }

                discoveryProgress -= 10;
            }
            else
            {
                discoveryProgress = 9;
            }
        }
    }
    public void UseItem(int itemId)
    {
        CmdUseItem(itemId);
    }

    [Command]
    public void CmdUseItem(int itemId)
    {
        allItems[itemId].useItem(gameObject, PlayerMovement.localPlayer.GetComponent<Stats>().serverData.GetComponent<ServerRoundController>());
        
        if (allItems[itemId].discardOnUse())
        {
            if (itemId > 3)
            {
                Debug.Log(itemId);
                itemsLeftToFind.Add(4);
                items.Remove(4);
                itemsLeftToFind.Add(5);
                items.Remove(5);
                itemsLeftToFind.Add(6);
                items.Remove(6);
                itemsLeftToFind.Add(7);
                items.Remove(7);
            }
            else
            {
                items.Remove(itemId);
                itemsLeftToFind.Add(itemId);
            }
        }

        CmdUpdateStatsToQueued();
    }

    public void init()
    {
        statDisplays = GameObject.Find("StatDisplays").GetComponent<UpdateStatDisplays>();
        stats.Callback = onStatChange;
        items.Callback = onItemsChange;


        allItems = new Item[8];
        allItems[0] = new TempSpeedBoostItem();
        allItems[1] = new TempMightBoostItem();
        allItems[2] = new TempSanityBoostItem();
        allItems[3] = new MedKit();
        allItems[4] = new PermSpeedBoostItem();
        allItems[5] = new PermMightBoostItem();
        allItems[6] = new PermSanityBoostItem();
        allItems[7] = new PermIntelligenceBoostItem();

        //initialize all items
        if (isServer)
        {
            for (int i = 0; i < allItems.Length; i += 1)
            {
                itemsLeftToFind.Add(i);
            }
        }
        
        /*
        if (isServer && !isReady())
        {
            for (int i = 0; i < NUM_STATS; i += 1)
            {
                setServerStat(i, Random.Range(2, 6));
            }
        }*/
        if (isLocalPlayer)
        {
            CmdUpdateStatsToQueued();
        }
    }

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

    public int getHealth()
    {
        return stats[HEALTH_INDEX];
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

    public void set(int spd, int mgt, int snty, int itel, int hlth)
    {
        CmdSetStat(SPEED_INDEX, spd);
        CmdSetStat(MIGHT_INDEX, mgt);
        CmdSetStat(SANITY_INDEX, snty);
        CmdSetStat(INTELLIGENCE_INDEX, itel);
        CmdSetStat(HEALTH_INDEX,hlth);
    }

    public void loseHighest(int amount)
    {
        int highestIndex = 0;
        for (int i = 0; i < stats.Count; i += 1)
        {
            if (stats[i] > stats[highestIndex])
            {
                highestIndex = i;
            }
        }
        CmdGainStat(highestIndex, -amount);
    }

    public void gainMight(int amount)
    {
        CmdGainStat(MIGHT_INDEX, amount);
    }

    public void gainSpeed(int amount)
    {
        CmdGainStat(SPEED_INDEX, amount);
    }

    public void gainIntelligence(int amount)
    {
        CmdGainStat(INTELLIGENCE_INDEX, amount);
    }

    public void gainSanity(int amount)
    {
        CmdGainStat(SANITY_INDEX, amount);
    }

    public void gainHealth(int amount)
    {
        if (curHealth + amount <= stats[HEALTH_INDEX])
        {
            CmdGainHealth(amount);
        }
    }

    [Command]
    void CmdGainHealth(int amount)
    {
        curHealth += amount;
        if (amount < 0)
        {
            RpcDamageText(amount);
        }
    }

    [ClientRpc]
    void RpcDamageText(int val)
    {
        transform.FindChild("DamageText").GetComponent<TextMesh>().text = val.ToString();
        transform.FindChild("DamageText").GetComponent<Animator>().SetTrigger("Hit");
    }

    [ClientRpc]
    public void RpcUpdateStats()
    {
        if (isLocalPlayer)
        {
            statDisplays = GameObject.Find("StatDisplays").GetComponent<UpdateStatDisplays>();
            //stats.Callback = onStatChange;
            for (int index = 0; index < stats.Count; index += 1)
            {
                statDisplays.updateDisplay(index, stats[index]);
            }
        }
    }

    [ClientRpc]
    public void RpcDisplayMonsterLevelUp()
    {
        if (PlayerMovement.localPlayer.GetComponent<PlayerMovement>().isWerewolf)
        {
            GameObject.Find("StatDisplays").transform.FindChild("LevelingTags").gameObject.SetActive(true);
        }
    }

    void onStatChange(SyncListInt.Operation op, int index)
    {
        print("stat change.");
        /*
        if ((op == SyncListInt.Operation.OP_SET || op == SyncListInt.Operation.OP_INSERT) && isLocalPlayer && index < stats.Count)
        {
            statDisplays.updateDisplay(index, stats[index]);
        }*/

        if (isServer)
        {
            RpcUpdateStats();
        }
    }

    public bool isReady()
    {
        return stats.Count >= NUM_STATS;
    }
    
    private void setServerStat(int index, int value)
    {
        if (queuedStats.Count <= index)
        {
            queuedStats.Insert(index, value);
            stats.Insert(index, value);
        }
        else
        {
            queuedStats[index] = value;
        }
    }

    [Command]
    public void CmdSetStat(int index, int value)
    {
        if (queuedStats.Count <= index)
        {
            queuedStats.Insert(index, value);
            stats.Insert(index, value);
        }
        else
        {
            queuedStats[index] = value;
        }
    }

    [Command]
    public void CmdGainStat(int index, int amount)
    {
        queuedStats[index] += amount;
    }

    [Command]
    public void CmdUpdateStatsToQueued()
    {
        bool damage = false;
        for (int i = 0; i < queuedStats.Count; i += 1)
        {
            if (queuedStats[i] < stats[i])
            {
                damage = true;
            }
            stats[i] = queuedStats[i];
            print("Updated stat " + i + " to " + stats[i]);
        }
        if (damage)
        {
            RpcDamageFlash();
        }
    }

    [ClientRpc]
    void RpcDamageFlash()
    {
        StartCoroutine("DamageFlash");
    }

    IEnumerator DamageFlash()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(1);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer || !isReady())
        {
            return;
        }
		if (Mod(getSpeed()) < serverData.subroundNumber)
        {
            statDisplays.updateColor(4, new Color(1,0.1f,0.1f));
        } else
        {
            statDisplays.updateColor(4, new Color(0.1f, 1, 0.1f));
        }
	}
}
