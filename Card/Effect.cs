using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType {
    Poison, // Take damage equal to [stackCount] X
    Strength, // Do 25% more damage X
    Thorns, // Deal [stackCount] damage to the attacker X
    Frail, // Receive 25% more damage  X
    Weak, // Do 25% less damage X
    Resistance, // Receive half damage X
    Protect, // Prevent other friendly units from receiving damage by redirecting it to yourself X
    Shield, // Block [stackCount] amount damage, possibly reduce speed by 50% X
    Heartless, // Prevent the unit from being healed X
    Lifesteal, // Heal the unit by the amount of damage dealt X
    Regen, // Restore 1 health per [stackCount] at the end of the turn X
    Stealth, // Unit cannot be targeted by enemies, if it makes a move stealth goes away 
    Haste, // Increase the speed of the unit by 25% of the unit's base speed X
    Slow, // Decrease the speed of the unit by 25% of the unit's base speed X
    Cleanse, // Remove all effects on the target X
    Immunity, // Prevent other effects from being applied on the target X
    Silence, // Prevent targeted unit from applying any effects X
    Endure, // Prevent unit's HP from being reduced below 1, once run out you'll gain 2 weak X
    Oblivion, // Negate all artifact effects
    Bomb, // When the bomb effect [stackCount] reaches 0 it explodes and deals damage X
    Stun, // Prevents the unit from making moves X
    Confusion, // The unit chooses targets at random X
    Cursed, // The unit takes [stackCout] damage for every move made X
    Energized, // At the start of turn you gain [stackCount] AP X
    Exhaust, // At the start of turn, you lose [stackCount] AP X
    Bound, // Reduce the amount of moves an unit can make to 1 X
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
    [Space(10)]
    [SerializeField]
    public List<Unit> targets;

    [SerializeField]
    public bool firstTurn = true;


    public Effect instantiate(Effect effect) {
        this.type = effect.type;
        this.stackCount = effect.stackCount;
        this.maintain = effect.maintain;
        this.targetType = effect.targetType;
        this.targets = effect.targets;
        return this;
    }

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
            
        }
    }

    public void reduceStackCount(Unit unit) {
        if(this.firstTurn) {
            this.firstTurn = false;
            return;
        }
        switch(this.type) {
            case EffectType.Slow:
            case EffectType.Stealth:
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
            case EffectType.Energized:
                unit.changeActionPoints(-this.stackCount--);
                break;
            case EffectType.Exhaust:
                unit.changeActionPoints(this.stackCount--);
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
        return $"Apply {this.stackCount} {this.type.ToString()} to {this.targetNames[(int)this.targetType]}.";
    }
    ///<summary>returns a string</summary>
    public string generateTitle() {
        return $"{type.ToString()}";
    }
    ///<summary>returns a string</summary>
    public string generateEffectDescription() {
        switch(type) {
            case EffectType.Regen:
                return $"Heal for {stackCount} {this.generateDurationDescription()}.";
            case EffectType.Lifesteal:
                return $"Heal for the amount of damage done by this unit {this.generateDurationDescription()}.";
            case EffectType.Shield:
                return $"Block {stackCount} damage.";
            case EffectType.Haste:
                return $"Speed increased by 25% {this.generateDurationDescription()}.";
            case EffectType.Slow:
                return $"Speed decreased by 25% {this.generateDurationDescription()}.";
            case EffectType.Poison:
                return $"Take {stackCount} damage at the start of the turn {this.generateDurationDescription()}.";
            case EffectType.Strength:
                return $"Damage increased by 25% {this.generateDurationDescription()}.";
            case EffectType.Weak:
                return $"Damage reduced by 25% {this.generateDurationDescription()}.";
            case EffectType.Frail:
                return $"Take 25% more damage {this.generateDurationDescription()}.";
            case EffectType.Resistance:
                return $"Take half damage {this.generateDurationDescription()}.";
            case EffectType.Protect:
                return $"Redirect all incoming damage to the unit {this.generateDurationDescription()}.";
            case EffectType.Thorns:
                return $"Deal {stackCount} damage to the attacker {this.generateDurationDescription()}.";
            case EffectType.Heartless:
                return $"The unit can not be healed {this.generateDurationDescription()}.";
            case EffectType.Stealth:
                return $"The unit can not be targeted {this.generateDurationDescription()}. If the unit makes a move, the effect is lost.";
            case EffectType.Cleanse:
                return $"Remove all effects on the target.";
            case EffectType.Immunity:
                return $"Prevent other effects from being applied on the unit {this.generateDurationDescription()}.";
            case EffectType.Silence:
                return $"Prevent the unit from applying any effects {this.generateDurationDescription()}.";
            case EffectType.Endure:
                return $"Prevent the units' HP from reducing below 1 {this.generateDurationDescription()}.";
            case EffectType.Bomb:
                if(stackCount > 1)
                return $"Explodes in {stackCount} turns and deals 25 damage."; //damage is undecided
                else return $"Explodes in {stackCount} turn and deals 25 damage.";
            case EffectType.Stun:
                if(stackCount > 1)
                return $"The unit can not make moves for {stackCount} turns.";
                else return $"The unit can not make moves for {stackCount} turn.";
            case EffectType.Confusion:
                return $"Targets for the units' moves are chosen at random {this.generateDurationDescription()}.";
            case EffectType.Cursed:
                return $"The unit takes {stackCount} damage for every move made {this.generateDurationDescription()}.";
            case EffectType.Energized:
                return $"At the start of the turn the unit gains {stackCount} AP {this.generateDurationDescription()}.";
            case EffectType.Exhaust:
                return $"At the start of the turn the unit loses {stackCount} AP {this.generateDurationDescription()}.";
            case EffectType.Bound:
                return $"The unit can make only 1 move AP {this.generateDurationDescription()}.";
            default:
                return "";
        }
        
    }

    public string generateDurationDescription() {
        return $"for {this.stackCount} turns";
    }

}
