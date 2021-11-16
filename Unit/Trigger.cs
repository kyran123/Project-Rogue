using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType {
    Effect,
    Healed,
    DamageReceived,
    DamageDealt,
    OnKill,
    OnDeath,
    HealthLower,
    HealthAbove,
    APLower,
    APAbove,
    SpeedLower,
    SpeedAbove,
    MoveCount
}

[System.Serializable]
public class Trigger {

    [SerializeField]
    public TriggerType type;

    [SerializeField]
    int triggerCount;

    [SerializeField]
    EffectType effectType;

    public bool isValid(Unit unit, int value = 0) {
        switch(this.type) {
            case TriggerType.Effect:
                if(unit.hasEffect(this.effectType)) return true;
                break;
            case TriggerType.Healed:
            case TriggerType.OnKill:
            case TriggerType.OnDeath:
                return true;
            case TriggerType.DamageReceived:
                if(value > this.triggerCount) return true;
                break;
            case TriggerType.DamageDealt:
                if(value > this.triggerCount) return true;
                break;
            case TriggerType.HealthLower:
                if(unit.Health < this.triggerCount) return true;
                break;
            case TriggerType.HealthAbove:
                if(unit.Health > this.triggerCount) return true;
                break;
            case TriggerType.APLower:
                if(unit.ActionPoints < this.triggerCount) return true;
                break;
            case TriggerType.APAbove:
                if(unit.ActionPoints > this.triggerCount) return true;
                break;
            case TriggerType.SpeedLower:
                if(unit.getSpeed() < this.triggerCount) return true;
                break;
            case TriggerType.SpeedAbove:
                if(unit.getSpeed() > this.triggerCount) return true;
                break;
            case TriggerType.MoveCount:
                if(unit.moves.Count > this.triggerCount) return true;
                break;
        }
        return false;
    }
    
}