using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    [SerializeField]
    public unitDisplay display;
    
    public string unitName;

    public bool canBeSelected = false;

    public EnemyAI enemyAI;

    public void OnPointerClick(PointerEventData pointerEventData) {
        if(canBeSelected) BattleManager.instance.unitSelected(this);
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
        this.display.edManager.updateDescriptions(this.effects);
        this.display.edManager.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        this.display.edManager.gameObject.SetActive(false);
    }

    public void selectUnit(Card card) {
        if(card.APCost > 0) this.changeActionPoints(card.APCost);
        if(card.speedCost != 0) this.modifySpeed(card.speedCost);
        if(card.HPCost != 0) this.modifyHealth(card.HPCost);
    }

    public bool isEnemy() {
        return enemyAI != null;
    }

    #region HEALTH
    [SerializeField]
    protected int health;
    public int Health {
        get { return this.health; }
        set { this.health = value; }
    }
    public void modifyHealth(int value) {
        this.health -= value; //doing damge or healing
        if(this.health <= 0) { //unit dies
            this.health = 0;
            this.die();
        }
        this.display.updateHealthBar(this.health, this.maxHealth);
    }

    public void receiveDamage(float value) {
        //Apply effects to base damage
        if(this.getEffectByType(EffectType.Frail) != null) value *= 1.25f;
        if(this.getEffectByType(EffectType.Resistance) != null) value *= 0.5f;
        this.health -= (int)value;
        if(this.health <= 0) { //unit dies
            this.health = 0;
            this.die();
        }
        this.display.updateHealthBar(this.health, this.maxHealth);
    }

    public void die() {
        this.gameObject.SetActive(false);
        BattleManager.instance.allUnits.Remove(this);
        BattleManager.instance.sortedUnits.Remove(this);
        if(this.isEnemy()) BattleManager.instance.enemyUnits.Remove(this);
        else BattleManager.instance.friendlyUnits.Remove(this);
        BattleManager.instance.sortList(BattleManager.instance.allUnits);
        BattleManager.instance.checkWinLossCondition();
    }

    [SerializeField]
    protected int maxHealth;
    public int MaxHealth { 
        get { return this.maxHealth; } 
    }
    #endregion

    #region ACTIONPOINTS
    [SerializeField]
    protected int actionPoints;
    public int ActionPoints {
        get { return this.actionPoints; }
    }
    [SerializeField]
    protected int maxActionPoints;
    public int MaxActionPoints {
        get { return this.maxActionPoints; }
    }

    public void changeActionPoints(int value) {
        this.actionPoints -= value;
        if(this.actionPoints > this.MaxActionPoints) this.actionPoints = this.MaxActionPoints;
    }
    #endregion

    #region SPEED
    [SerializeField]
    protected int speed;
    public int Speed {
        get {
            return this.speed;
        }
    }

    public int getSpeed() {
        int value = this.speed;
        foreach(Effect effect in effects) {
            if(effect.type == EffectType.Haste) value += Mathf.RoundToInt(this.speed * 0.25f);
            if(effect.type == EffectType.Slow) value -= Mathf.RoundToInt(this.speed * 0.25f);
        }
        return value;
    }


    public void modifySpeed(int value) {
        this.speed -= value; //Negative speed is positive
    } 
    #endregion

    #region MOVES
    [SerializeField]
    public List<Move> moves = new List<Move>();

    public int maxMoves = 3;

    public void addMove(Move move) {
        if(this.moves.Count >= maxMoves) return;
        this.moves.Add(move);
    }

    public void removeMove(Move move) {
        if(this.moves.Contains(move)) {
            moves.Remove(move);
        }
    }
    public void removeMoves() {
        this.moves = new List<Move>();
    }

    public int calculateAttackDamage(float damage) {
        if(this.getEffectByType(EffectType.Strength) != null) damage *= 1.25f;
        if(this.getEffectByType(EffectType.Weak) != null) damage *= 0.75f;
        return (int)damage;
    }
    #endregion

    #region EFFECTS
    [SerializeField]
    public List<Effect> effects = new List<Effect>();

    public void addEffect(Effect effect) {
        if(effect.type == EffectType.Cleanse) return;
        this.effects.Add(effect);
        this.display.updateIcons(this.effects);
    }

    public Effect getEffectByType(EffectType type) {
        foreach(Effect effect in this.effects) {
            if(effect.type == type) {
                return effect;
            }
        }
        return null;
    }

    public void dealDamage(Unit target, Move move) {
        target.receiveDamage(this.calculateAttackDamage(move.damage));

        Effect effect = this.getEffectByType(EffectType.Lifesteal);
        if(effect != null) {
            this.modifyHealth(-Mathf.RoundToInt(this.calculateAttackDamage(move.damage) * 0.2f));
            effect.stackCount--;
        }
    }

    public void cleanse() {
        this.effects.Clear();
    }
    #endregion

    #region TILES
    
    public List<Card> cards = new List<Card>();
    public List<Movedisplay> tiles = new List<Movedisplay>();

    public void addTile(Card card) {
        this.cards.Add(card);
        int index = this.cards.Count - 1;
        this.tiles[index].updateCardData(card);
        this.tiles[index].gameObject.SetActive(true);
    }

    public void removeTiles() {
        foreach(Movedisplay tile in this.tiles) {
            tile.gameObject.SetActive(false);
        }
        this.cards.Clear();
    }
    #endregion
    
}
