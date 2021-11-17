using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    [SerializeField]
    public unitDisplay display;

    [SerializeField]
    public string unitName;

    [SerializeField]
    public bool canBeSelected = false;

    [SerializeField]
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
    
    [SerializeField]
    protected int maxHealth;
    public int MaxHealth { 
        get { return this.maxHealth; } 
    }

    public void modifyHealth(int value) {
        //Positive value = do damage / Negative value = heal
        if(value < 0 && this.getEffectByType(EffectType.Heartless) == null) return;
        if(this.getEffectByType(EffectType.Endure) != null && value >= this.health) this.health = 1;
        else this.health -= value;
        if(this.health > this.maxHealth) this.health = this.maxHealth;
        if(this.health <= 0) { //unit dies
            this.health = 0;
            this.cleanse();
            this.die();
            return;
        }

        this.display.updateHealthBar(this.health, this.maxHealth);
        this.display.updateSpeed(this.getSpeed());

        //Send out skill triggers
        if(value < 0) this.EventTrigger(TriggerType.Healed, value);
        else this.EventTrigger(TriggerType.DamageReceived, value);
        this.EventTrigger(TriggerType.HealthAbove);
        this.EventTrigger(TriggerType.HealthLower);
    }

    public void receiveDamage(float value) {
        //Apply effects to base damage
        if(this.getEffectByType(EffectType.Frail) != null) value *= 1.25f;
        if(this.getEffectByType(EffectType.Resistance) != null) value *= 0.5f;
        this.modifyHealth((int)value);
    }

    public void die() {
        this.EventTrigger(TriggerType.OnDeath);
        this.gameObject.SetActive(false);
        BattleManager.instance.allUnits.Remove(this);
        BattleManager.instance.sortedUnits.Remove(this);
        if(this.isEnemy()) BattleManager.instance.enemyUnits.Remove(this);
        else BattleManager.instance.friendlyUnits.Remove(this);
        BattleManager.instance.sortList(BattleManager.instance.allUnits);
        BattleManager.instance.checkWinLossCondition();
    }

    public void dealDamage(Unit target, Move move) {
        if(this.calculateAttackDamage(move.damage) > target.Health) this.EventTrigger(TriggerType.OnKill);
        target.receiveDamage(this.calculateAttackDamage(move.damage));
        //Check for effects
        Effect effect = this.getEffectByType(EffectType.Lifesteal);
        if(effect != null) {
            this.modifyHealth(-Mathf.RoundToInt(this.calculateAttackDamage(move.damage) * 0.2f));
        }
        this.EventTrigger(TriggerType.DamageDealt, move.damage);
    }

    public int calculateAttackDamage(float damage) {
        if(this.getEffectByType(EffectType.Strength) != null) damage *= 1.25f;
        if(this.getEffectByType(EffectType.Weak) != null) damage *= 0.75f;
        return (int)damage;
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
        //Positive value = remove AP / Negative alue = add AP
        this.actionPoints -= value;
        if(this.actionPoints > this.MaxActionPoints) this.actionPoints = this.MaxActionPoints;
        this.EventTrigger(TriggerType.APAbove);
        this.EventTrigger(TriggerType.APLower);
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
        Skill speedSkill = this.getSkillByType(SkillType.Speed);
        if(speedSkill != null) {
            if(speedSkill.isValid(this)) {
                value -= this.getSkillByType(SkillType.Speed).skillBonus;
            }
        }
        foreach(Effect effect in effects) {
            if(effect.type == EffectType.Haste) value += Mathf.RoundToInt(this.speed * 0.25f);
            if(effect.type == EffectType.Slow) value -= Mathf.RoundToInt(this.speed * 0.25f);
        }
        return value;
    }

    public void modifySpeed(int value) {
        //Positive value = reduce speed / Negatie value = increase speed
        this.speed -= value;
        this.EventTrigger(TriggerType.SpeedAbove);
        this.EventTrigger(TriggerType.SpeedLower);
    } 

    #endregion

    #region MOVES

    [SerializeField]
    public List<Move> moves = new List<Move>();

    public int maxMoves = 3;

    public void addMove(Move move, Card card) {
        if(this.moves.Count >= maxMoves) return;
        this.moves.Add(move);
        if(card != null) this.addTile(card);
        this.EventTrigger(TriggerType.MoveCount);
        this.display.updateAllUnitInfo(this);
    }

    public void removeMove(Move move) {
        if(this.moves.Contains(move)) {
            moves.Remove(move);
            this.EventTrigger(TriggerType.MoveCount);
        }
    }

    ///<summary> Removes all moves </summary>
    public void removeMoves() {
        this.moves = new List<Move>();
        this.EventTrigger(TriggerType.MoveCount);
    }

    /// <summary> Loops through all moves </summary>
    public void executeMoves() {
        foreach(Move move in this.moves) {
            this.executeMove(move);
            this.applyMoveCost(this, move);
        }
        this.removeTiles();
        this.removeMoves();
    }

    /// <summary> Execute move on target </summary>
    public void executeMove(Move move) {
        Unit target = move.damageTargets[0];
        if(!BattleManager.instance.isTargetAvailable(target, move)) {
            this.executeMove(move); 
            return;   
        } 
        Unit protectUnit = BattleManager.instance.getProtectUnit(BattleManager.instance.getUnitListByType(target));
        if(protectUnit != null) target = protectUnit;
        this.dealDamage(target, move);
        if(move.hasEffectByType(EffectType.Cleanse)) target.cleanse();
    }

    /// <summary> Apply the move cost to the unit. Note: Enemies only! </summary>
    public void applyMoveCost(Unit unit, Move move) {
        if(!unit.isEnemy()) return;
        unit.changeActionPoints(move.moveCost.APcost);
        unit.modifySpeed(move.moveCost.speedCost);
        unit.modifyHealth(move.moveCost.HPCost);
        updateAllUnitInfo(unit);
    }

    #endregion

    #region EFFECTS

    [SerializeField]
    public List<Effect> effects = new List<Effect>();

    public void addEffect(Effect effect) {
        if(effect.type == EffectType.Cleanse) return;
        if(this.effects.Contains(effect)) {
            int index = this.effects.IndexOf(effect);
            this.effects[index].stackCount += effect.stackCount;
        }
        this.effects.Add(effect);
        this.display.updateIcons(this.effects);
        this.EventTrigger(TriggerType.Effect);
    }

    public Effect getEffectByType(EffectType type) {
        foreach(Effect effect in this.effects) {
            if(effect.type == type) {
                return effect;
            }
        }
        return null;
    }

    public bool hasEffect(EffectType effectType) {
        foreach(Effect effect in this.effects) {
            if(effect.type == effectType) return true;
        }
        return false;
    }

    /// <summary> Reduces stackcount of all effects on given unit </summary>
    public void reduceEffectCount() {
        foreach(Effect effect in this.effects) {
            effect.reduceStackCount(this);
        }
    }

    /// <summary> Apply all effects on the unit </summary>
    public void applyEffects() {
        foreach(Effect effect in this.effects) {
            effect.apply(this);
        }
        this.removeFinishedEffects();
        this.display.updateIcons(this.effects);
    }

    public void removeFinishedEffects() {
        for(int i = this.effects.Count; i > 0; i--) {
            Effect effect = this.effects[i-1];
            if(effect.stackCount == 0) {
                this.display.removeEffectIcon(effect);
                this.display.edManager.removeDescription(effect);
                this.effects.Remove(effect);
            }
        }
    }

    public void ApplyEffectsToTargets(Move move) {
        foreach(Effect effect in move.effects) {
            foreach(Unit target in effect.targets) {
                target.addEffect(effect); //TODO: Check if unit is immune to effects
                target.display.updateSpeed(target.getSpeed());
            }
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
    
    #region SKILLS

    public List<Skill> skills = new List<Skill>();

    public void EventTrigger(TriggerType type, int value = 0) {
        foreach(Skill skill in this.skills) {
            skill.eventTrigger(type, this, value);
        }
    }

    public Skill getSkillByTriggerType(TriggerType type) {
        foreach(Skill skill in this.skills) {
            if(skill.getTriggerType() == type) return skill;
        }
        return null;
    }

    public Skill getSkillByType(SkillType type) {
        foreach(Skill skill in this.skills) {
            if(skill.skillType == type) return skill;
        }
        return null;
    }
    
    #endregion
}
