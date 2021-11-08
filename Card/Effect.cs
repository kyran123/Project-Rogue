using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public enum EffectType {
    Poison, //Take damge equal to stacks
    Strength, //Do 25% more damage
    Thorns, //Deal [Stack Count] damage to the attacker
    Frail, //Receive 25% more damage
    Weak, //Do 25% less damage
    Resistance, //Receive half damage
    Protect, //Protect other friendly units from damage by receiving it yourself
    Shield, //Blocks x amount damage, reduce speed by 50%.  Decreases every turn.
    Heartless, //Prevents the unit from being healed
    Lifesteal, //When a unit with Lifesteal attacks, it restores health equal to the damage dealt.
    Regen, //Restores 1 health per stack at the end of turn. Decreases every turn.
    Stealth, //Unit cannot be targeted & attacked. If it does an (attack?) move stealth goes away. Decreases every turn.
    Haste, //Speed buff. Decreases every turn.
    Slow, //Speed debuff. Decreases every turn.
    Cleanse, //Remove all buffs & debuffs on target.
    Immunity, //Prevent other effects from being applied.
    Silence, //Prevent targeted unit from applying any effects (They can still do damage)
    Endure, //HP can not be reduced below 1. Once run out you'll gain 4 weak.
    Oblivion, //negate all artifact effects.
    Bomb, //When the bomb effect stackCount reaches 0 it explodes and deals damage.
    Stun, //Prevents casting moves on the unit
    Confusion, //The unit chooses target at random
    Cursed, //Take damage for every move the unit uses
    Energized, //At the start of turn, you gain AP for every stack of Energized. Decreases every turn.
    Exhaust, //At the start of turn, you lose AP for every stack of Exhaust. Decreases every turn.
    Bound, //Reduce the amount of moves an unit can have
}

[System.Serializable]
public class Effect {

    public EffectType type;

    public int stackCount;

    public bool maintain;

    [SerializeField]
    public targetType targetType;
    [SerializeField]
    public List<Unit> targets;

    public Effect() {

    }

    public void apply(Unit unit) {
        if(this.stackCount == 0) return;
        switch(this.type) {
            case EffectType.Poison: 
                unit.modifyHealth(this.stackCount);
                this.stackCount--;
                break;
            case EffectType.Regen:
                unit.modifyHealth(-this.stackCount);
                this.stackCount--;
                break;
            case EffectType.Lifesteal:
                if(!this.maintain) this.stackCount--;
                break;
        }
    }

    public void reduceStackcount(Unit unit) {
        switch(this.type) {
            case EffectType.Slow: 
            case EffectType.Haste:
                this.stackCount--;              
                break;
        }
    }

    string[] targetNames = {
        "None",
        "this unit",
        "an enemy",
        "all enemy units",
        "all friendly units"
    };

    public string generateDescription() {
        //stackcount effect type name 'to' target name
        return $"Apply {stackCount} {type.ToString()} to {targetNames[(int)this.targetType]}.";
    }

    public string generateTitle() {
        return $"{type.ToString()}";
    }

    public string generateEffectDescription() {
        switch(type) {
            case EffectType.Regen:
                return $"Heal for {stackCount}. Decreases every turn.";
            case EffectType.Lifesteal:
                return $"Heal for the damage done by this unit.";
            case EffectType.Shield:
                return $"Block {stackCount} damage.";
            default:
                return "";
        }
        
    }

}
