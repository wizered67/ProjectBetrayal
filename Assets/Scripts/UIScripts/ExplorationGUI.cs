using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplorationGUI : MonoBehaviour
{
    public Slider mySlider;
    public Image myFill;
    public Text myText;
    public Button[] items;

    static private ExplorationGUI GetExplorationGUI()
    {
        return GameObject.FindObjectOfType<ExplorationGUI>().GetComponent<ExplorationGUI>();
    }

    void Start()
    {
        UpdateValue(0);
    }

    static public void UpdateValue(int value)
    {
        ExplorationGUI myGUI = GetExplorationGUI();

        if (value == 0)
        {
            myGUI.myFill.enabled = false;
            myGUI.myText.text = "0 / 10";
        }
        else
        {
            myGUI.myFill.enabled = true;
            myGUI.mySlider.value = value;
            myGUI.myText.text = value.ToString() + " / 10";
        }
    }

    static public void AddItemToDisplay(int i)
    {
        ExplorationGUI myGUI = GetExplorationGUI();
        myGUI.items[i].interactable = true;
    }

    static public void RemoveItemToDisplay(int i)
    {
        ExplorationGUI myGUI = GetExplorationGUI();
        myGUI.items[i].interactable = false;
    }
}
