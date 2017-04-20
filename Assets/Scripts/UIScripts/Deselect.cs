using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deselect : MonoBehaviour
{
    public void DS()
    {
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }
}
