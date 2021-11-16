using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public enum EffectType {
    Poison, // Take damage equal to [stackCount]
    Strength, // Do 25% more damage
    Thorns, // Deal [stackCount] damage to the attacker
    Frail, // Receive 25% more damage
    Weak, // Do 25% less damage
    Resistance, // Receive half damage
    Protect, // Prevent other friendly units from receiving damage by redirecting it to yourself
    Shield, // Block [stackCount] amount damage, reduce speed by 50%
    Heartless, // Prevent the unit from being healed
    Lifesteal, // Heal the unit by the amount of damage dealt
    Regen, // Restore 1 health per [stackCount] at the end of the turn
    Stealth, // Unit cannot be targeted by enemies, if it makes a move stealth goes away
    Haste, // Increase the speed of the unit by 25% of the unit's base speed
    Slow, // Decrease the speed of the unit by 25% of the unit's base speed
    Cleanse, // Remove all effects on the target
    Immunity, // Prevent other effects from being applied on the target
    Silence, // Prevent targeted unit from applying any effects
    Endure, // Prevent unit's HP from being reduced below 1, once run out you'll gain 4 weak
    Oblivion, // Negate all artifact effects
    Bomb, // When the bomb effect [stackCount] reaches 0 it explodes and deals damage
    Stun, // Prevents the unit from making moves
    Confusion, // The unit chooses targets at random
    Cursed, // The unit takesmdamage for every move made
    Energized, // At the start of turn you gain [stackCount] AP
    Exhaust, // At the start of turn, you lose [stackCount] AP
    Bound, // Reduce the amount of moves an unit can make to 1
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
    ///<summary>returns a string</summary>
    public string generateDescription() {
        //stackcount effect type name 'to' target name
        return $"Apply {stackCount} {type.ToString()} to {targetNames[(int)this.targetType]}.";
    }
    ///<summary>returns a string</summary>
    public string generateTitle() {
        return $"{type.ToString()}";
    }
    ///<summary>returns a string</summary>
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
