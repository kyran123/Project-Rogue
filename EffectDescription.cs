using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EffectDescription : MonoBehaviour {
    
    public Effect effect;
    public TMP_Text title;
    public TMP_Text description;

    public void updateText(string title, string desc) {
        this.title.text = $"{title}";
        this.description.text = $"{desc}";
    }

}
