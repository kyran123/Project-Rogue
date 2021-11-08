using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Icon : MonoBehaviour {

    public TMP_Text stackCount;

    public void updateIcon(Effect effect) {
        stackCount.text = $"{effect.stackCount}";
    }
    
}
