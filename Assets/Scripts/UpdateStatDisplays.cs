using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateStatDisplays : MonoBehaviour {
    public GameObject speedDisplay;
    public GameObject mightDisplay;
    public GameObject sanityDisplay;
    public GameObject intelligenceDisplay;
	// Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void updateDisplay(int index, int newValue)
    {
       switch (index)
        {
            case 0:
                speedDisplay.GetComponent<UnityEngine.UI.Text>().text = "Speed: " + Stats.Mod(newValue);
                speedDisplay.transform.Find("Mod").GetComponent<UnityEngine.UI.Text>().text = "("+Stats.Remainder(newValue) +"/"+ Stats.Mod(newValue) + ")";
                break;
            case 1:
                mightDisplay.GetComponent<UnityEngine.UI.Text>().text = "Damage: " + Stats.Mod(newValue);
                mightDisplay.transform.Find("Mod").GetComponent<UnityEngine.UI.Text>().text = "(" + Stats.Remainder(newValue) + "/" + Stats.Mod(newValue) + ")";
                break;
            case 2:
                sanityDisplay.GetComponent<UnityEngine.UI.Text>().text = "Perception: " + Stats.Mod(newValue);
                sanityDisplay.transform.Find("Mod").GetComponent<UnityEngine.UI.Text>().text = "(" + Stats.Remainder(newValue) + "/" + Stats.Mod(newValue) + ")";
                break;
            case 3:
                intelligenceDisplay.GetComponent<UnityEngine.UI.Text>().text = "Intelligence: " + Stats.Mod(newValue);
                intelligenceDisplay.transform.Find("Mod").GetComponent<UnityEngine.UI.Text>().text = "(" + Stats.Remainder(newValue) + "/" + Stats.Mod(newValue) + ")";
                break;
        }
    }

    public void updateColor(int index, Color newColor)
    {
        switch (index)
        {
            case 0:
                speedDisplay.GetComponent<UnityEngine.UI.Text>().color = newColor;
                break;
            case 1:
                mightDisplay.GetComponent<UnityEngine.UI.Text>().color = newColor;
                break;
            case 2:
                sanityDisplay.GetComponent<UnityEngine.UI.Text>().color = newColor;
                break;
            case 3:
                intelligenceDisplay.GetComponent<UnityEngine.UI.Text>().color = newColor;
                break;
            case 4:
                transform.parent.Find("RoundText").GetComponent<UnityEngine.UI.Text>().color = newColor;
                break;
        }
    }

    public void LevelUpSpeed()
    {
        PlayerMovement.localPlayer.GetComponent<Stats>().gainSpeed(1);
        updateDisplay(0, PlayerMovement.localPlayer.GetComponent<Stats>().getSpeed());
        PlayerMovement.localPlayer.GetComponent<Stats>().CmdUpdateStatsToQueued();
    }
    public void LevelUpMight()
    {
        PlayerMovement.localPlayer.GetComponent<Stats>().gainMight(1);
        updateDisplay(1, PlayerMovement.localPlayer.GetComponent<Stats>().getMight());
        PlayerMovement.localPlayer.GetComponent<Stats>().CmdUpdateStatsToQueued();
    }
    public void LevelUpSanity()
    {
        PlayerMovement.localPlayer.GetComponent<Stats>().gainSanity(1);
        updateDisplay(2, PlayerMovement.localPlayer.GetComponent<Stats>().getSanity());
        PlayerMovement.localPlayer.GetComponent<Stats>().CmdUpdateStatsToQueued();
    }
    public void LevelUpIntelligence()
    {
        PlayerMovement.localPlayer.GetComponent<Stats>().gainIntelligence(1);
        updateDisplay(3, PlayerMovement.localPlayer.GetComponent<Stats>().getIntelligence());
        PlayerMovement.localPlayer.GetComponent<Stats>().CmdUpdateStatsToQueued();
    }
}
