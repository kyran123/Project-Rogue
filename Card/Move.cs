using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum targetType {
    NONE,
    FRIENDLY,
    ENEMY,
    ALLENEMIES,
    ALLFRIENDLIES,
    BOTH //Artifacts only!
}

[System.Serializable]
public class Move {
    
    [SerializeField]
    public MoveCost moveCost;

    [SerializeField]
    public string moveName;

    [SerializeField]
    public bool confusion;

    [SerializeField]
    public int damage;

    [SerializeField]
    public targetType damageTargetType;

    [SerializeField]
    public List<Unit> damageTargets;

    [SerializeField]
    public List<Effect> effects = new List<Effect>();

    public bool hasEffectByType(EffectType type) {
        foreach(Effect effect in this.effects) {
            if(effect.type == type) {
                return true;
            }
        }
        return false;
    }

    public Effect getEffectByType(EffectType type) {
        foreach(Effect effect in this.effects) {
            if(effect.type == type) {
                return effect;
            }
        }
        return null;
    }
}

//Enemy only
[System.Serializable]
public class MoveCost {

    public int APcost;

    public int speedCost;

    public int HPCost;

}