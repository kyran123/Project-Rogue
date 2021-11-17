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
    Endure, // Prevent unit's HP from being reduced below 1, once run out you'll gain 2 weak
    Oblivion, // Negate all artifact effects
    Bomb, // When the bomb effect [stackCount] reaches 0 it explodes and deals damage
    Stun, // Prevents the unit from making moves
    Confusion, // The unit chooses targets at random
    Cursed, // The unit takes damage for every move made
    Energized, // At the start of turn you gain [stackCount] AP
    Exhaust, // At the start of turn, you lose [stackCount] AP
    Bound, // Reduce the amount of moves an unit can make to 1
}

[System.Serializable]
public class Effect {

    [SerializeField]
    public EffectType type;
    [SerializeField]
    public int stackCount;
    [SerializeField]
    public bool maintain;
    [SerializeField]
    public targetType targetType;
    [SerializeField]
    public List<Unit> targets;

    ///<summary>Apply effects at the start of the units' turn</summary>
    public void apply(Unit unit) {
        if(this.stackCount == 0) return;
        switch(this.type) {
            case EffectType.Poison: 
                unit.modifyHealth(this.stackCount--);
                break;
            case EffectType.Regen:
                unit.modifyHealth(-this.stackCount--);
                break;
            case EffectType.Lifesteal:
                if(!this.maintain) this.stackCount--;
                break;
            case EffectType.Bomb:
                if(--this.stackCount == 0) unit.modifyHealth(25); // damage is undecided
                break;
            case EffectType.Energized:
                unit.changeActionPoints(-this.stackCount--);
                break;
            case EffectType.Exhaust:
                unit.changeActionPoints(this.stackCount--);
                break;
        }
    }

    public void reduceStackCount(Unit unit) {
        switch(this.type) {
            case EffectType.Slow:
            case EffectType.Haste:
            case EffectType.Shield:
            case EffectType.Immunity:
            case EffectType.Heartless:
            case EffectType.Strength:
            case EffectType.Weak:
            case EffectType.Frail:
            case EffectType.Thorns:
            case EffectType.Resistance:
            case EffectType.Bound:
            case EffectType.Cursed:
            case EffectType.Confusion:
            case EffectType.Stun:
            case EffectType.Endure:
            case EffectType.Silence:
                this.stackCount--;              
                break;
        }
    }
    [SerializeField]
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
                return $"Heal for {stackCount}.";
            case EffectType.Lifesteal:
                return $"Heal for the damage done by this unit.";
            case EffectType.Shield:
                return $"Block {stackCount} damage.";
            case EffectType.Haste:
                return $"Speed increased by 25%.";
            case EffectType.Slow:
                return $"Speed decreased by 25%.";
            case EffectType.Poison:
                return $"Take {stackCount} damage at the start of the turn.";
            case EffectType.Strength:
                return $"Damage increased by 25%.";
            case EffectType.Weak:
                return $"Damage reduced by 25%.";
            case EffectType.Frail:
                return $"Take 25% more damage.";
            case EffectType.Resistance:
                return $"Take half damage.";
            case EffectType.Protect:
                return $"Redirect all incoming damage to the unit.";
            case EffectType.Thorns:
                return $"Deal {stackCount} damage to the atacker.";
            case EffectType.Heartless:
                return $"The unit can not be healed.";
            case EffectType.Stealth:
                return $"The unit can not be targeted by enemies. If the unit makes a move, the effect is lost.";
            case EffectType.Cleanse:
                return $"Remove all effects on the target.";
            case EffectType.Immunity:
                return $"Prevent other effects from being applied on the unit.";
            case EffectType.Silence:
                return $"Preent the unit from applying any effetcs.";
            case EffectType.Endure:
                return $"Prevent the units' HP from reducing bellow 1. Receive Weak for 2 turns after it's removed.";
            case EffectType.Bomb:
                if(stackCount > 1)
                return $"Explodes in {stackCount} turns and deals 25 damage."; //damage is undecided
                else return $"Explodes in {stackCount} turn and deals 25 damage.";
            case EffectType.Stun:
                if(stackCount > 1)
                return $"The unit can not make moves for {stackCount} turns.";
                else return $"The unit can not make moves for {stackCount} turn.";
            case EffectType.Confusion:
                return $"Targets for the units' moves are chosen at random.";
            case EffectType.Cursed:
                return $"The unit takes 5 damage for every move made."; //effect is not implemented, damage made is undecided
            case EffectType.Energized:
                return $"At the start of the turn the unit gains {stackCount} AP.";
            case EffectType.Exhaust:
                return $"At the start of the turn the unit loses {stackCount} AP.";
            case EffectType.Bound:
                return $"The unit can make only 1 move.";
            default:
                return "";
        }
        
    }

}
