using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class unitDisplay : MonoBehaviour {
    
    [Header("Unit objects")]
    public GameObject Healthbar;
    public GameObject Health;
    public TMP_Text HealthText;

    public GameObject APPoints;
    public TMP_Text APText;

    public GameObject SpeedPoints;
    public TMP_Text SpeedText;

    public TMP_Text indexText;

    public GameObject highlight;

    [Space(10)]
    [Header("Effect icons")]
    public GameObject effectGrid;
    public Dictionary<EffectType, GameObject> effectsOnUnit = new Dictionary<EffectType, GameObject>();

    [SerializeField]
    public EffectDescriptionManager edManager;

    public void updateIcons(List<Effect> effects) {
        foreach(Effect effect in effects) {
            if(!this.effectsOnUnit.ContainsKey(effect.type)) {
                GameObject obj = Instantiate(this.getIconPrefab(effect.type));
                obj.transform.SetParent(this.effectGrid.transform);
                obj.transform.localPosition = new Vector3(0, 0 ,0);
                obj.transform.localScale = new Vector3(1, 1, 1);
                this.effectsOnUnit.Add(effect.type, obj);
            } 
            this.effectsOnUnit[effect.type].GetComponent<Icon>().updateIcon(effect);
        }
    }

    public void removeEffectIcon(Effect effect) {
        if(this.effectsOnUnit.ContainsKey(effect.type)) {
            Destroy(this.effectsOnUnit[effect.type]);
            this.effectsOnUnit.Remove(effect.type);
        }
    }

    public GameObject getIconPrefab(EffectType type) {
        foreach(effectIcon effectIcon in Assets._instance.effectIcons) {
            if(effectIcon.type == type) return effectIcon.icon;
        }
        return null;
    }

    public void updateHealthBar(float health, float maxHealth) {
        float width = health / maxHealth;
        this.Health.transform.localScale = new Vector3(width, 1, 1);
        this.HealthText.text = $"{health}";
    }

    public void updateAP(float ap) {
        this.APText.text = $"{ap}";
    }

    public void updateSpeed(float speed) {
        this.SpeedText.text = $"{speed}";
    }

    string[] appendage = {
        "st",
        "nd",
        "rd",
        "th",
        "th",
        "th"
    };

    public void showOrder(int index) {
        this.indexText.text = $"{index}{appendage[index - 1]}";
    }


    public void setHighlight(bool active) {
        highlight.SetActive(active);
    }



}