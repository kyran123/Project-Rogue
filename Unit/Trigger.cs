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
    public EffectType effectType;

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
    
    public string generateSkillDescription() {
        switch(this.type) {
            case TriggerType.Effect:
                return $"on gaining {this.effectType} effect";
            case TriggerType.Healed:
                return $"when healed";
            case TriggerType.OnKill:
                return $"when it kills";
            case TriggerType.OnDeath:
                return $"when killed";
            case TriggerType.DamageReceived:
                return $"when damage is received";
            case TriggerType.DamageDealt:
                return $"when dealing damage";
            case TriggerType.HealthLower:
                return $"when health is below {this.triggerCount}";
            case TriggerType.HealthAbove:
                return $"when health is above {this.triggerCount}";
            case TriggerType.APLower:
                return $"when Action points is below {this.triggerCount}";
            case TriggerType.APAbove:
                return $"when Action points is above {this.triggerCount}";
            case TriggerType.SpeedLower:
                return $"when speed is below {this.triggerCount}";
            case TriggerType.SpeedAbove:
                return $"when speed is above {this.triggerCount}";
            case TriggerType.MoveCount:
                return $"on having {this.triggerCount} moves";
        }
        return "";
    }
}