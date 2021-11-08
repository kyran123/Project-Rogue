using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectDescriptionManager : MonoBehaviour {

    public GameObject descriptionPrefab;
    public Dictionary<EffectType, GameObject> descriptions = new Dictionary<EffectType, GameObject>();

    public void updateDescriptions(List<Effect> effects) {
        foreach(Effect effect in effects) {
            if(this.descriptions.ContainsKey(effect.type)) {
                this.descriptions[effect.type].GetComponent<EffectDescription>().updateText(effect.generateTitle(), effect.generateEffectDescription());
            } else {
                GameObject obj = Instantiate(descriptionPrefab);
                obj.transform.SetParent(this.transform);
                obj.transform.localPosition = new Vector3(0, 0, 0);
                obj.transform.localScale = new Vector3(1, 1, 1);
                this.descriptions.Add(effect.type, obj);
                obj.GetComponent<EffectDescription>().updateText(effect.generateTitle(), effect.generateEffectDescription());
            }
        }
    }

    public void removeDescription(Effect effect) {
        if(this.descriptions.ContainsKey(effect.type)) {
            Destroy(this.descriptions[effect.type]);
            this.descriptions.Remove(effect.type);
        }
    }

}
