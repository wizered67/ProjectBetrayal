using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateStatDisplays : MonoBehaviour {
    public GameObject speedDisplay;
    public GameObject mightDisplay;
    public GameObject sanityDisplay;
    public GameObject intelligenceDisplay;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void updateDisplay(int index, int newValue)
    {
       switch (index)
        {
            case 0:
                speedDisplay.GetComponent<UnityEngine.UI.Text>().text = "Speed: " + newValue;
                break;
            case 1:
                mightDisplay.GetComponent<UnityEngine.UI.Text>().text = "Might: " + newValue;
                break;
            case 2:
                sanityDisplay.GetComponent<UnityEngine.UI.Text>().text = "Sanity: " + newValue;
                break;
            case 3:
                intelligenceDisplay.GetComponent<UnityEngine.UI.Text>().text = "Intelligence: " + newValue;
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
        }
    }
}
