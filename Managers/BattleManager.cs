using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleManager : MonoBehaviour {

    public static BattleManager instance;
    public HandManager handManager;
    
    public List<Unit> friendlyUnits;
    public List<Unit> enemyUnits;


    public List<Unit> allUnits = new List<Unit>();
    public List<Unit> sortedUnits;

    public PileManager pileManager;
    public MenuManager menuManager;

    public bool battleIsOver = false;

    void Start() {
        BattleManager.instance = this;
        List<Unit> units = new List<Unit>();
        units.AddRange(friendlyUnits);
        units.AddRange(enemyUnits);
        this.allUnits.AddRange(units);
        foreach(Unit unit in units) {
            unit.receiveDamage(Random.Range(1f, 15f));
            unit.display.updateAP(unit.ActionPoints);
            unit.display.updateSpeed(unit.getSpeed());
            unit.display.updateHealthBar(unit.Health, unit.MaxHealth);
            unit.display.updateIcons(unit.effects);
            unit.display.edManager.updateDescriptions(unit.effects);
        }
        this.pileManager.updateCardCounts();
        this.fillSortedList();
        this.sortList(this.allUnits);
    }

    //Adds units to the list to be sorted
    public void fillSortedList() {
        this.sortedUnits.Clear();
        this.sortedUnits.AddRange(friendlyUnits);
        this.sortedUnits.AddRange(enemyUnits);
    }

    //Sort the unit list
    public void sortList(List<Unit> units) {
        this.sortedUnits = units.OrderByDescending(u => u.getSpeed()).ToList();
        this.allUnits = this.allUnits.OrderByDescending(u => u.getSpeed()).ToList();
        foreach(Unit unit in this.allUnits) {
            unit.display.updateSpeed(unit.getSpeed());
            unit.display.showOrder(this.allUnits.IndexOf(unit) + 1);
        }
    }

    public void EndTurn() {
        this.sortList(this.sortedUnits);
        //Do unit move
        Unit unit = this.sortedUnits[0];
        foreach(Effect effect in unit.effects) {
            effect.reduceStackcount(unit);
        }
        if(this.battleIsOver) return;
        if(unit.isEnemy()) unit.enemyAI.MoveSelection(unit, this.friendlyUnits);

        if(unit.moves.Count > 0) {
            foreach(Move move in unit.moves) {
                foreach(Unit target in move.damageTargets) {
                    Unit targetedUnit = target;
                    if(this.friendlyUnits.Contains(target)) {
                        foreach(Unit protectUnit in this.friendlyUnits) {
                            if(protectUnit.getEffectByType(EffectType.Protect) != null) targetedUnit = protectUnit;
                        }
                    }
                    else {
                        foreach(Unit protectUnit in this.enemyUnits) {
                            if(protectUnit.getEffectByType(EffectType.Protect) != null) targetedUnit = protectUnit;
                        }
                    }
                    unit.dealDamage(targetedUnit, move);
                    if(move.hasEffectByType(EffectType.Cleanse)) targetedUnit.cleanse();
                }
                foreach(Effect effect in move.effects) {
                    foreach(Unit target in effect.targets) {
                        target.addEffect(effect);
                        target.display.updateSpeed(target.getSpeed());
                    }
                }
                if(unit.isEnemy()) {
                    unit.changeActionPoints(move.moveCost.APcost);
                    unit.modifySpeed(move.moveCost.speedCost);
                    unit.modifyHealth(move.moveCost.HPCost);
                    unit.display.updateAP(unit.ActionPoints);
                    unit.display.updateSpeed(unit.getSpeed());
                    unit.display.updateHealthBar(unit.Health, unit.MaxHealth);
                }
            }
            unit.removeTiles();
            unit.removeMoves();
        }
        unit.display.updateSpeed(unit.getSpeed());
        //Remove unit
        this.sortedUnits.Remove(this.sortedUnits[0]);
        if(this.sortedUnits.Count > 0){ 
            this.EndTurn();
        } else {
            this.fillSortedList();
            //for applying effects effectUnit = unit
            foreach(Unit effectUnit in this.allUnits) {
                if(effectUnit.effects.Count > 0) {
                    foreach(Effect effect in effectUnit.effects) {
                        effect.apply(effectUnit);
                        effectUnit.display.updateIcons(effectUnit.effects);
                    }
                    for(int i = effectUnit.effects.Count; i > 0; i--) {
                        if(effectUnit.effects[i-1].stackCount == 0) {
                            effectUnit.display.removeEffectIcon(effectUnit.effects[i-1]);
                            effectUnit.display.edManager.removeDescription(effectUnit.effects[i-1]);
                            effectUnit.effects.RemoveAt(i-1);
                        } 
                    }
                }
                effectUnit.changeActionPoints(-2);
                effectUnit.display.updateAP(effectUnit.ActionPoints);
            }
            for(int i = this.handManager.cards.Count; i > 0; i--) {
                this.discardCard(this.handManager.cards[i-1]);
            }
            this.pileManager.moveCards();
            for(int i = 0; i < 5; i++) {
                Card card = this.pileManager.draw();
                this.handManager.addCard(card);
            }
        }
    }

    public void checkWinLossCondition() {
        if(this.friendlyUnits.Count == 0) {
            this.battleIsOver = true;
            menuManager.LossScreen.SetActive(true);
        } 
        if(this.enemyUnits.Count == 0) {
            this.battleIsOver = true;
            //When a battle is won -> show reward screen
        }
    }

    public enum phase {
        Pick,
        Target,
        Cast1,
        Cast2,
        End
    }
    public phase gamePhase = phase.Pick;
    public phase GamePhase {
        get { return this.gamePhase; }
        set {
            this.gamePhase = value;
        }
    }
    public void setGamePhase(phase value) {
        if(this.gamePhase == value) return;
        this.gamePhase = value;
        this.updatePhase();                
    } 

    public void updatePhase() {
        this.unHighlightTargets();
        Move move = this.selectedCard.Move;
        switch(this.gamePhase) {
            case phase.Pick:
                this.selectedCard = null;
                this.selectedUnit = null;
                break;
            case phase.Target: 
                this.highlightTargets(this.friendlyUnits);
                break;
            case phase.Cast1:
                //Check if we need to select an enemy to apply damage or effects to
                if(move.damageTargetType == targetType.ENEMY || this.hasEffectTargetType(move.effects, targetType.ENEMY)) {
                    this.highlightTargets(this.enemyUnits);
                } else {
                    //If there is no enemy to apply damage or effects to, check if it's AOE
                    if(move.damageTargetType == targetType.ALLENEMIES) move.damageTargets = this.enemyUnits;
                    foreach(Effect effect in move.effects) {
                        if(effect.targetType == targetType.ALLENEMIES) effect.targets = this.enemyUnits;
                    }
                    //Go to next phase
                    this.GamePhase = phase.Cast2;
                    this.updatePhase();
                }                
                break;
            case phase.Cast2:
                //Check if we need to select a friendly unit to apply damage or effects to
                if(move.damageTargetType == targetType.FRIENDLY || this.hasEffectTargetType(move.effects, targetType.FRIENDLY)) {
                    foreach(Effect effect in move.effects) {
                        effect.targets = new List<Unit>(){ this.selectedUnit };
                    }
                    this.GamePhase = phase.End;
                    this.updatePhase();
                } else {
                    //If there is no friendly unit to apply damage or effects to check if it's AOE
                    if(move.damageTargetType == targetType.ALLFRIENDLIES) move.damageTargets = this.friendlyUnits; 
                    foreach(Effect effect in move.effects) {
                        if(effect.targetType == targetType.ALLFRIENDLIES) effect.targets = this.friendlyUnits;
                    } 
                    //Go to next phase  
                    this.GamePhase = phase.End;
                    this.updatePhase();
                }           
                break;
            case phase.End:
                this.selectedUnit.selectUnit(this.selectedCard);
                this.selectedUnit.addMove(move);
                this.selectedUnit.addTile(this.selectedCard);
                this.selectedUnit.display.updateAP(this.selectedUnit.ActionPoints);
                this.selectedUnit.display.updateSpeed(this.selectedUnit.getSpeed());
                this.selectedUnit.display.updateHealthBar(this.selectedUnit.Health, this.selectedUnit.MaxHealth);
                this.gamePhase = phase.Pick;
                this.unHighlightTargets();
                if(this.selectedCard.consumable) {
                    this.consumeCard(this.selectedCard);
                } else {
                    this.discardCard(this.selectedCard);
                }
                this.selectedCard = null;
                this.selectedUnit = null;
                this.sortList(this.sortedUnits);
                break;
        }
    }

    public bool hasEffectTargetType(List<Effect> effects, targetType targetType) {
        foreach(Effect effect in effects) {
            if(effect.targetType == targetType) return true;
        }
        return false;
    }

    public void unitSelected(Unit unit) {
        Move move = this.selectedCard.Move;
        //Check game phase
        if(this.GamePhase == phase.Cast2) {
            //Check if selected unit is selected as target type
            if(move.damageTargetType == targetType.FRIENDLY) move.damageTargets = new List<Unit>(){ unit };
            //Or as effect type
            foreach(Effect effect in move.effects) {
                if(effect.targetType == targetType.FRIENDLY) effect.targets = new List<Unit>(){ unit };
            }
            this.GamePhase = phase.End;
        } 
        if(this.GamePhase == phase.Cast1) {
            if(move.damageTargetType == targetType.ENEMY) move.damageTargets = new List<Unit>(){ unit };
            foreach(Effect effect in move.effects) {
                if(effect.targetType == targetType.ENEMY) effect.targets = new List<Unit>(){ unit };
            }
            this.GamePhase = phase.Cast2;
        }
        if(this.GamePhase == phase.Target) {
            this.selectedUnit = unit;
            this.GamePhase = phase.Cast1;
        }
        this.updatePhase();
    }

    public Unit selectedUnit = null;
    public Card selectedCard = null;

    public void addSelectedCard(Card card) {
        this.selectedCard = card;
    }

    public void highlightTargets(List<Unit> targets) {
        Card card = this.selectedCard;
        foreach(Unit target in targets) {
            if(this.gamePhase == phase.Target) {
                
                if(target.ActionPoints >= card.APCost && target.Speed >= card.speedCost) {
                    target.display.setHighlight(true);
                    target.canBeSelected = true;
               }
            }
            if(this.gamePhase == phase.Cast1) {
                //Check for effects that negate your attack
                target.display.setHighlight(true);
                target.canBeSelected = true;
            }
            if(this.gamePhase == phase.Cast2) {
                //Check for effects that negate your effect
                //target.display.setHighlight(true);
                //target.canBeSelected = true;
            }
        }
    }

    public void unHighlightTargets() {
        List<Unit> units = new List<Unit>();
        units.AddRange(friendlyUnits);
        units.AddRange(enemyUnits);

        foreach(Unit unit in units) {
            unit.display.setHighlight(false);
            unit.canBeSelected = false;
        }
    }

    public void discardCard(Card card) {
        this.handManager.removeCard(card);
        this.pileManager.discardCard(card);
        handManager.selectedCard = null;
        card.GetComponent<CardBehavior>().highlight.SetActive(false);
        card.gameObject.SetActive(false);
    }

    public void consumeCard(Card card) {
        this.handManager.removeCard(card);
        this.selectedCard = null;
        this.handManager.selectedCard = null;
        Destroy(card.gameObject);
    }
}

/*
YOUR TURN:
 0. Draw cards
 1. You apply move cards / spells to your units OR use equipment on enemies
    1.1 You are limited by the amount of action points you have. (x)
 2. You press end turn
 3. Depending on speed, units get selected first (X)
    3.1 execute move that was applied to unit (apply damage & effects from moves) ALWAYS to the first unit. (X)
    3.2 if no move is given to an unit it'll maintain and add action points (?)
    3.3 apply effects to units (x)
 4. Discard you hand
*/

/*
ENEMY TURN:
    1. Enemy unit has action points
        1.1 Enemy unit abilities get weighted score based on certain conditions of the battle
        1.2 Enemy unit abilities weighted score gets adjusted by the unit's personality
        1.3 Enemy selects ability with the highest weighted score (and if any are equal, randomize it) OR decides to save up action points (do nothing)
    2. Apply effects to enemy units
*/

/*
    1. PICK PHASE select the card you want to use
    2. TARGET select the (friendly) unit that will use the card's move
    3. (optionally) IF move damageTargetType or effectTargetType is ENEMY have cast phase 1
    4. (optionally) IF move damageTargetType or effectTargetType is FRIENDLY have cast phase 2
*/