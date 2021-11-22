using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType {
    Health,
    Speed,
    AP,
    Effect,
    Draw
}

[System.Serializable]
public class Skill {

    [SerializeField]
    public string name;

    [SerializeField]
    Trigger trigger;

    [SerializeField]
    public SkillType skillType;

    [SerializeField]
    public int skillBonus;

    [SerializeField]
    Effect effect;

    public void eventTrigger(TriggerType type, Unit unit, int value) {
        if(this.trigger.type == type) {
            if(this.trigger.isValid(unit, value)) {
                switch(this.skillType) {
                    case SkillType.Health:
                        unit.modifyHealth(this.skillBonus);
                        break;
                    case SkillType.AP:
                        unit.changeActionPoints(-this.skillBonus);
                        break;
                    case SkillType.Effect:
                        unit.addEffect(effect);
                        break;
                    case SkillType.Draw:
                        BattleManager.instance.drawCard(this.skillBonus);
                        break;
                    case SkillType.Speed:
                        unit.modifySpeed(this.skillBonus);
                        break;
                }

            }
        }
    }

    public bool isValid(Unit unit) {
        return this.trigger.isValid(unit);
    }

    public TriggerType getTriggerType() {
        return this.trigger.type;
    }

    public string generateTitle() {
        return $"{this.name.ToString()}";
    }

    public string generateDescription() {
        switch(this.skillType) {
            case SkillType.Health:
                return $"Gain {this.skillBonus} HP {this.trigger.generateSkillDescription()}";
            case SkillType.AP:
                return $"Gain {this.skillBonus} AP {this.trigger}";
            case SkillType.Effect:
                return $"Gain {this.effect.stackCount} {this.effect.generateTitle()} {this.trigger}";
            case SkillType.Draw:
                return $"Draw {this.skillBonus} cards {this.trigger}";
            case SkillType.Speed:
                return $"Gain {this.skillBonus} speed {this.trigger}";
        }
        return "";
    }
    
}