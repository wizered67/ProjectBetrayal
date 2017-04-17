using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface Item {
    void useItem(GameObject user, ServerRoundController src);
    string getName();
    bool canUse();
    bool discardOnUse();
}
