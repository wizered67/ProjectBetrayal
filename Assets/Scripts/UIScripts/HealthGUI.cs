using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthGUI : MonoBehaviour
{
    public Slider mySlider;
    public Image myFill;
    public Text myText;

    static private HealthGUI GetHealthGUI()
    {
        return GameObject.FindObjectOfType<HealthGUI>().GetComponent<HealthGUI>();
    }

    void Start()
    {
        StartCoroutine(WaitForStats());
    }

    private IEnumerator WaitForStats()
    {   
        while (Stats.LocalStats() == null)
        {
            yield return null;
        }

        Stats myStats = Stats.LocalStats();

        mySlider.maxValue = myStats.getHealth();
        UpdateValue(myStats.getHealth());
    }

    static public void UpdateValue(int newValue)
    {
        HealthGUI myGUI = GetHealthGUI();
        myGUI.myFill.enabled = true;
        myGUI.mySlider.value = newValue;
        myGUI.myText.text = newValue + " / " + myGUI.mySlider.maxValue;
    }
}
