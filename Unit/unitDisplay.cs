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

    ///<summary>Update effect icons</summary>
    public void updateIcons(List<Effect> effects, int index) {
        if(effects.Count == 0) { 
            this.clearIcons();
            return;
        }
        Effect effect = effects[index];
        if(!this.effectsOnUnit.ContainsKey(effect.type)) {
            GameObject obj = Instantiate(this.getIconPrefab(effect.type));
            obj.transform.SetParent(this.effectGrid.transform);
            obj.transform.localPosition = new Vector3(0, 0 ,0);
            obj.transform.localScale = new Vector3(1, 1, 1);
            this.effectsOnUnit.Add(effect.type, obj);
        }
        if(effect.stackCount < 1) this.removeEffectIcon(effect);
        else this.effectsOnUnit[effect.type].GetComponent<Icon>().updateIcon(effect);
        if(index < (effects.Count - 1)) {
            index++;
            this.updateIcons(effects, index);
        }
    }

    public void clearIcons() {
        foreach(GameObject descriptionObject in this.edManager.descriptions.Values) {
            Destroy(descriptionObject);
        }
        this.edManager.descriptions.Clear();
        foreach(GameObject effectObject in this.effectsOnUnit.Values) {
            Destroy(effectObject);
        }
        this.effectsOnUnit.Clear();
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

    ///<summary>Update all info displayed on the unit</summary>
    public void updateAllUnitInfo(Unit unit) {
        this.updateHealthBar(unit.Health, unit.MaxHealth);
        this.updateAP(unit.ActionPoints);
        this.updateSpeed(unit.getSpeed());
        this.updateIcons(unit.effects, 0);
    }

    ///<summary>Turn order appendage</summary>
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

    ///<summary>Toggle unit highlight</summary>
    public void setHighlight(bool active) {
        highlight.SetActive(active);
    }
}