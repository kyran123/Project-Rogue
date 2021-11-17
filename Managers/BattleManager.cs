using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class BattleManager : MonoBehaviour {

    [Header("Game Managers")]
    [SerializeField]
    public static BattleManager instance;
    public HandManager handManager;
    public PileManager pileManager;
    public MenuManager menuManager;
    
    [Header("Unit lists")]
    [SerializeField]
    public List<Unit> friendlyUnits;
    public List<Unit> enemyUnits;
    public List<Unit> allUnits = new List<Unit>();
    public List<Unit> sortedUnits;


    [Header("Other")]
    [SerializeField]
    public bool battleIsOver = false;
    public int drawCount = 0;
    public int actionPointsPerTurn = 2;
    public int drawPerTurn = 3;


    #region BATTLE SETUP

    void Start() {
        BattleManager.instance = this;
        gpManager = new GamePhaseManager();
        gpManager.battleManager = this;
        List<Unit> units = this.getAllUnits();
        this.allUnits.AddRange(units);
        this.setupUnits(units);
        this.pileManager.updateCardCounts();
        this.resetSortedList();
        this.sortList(this.allUnits);
    }

    /// <summary> Gets all units and returns the list </summary>
    public List<Unit> getAllUnits() {
        List<Unit> temp = new List<Unit>();
        temp.AddRange(friendlyUnits);
        temp.AddRange(enemyUnits);
        return temp;
    }

    /// <summary> Updates all stats of the units at the start of the battle </summary>
    public void setupUnits(List<Unit> units) {
        foreach(Unit unit in units) {
            unit.receiveDamage(Random.Range(1f, 15f)); //Temporary
            unit.display.updateAP(unit.ActionPoints);
            unit.display.updateSpeed(unit.getSpeed());
            unit.display.updateHealthBar(unit.Health, unit.MaxHealth);
            unit.display.updateIcons(unit.effects);
            unit.display.edManager.updateDescriptions(unit.effects);
        }
    }

    #endregion

    #region SORT LISTS

    /// <summary> Empties and refills the sorted units list </summary>
    public void resetSortedList() {
        this.sortedUnits.Clear();
        this.sortedUnits.AddRange(friendlyUnits);
        this.sortedUnits.AddRange(enemyUnits);
    }
    
    /// <summary> Sorts list of units by speed (descending) </summary>
    public void sortList(List<Unit> units) {
        this.sortedUnits = units.OrderByDescending(u => u.getSpeed()).ToList();
        this.allUnits = this.allUnits.OrderByDescending(u => u.getSpeed()).ToList();
        foreach(Unit unit in this.allUnits) {
            unit.display.updateSpeed(unit.getSpeed());
            unit.display.showOrder(this.allUnits.IndexOf(unit) + 1);
        }
    }

    #endregion

    #region END TURN

    public void EndTurn() {
        this.handManager.discardCards();
        this.unitTurn();
    }

    public void unitTurn() {
        Unit unit = this.getNextUnit();
        unit.reduceEffectCount();

        if(this.battleIsOver) return;
        if(unit.isEnemy()) unit.enemyAI.MoveSelection(unit, this.friendlyUnits);

        if(unit.moves.Count > 0) unit.executeMoves();
        this.sortedUnits.Remove(unit);

        if(this.sortedUnits.Count > 0) this.unitTurn();
        else this.finishUnitTurns();
    }

    /// <summary> Returns the first Unit in list sorted by speed. </summary>
    public Unit getNextUnit() {
        this.sortList(this.sortedUnits);
        return this.sortedUnits[0];
    }

    /// <summary> Resets unit lists, applies effects and updates unit information displays </summary>
    public void finishUnitTurns() {
        this.resetSortedList();
        foreach(Unit unit in this.allUnits) {
            unit.applyEffects();
            unit.changeActionPoints(-this.actionPointsPerTurn);
            unit.display.updateAP(unit.ActionPoints);
        }
        this.handManager.drawCard(this.drawPerTurn);
        this.drawCount = 0;
    }

    #endregion

    #region Cards

    public void drawCard(int count) {
        this.handManager.drawCard(count);
    }

    public void discardCard(Card card) {
        this.handManager.discardCard(card);
    }

    public void consumeCard(Card card) {
        this.handManager.removeCard(card);
        this.gpManager.selectedCard = null;
        this.handManager.selectedCard = null;
        Destroy(card.gameObject);
    }

    #endregion

    #region Units
    /// <summary> If target of a move is still alive </summary>
    public bool isTargetAvailable(Unit target, Move move) {
        if(target.Health <= 0) {
            move.damageTargets.Remove(target);
            return false;
        }
        return true;
    }

    /// <summary> Returns the unit with protect effect type from given list </summary>
    public Unit getProtectUnit(List<Unit> units) {
        foreach(Unit unit in units) {
            if(unit.getEffectByType(EffectType.Protect) != null) return unit;
        }
        return null;
    }

    /// <summary> Get friendlies or enemies list depending on which the given unit is in </summary>
    public List<Unit> getUnitListByType(Unit unit) {
        if(this.friendlyUnits.Contains(unit)) return this.friendlyUnits;
        else return this.enemyUnits;
    }

    #endregion

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

    [Space(10)]
    [SerializeField]
    public GamePhaseManager gpManager;

    public void setGamePhase(phase value) {
        if(this.gpManager.gamePhase == value) return;
        this.gpManager.gamePhase = value;
        this.gpManager.onPhaseUpdate();                
    }

    public void addSelectedCard(Card card) {
        this.gpManager.selectedCard = card;
    }
    
    /// <summary> Sets the given unit as target or executer based on the game phase </summary>
    public void unitSelected(Unit unit) {
        if(this.gpManager.GamePhase == phase.Target) {
            this.gpManager.setTargets(this.gpManager.selectedCard.Move, new List<Unit>(){ unit }, targetType.ENEMY);
            this.setGamePhase(phase.End);
        }
        if(this.gpManager.GamePhase == phase.Select) {
            this.gpManager.selectedUnit = unit;
            this.setGamePhase(phase.Target);
        }
    }
}