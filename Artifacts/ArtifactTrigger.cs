using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ArtifactTriggerType {
    Turn, //Triggered every turn
    Play, //Play cards
    Kill, //When you kill an unit
    Death, //When your unit dies
    Consume, //When card is consumed
    ReceiveDamage, //When damage is received
    DealDamage, //When damage is dealt
}

[System.Serializable]
public class ArtifactTrigger {
    [SerializeField]
    bool isOneTimeUse; //If you can only use it once per battle

    [SerializeField]
    bool isStartOfBattle; //If it can only be used as the battle is initialized

    [SerializeField]
    bool isOncePerTurn; //If you can only use it once per turn
    [SerializeField]
    public bool atStartofTurn; //Is at end of turn on false

    public bool isTriggered;

    [SerializeField]
    public ArtifactTriggerType type;

    [SerializeField]
    int triggerCount;

    [SerializeField]
    int currentCount;

    [SerializeField]
    [Tooltip("The units that can trigger this artifact")]
    targetType tType;

    public bool isValid(Unit unit, int value) {
        //TODO: if any unit has oblivion -> return false
        switch(tType) {
            case targetType.FRIENDLY:
            case targetType.ALLFRIENDLIES:
                if(!BattleManager.instance.friendlyUnits.Contains(unit)) return false;
                break;
            case targetType.ENEMY:
            case targetType.ALLENEMIES:
                if(!BattleManager.instance.enemyUnits.Contains(unit)) return false;
                break;
        }
        if(this.isTriggered && this.isOncePerTurn) return false; //Make sure to reset this.isTriggered on start of new player turn
        switch(this.type) {
            case ArtifactTriggerType.Play:
                currentCount += value;
                if(this.isDivisible(this.currentCount, this.triggerCount)) return true;
                break;
            case ArtifactTriggerType.ReceiveDamage:
            case ArtifactTriggerType.DealDamage:
                currentCount += value;
                if(this.currentCount >= this.triggerCount) {
                    this.currentCount = 0;
                    return true;
                } 
                break;
            case ArtifactTriggerType.Turn:
            case ArtifactTriggerType.Kill:
            case ArtifactTriggerType.Death:
            case ArtifactTriggerType.Consume:
                return true;
            
        }
        return false;
    }

    public bool isDivisible(int count, int trigger) {
        return (count % trigger) == 0;
    }

    public string getDescription() {
        switch(this.type) {
            case ArtifactTriggerType.Turn:
                return $"On every turn";
            case ArtifactTriggerType.Play:
                if(this.triggerCount == 1) return $"On every card played";
                else return $"On every {this.triggerCount} cards played";
            case ArtifactTriggerType.Kill:
                return $"On every kill";
            case ArtifactTriggerType.Death:
                return $"On every friendly unit death";
            case ArtifactTriggerType.Consume:
                return $"Whenever a Consume card is played";
            case ArtifactTriggerType.ReceiveDamage:
                return $"For every {this.triggerCount} damage received";
            case ArtifactTriggerType.DealDamage:
                return $"For every {this.triggerCount} damage dealt";
        }
        return "";
    }
}
