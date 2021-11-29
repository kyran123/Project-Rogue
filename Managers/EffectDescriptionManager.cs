using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EffectDescriptionManager : MonoBehaviour {

    public GameObject descriptionPrefab;
    public Dictionary<EffectType, GameObject> descriptions = new Dictionary<EffectType, GameObject>();

    public void setSkillDescriptions(Unit unit) {
        foreach(Skill skill in unit.skills) {
            GameObject obj = this.instantiatePrefab(descriptionPrefab);
            obj.transform.SetAsFirstSibling();
            obj.GetComponent<Image>().color = new Color32(74, 220, 255, 255);
            obj.GetComponent<EffectDescription>().updateText(skill.generateTitle(), skill.generateDescription());
        }
        //TODO: Injuries
    }

    public void updateDescriptions(List<Effect> effects) {
        foreach(Effect effect in effects) {
            if(this.descriptions.ContainsKey(effect.type)) {
                if(effect.stackCount == 0) this.removeDescription(effect);
                else this.descriptions[effect.type].GetComponent<EffectDescription>().updateText(effect.generateTitle(), effect.generateEffectDescription());
            } else {
                if(effect.stackCount > 0) {
                    GameObject obj = this.instantiatePrefab(descriptionPrefab);
                    this.descriptions.Add(effect.type, obj);
                    obj.GetComponent<EffectDescription>().updateText(effect.generateTitle(), effect.generateEffectDescription());
                }
            }
        }
    }

    public GameObject instantiatePrefab(GameObject prefab) {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(this.transform);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);
        return obj;
    }

    public void removeDescription(Effect effect) {
        if(this.descriptions.ContainsKey(effect.type)) {
            Destroy(this.descriptions[effect.type]);
            this.descriptions.Remove(effect.type);
        }
    }

}
