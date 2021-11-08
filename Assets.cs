using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Assets : MonoBehaviour {

    public static Assets _instance;

    [SerializeField]
    public List<effectIcon> effectIcons;
    
    void Start() {
        _instance = this;
    }
}


[System.Serializable]
public class effectIcon {

    public EffectType type;
    public GameObject icon;

} 