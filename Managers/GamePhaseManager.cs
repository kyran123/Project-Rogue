using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public enum phase {
    Pick, //Card picking phase
    Select, //Friendly unit selection for executing move
    Target, //Target of the move selection
    End //End phase to finish logic
}

[System.Serializable]
public class GamePhaseManager {

    [SerializeField]
    public BattleManager battleManager;

    [SerializeField]
    [Header("Game phases")]
    public phase gamePhase = phase.Pick;
    public phase GamePhase {
        get { return this.gamePhase; }
        set {
            this.gamePhase = value;
        }
    }
    [Header("Selected objects")]
    public Unit selectedUnit = null;
    public Card selectedCard = null;

    
    /// <summary> Whenever the game phase is changed this function is called </summary>
    public void onPhaseUpdate() {
        this.unHighlightTargets();
        Move move = this.selectedCard.Move;
        switch(this.GamePhase) {
            case phase.Pick:
                this.reset();
                break;
            case phase.Select:
                this.highlightTargets(battleManager.friendlyUnits);
                break;
            case phase.Target:
                if(this.hasSingleTarget(move)) {
                    this.highlightTargets(battleManager.enemyUnits);
                } else {
                    this.setTargets(move, this.battleManager.enemyUnits, targetType.ALLENEMIES);
                    this.setTargets(move, this.battleManager.friendlyUnits, targetType.ALLFRIENDLIES);
                    this.GamePhase = phase.End;
                    this.onPhaseUpdate();
                }
                break;
            case phase.End:
                this.selectedUnit.addMove(move, this.selectedCard);
                this.gamePhase = phase.Pick;
                this.unHighlightTargets();
                if(this.selectedCard.consumable) {
                    this.battleManager.consumeCard(this.selectedCard);
                } else {
                    this.battleManager.discardCard(this.selectedCard);
                }
                this.reset();
                this.battleManager.sortList(this.battleManager.sortedUnits);
                break;
        }
    }

    public void reset() {
        this.selectedCard = null;
        this.selectedUnit = null;
    }

    /// <summary> Sets the target unit in list as the damage target for the move </summary>
    public void setTargets(Move move, List<Unit> targets, targetType type) {
        if(!this.hasDamageTargetType(move, type)) return;
        move.damageTargets = targets;
        foreach(Effect effect in move.effects) {
            if(effect.targetType == type) effect.targets = targets;
        }
    }

    /// <summary> Checks if the move has the correct damageTargetType </summary>
    public bool hasDamageTargetType(Move move, targetType type) {
        if(move.damageTargetType == type) return true;
        return false;
    }

    /// <summary> Checks if the move has the correct effectTargetType </summary>
    public bool hasEffectTargetType(List<Effect> effects, targetType targetType) {
        foreach(Effect effect in effects) {
            if(effect.targetType == targetType) return true;
        }
        return false;
    }

    /// <summary> Returns boolean if the move has is meant for a single target </summary>
    public bool hasSingleTarget(Move move) {
        if(move.damageTargetType == targetType.ENEMY) return true;
        return false;
    }

    public void highlightTargets(List<Unit> targets) {
        Card card = this.selectedCard;
        foreach(Unit target in targets) {
            if(this.gamePhase == phase.Select) {
                if(
                    !target.hasEffect(EffectType.Stun) && 
                    target.ActionPoints >= card.APCost && 
                    target.Speed >= card.speedCost &&
                    target.moves.Count < target.getMaxMoves()
                ) {
                    target.display.setHighlight(true);
                    target.canBeSelected = true;
               }
            }
            if(this.gamePhase == phase.Target) {
                //Check for effects that negate your attack
                if(!target.hasEffect(EffectType.Stealth)) {
                    target.display.setHighlight(true);
                    target.canBeSelected = true;
                }
            }
        }
    }

    public void unHighlightTargets() {
        List<Unit> units = new List<Unit>();
        units.AddRange(this.battleManager.friendlyUnits);
        units.AddRange(this.battleManager.enemyUnits);

        foreach(Unit unit in units) {
            unit.display.setHighlight(false);
            unit.canBeSelected = false;
        }
    }
}
