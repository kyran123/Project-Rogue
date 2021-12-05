using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerClickHandler {

    [SerializeField]
    public unitDisplay display;

    [SerializeField]
    public string unitName;

    [SerializeField]
    public bool canBeSelected = false;
    public bool canUseEquipmentOn = false;

    [SerializeField]
    public EnemyAI enemyAI;

    public void OnPointerClick(PointerEventData pointerEventData) {
        if(canBeSelected) BattleManager.instance.unitSelected(this);
        if(this.canUseEquipmentOn) BattleManager.instance.equipmentUse(this);
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
        set { this.maxHealth = value; }
    }

    public void modifyHealth(int value) {
        //Positive value = do damage / Negative value = heal
        if(value < 0 && this.hasEffect(EffectType.Heartless)) return;
        if(this.hasEffect(EffectType.Endure) && value >= this.health) {
            if(value > 0) BattleManager.instance.artifactManager.eventTrigger(ArtifactTriggerType.ReceiveDamage, this, (value - (this.health - 1)));
            this.health = 1;
        } 
        else {
            if(value > 0) BattleManager.instance.artifactManager.eventTrigger(ArtifactTriggerType.ReceiveDamage, this, value);
            this.health -= value;
        } 
        if(this.health > this.maxHealth) this.health = this.maxHealth;
        if(this.health <= 0) { //unit dies
            this.cleanse();
            this.die();
            return;
        }

        this.display.updateHealthBar(this.health, this.maxHealth);
        this.display.updateSpeed(this.getSpeed());

        //Send out skill triggers
        if(value < 0) this.EventTrigger(TriggerType.Healed, value);
        this.EventTrigger(TriggerType.HealthAbove, this.Health);
        this.EventTrigger(TriggerType.HealthLower, this.Health);
    }

    #endregion

    #region DAMAGE

    public int bonusDamage = 0;

    public void receiveDamage(float value) {
        //Apply effects to base damage
        if(this.hasEffect(EffectType.Frail)) value *= 1.25f;
        if(this.hasEffect(EffectType.Resistance)) value *= 0.5f;
        if(this.hasEffect(EffectType.Shield)) {
            Effect shield = this.getEffectByType(EffectType.Shield);
            if(value >= shield.stackCount) {
                value -= shield.stackCount;
                this.effects.Remove(shield);
            }
            else {
                shield.stackCount -= (int)value;
                return;
            }
        }

        this.modifyHealth(Mathf.Abs((int)value));
    }

    public void die() {
        Debug.Log($"Unit {this.unitName} dying...");
        this.EventTrigger(TriggerType.OnDeath, 0);
        BattleManager.instance.artifactManager.eventTrigger(ArtifactTriggerType.Death, this);
        this.gameObject.SetActive(false);
        BattleManager.instance.allUnits.Remove(this);
        BattleManager.instance.sortedUnits.Remove(this);
        if(this.isEnemy()) BattleManager.instance.enemyUnits.Remove(this);
        else BattleManager.instance.friendlyUnits.Remove(this);
        BattleManager.instance.checkWinLossCondition();
    }

    public void dealDamage(Unit target, Move move) {
        if(move.damage < 0) {
            target.modifyHealth(move.damage);
            return;
        } 
        if(this.calculateAttackDamage(move.damage) > target.Health){
            this.EventTrigger(TriggerType.OnKill, 0);
            BattleManager.instance.artifactManager.eventTrigger(ArtifactTriggerType.Kill, this);
        } 
        BattleManager.instance.artifactManager.eventTrigger(ArtifactTriggerType.DealDamage, this, this.calculateAttackDamage(move.damage));
        target.receiveDamage(this.calculateAttackDamage(move.damage));
        //Check for effects
        if(target.hasEffect(EffectType.Thorns)) this.receiveDamage(target.getEffectByType(EffectType.Thorns).stackCount);

        if(this.hasEffect(EffectType.Lifesteal)) {
            this.modifyHealth(-Mathf.RoundToInt(this.calculateAttackDamage(move.damage)));
        }
        this.EventTrigger(TriggerType.DamageDealt, move.damage, this, target);
        this.EventTrigger(TriggerType.DamageReceived, move.damage, target, this);
    }

    public int calculateAttackDamage(float damage) {
        if(this.getEffectByType(EffectType.Strength) != null) damage *= 1.25f;
        if(this.getEffectByType(EffectType.Weak) != null) damage *= 0.75f;
        damage += this.bonusDamage;
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
        if(this.actionPoints < 0) this.actionPoints = 0;
        this.EventTrigger(TriggerType.APAbove, this.ActionPoints);
        this.EventTrigger(TriggerType.APLower, this.ActionPoints);
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
        this.EventTrigger(TriggerType.SpeedAbove, this.getSpeed());
        this.EventTrigger(TriggerType.SpeedLower, this.getSpeed());
    } 

    #endregion

    #region MOVES

    [SerializeField]
    public List<Move> moves = new List<Move>();
    
    [SerializeField]    
    public int maxMoves = 3;
    public int getMaxMoves() {
        if(this.hasEffect(EffectType.Bound)) return 1;
        return maxMoves;
    }

    public void addMove(Move move, Card card) {
        if(this.moves.Count >= this.getMaxMoves()) return;
        this.moves.Add(move);
        if(card != null) {
            if(move.damageTargets.Count > 0) this.addTile(card, this, move.damageTargets[0]);
            else this.addTile(card, this);
        } 
        this.EventTrigger(TriggerType.MoveCount, this.moves.Count);
        this.display.updateAllUnitInfo(this);
    }

    public void removeMove(Move move) {
        if(this.moves.Contains(move)) {
            moves.Remove(move);
            this.EventTrigger(TriggerType.MoveCount, this.moves.Count);
        }
    }

    ///<summary> Removes all moves </summary>
    public void removeMoves() {
        if(this.isEnemy()) return;
        this.moves = new List<Move>();
        this.EventTrigger(TriggerType.MoveCount, this.moves.Count);
    }

    public List<Effect> effectsToAdd = new List<Effect>();

    /// <summary> Loops through all moves </summary>
    public void executeMoves() {
        this.effectsToAdd.Clear();
        if(this.hasEffect(EffectType.Bound)) this.moves = new List<Move>(){ this.moves[0] };
        foreach(Move move in this.moves) {
            if(move.damageTargets.Count > 0) this.executeMove(move, 0);
            else this.applyMoveEffects(move);
            this.applyMoveCost(this, move);
        }
        this.addEffectsToFriendly();
        this.display.updateIcons(this.effects, 0);
        this.removeTiles();
        this.removeMoves();
    }

    /// <summary> Execute move on target </summary>
    public void executeMove(Move move, int index) {
        if(!this.hasEffect(EffectType.Stun)) {
            Unit target = move.damageTargets[index];
            if(this.hasEffect(EffectType.Confusion)) {
                if(this.isEnemy()) {
                    target = BattleManager.instance.friendlyUnits[Random.Range(0, BattleManager.instance.friendlyUnits.Count)];
                } else {
                    target = BattleManager.instance.enemyUnits[Random.Range(0, BattleManager.instance.enemyUnits.Count)];
                }
            }
            if(this.hasEffect(EffectType.Cursed)) this.modifyHealth(this.getEffectByType(EffectType.Cursed).stackCount);
            if(move.damage > 0) {
                Unit protectUnit = BattleManager.instance.getProtectUnit(BattleManager.instance.getUnitListByType(target));
                if(protectUnit != null) target = protectUnit;
            }
            this.dealDamage(target, move);
            if(!this.hasEffect(EffectType.Silence)) {
                foreach(Effect effect in move.effects) {
                    if(effect.targetType == targetType.ENEMY) {
                        if(this.isEnemy()) target.addEffect(effect);
                        else this.effectsToAdd.Add(effect); 
                    }
                    if(effect.targetType == targetType.FRIENDLY) {
                        if(this.isEnemy()) this.effectsToAdd.Add(effect);
                        else target.addEffect(effect);
                    }                    
                }
            }
            if(this.hasEffect(EffectType.Stealth)) {
                this.display.removeEffectIcon(this.getEffectByType(EffectType.Stealth));
                this.effects.Remove(this.getEffectByType(EffectType.Stealth));
            }   
        }
        index++;
        if(index < move.damageTargets.Count) this.executeMove(move, index);
    }

    public void applyMoveEffects(Move move) {
        if(!this.hasEffect(EffectType.Silence)) {
            foreach(Effect effect in move.effects) {
                if(effect.targets.Count > 0) {
                    foreach(Unit target in effect.targets) {
                        target.addEffect(effect);
                    }
                } else {
                    this.effectsToAdd.Add(effect);
                }
            }
        }
        if(this.hasEffect(EffectType.Stealth)) {
            this.display.removeEffectIcon(this.getEffectByType(EffectType.Stealth));
            this.effects.Remove(this.getEffectByType(EffectType.Stealth));
        }   
    }

    public void addEffectsToFriendly() {
        if(this.effectsToAdd.Count > 0) { 
            foreach(Effect effect in this.effectsToAdd) {
                if(effect.targets.Count > 0) {
                    foreach(Unit target in effect.targets) {
                        target.addEffect(effect);
                    }
                } else {
                    this.addEffect(effect);
                }                
            }
        }
    }

    /// <summary> Apply the move cost to the unit. Note: Enemies only! </summary>
    public void applyMoveCost(Unit unit, Move move) {
        if(!unit.isEnemy()) return;
        unit.changeActionPoints(move.moveCost.APcost);
        unit.modifySpeed(move.moveCost.speedCost);
        unit.modifyHealth(move.moveCost.HPCost);
        this.display.updateAllUnitInfo(unit);
    }

    #endregion

    #region EFFECTS

    [SerializeField]
    public List<Effect> effects = new List<Effect>();
    public List<Effect> effectsToAddAfter = new List<Effect>();
    
    public void addEffect(Effect effect, bool appliedToItself = false) {
        if(this.hasEffect(EffectType.Immunity)) return;
        if(effect.type == EffectType.Cleanse) {
            this.effects.Clear();
            this.display.updateIcons(this.effects, 0);
            return;
        }
        if(this.hasEffect(effect.type)) {
            this.getEffectByType(effect.type).stackCount += effect.stackCount;
        } else {
            this.effects.Add(new Effect().instantiate(effect));

        }
        this.display.updateIcons(this.effects, 0);
        this.EventTrigger(TriggerType.Effect, effect.type);
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
            this.display.updateIcons(this.effects, this.effects.IndexOf(effect));
            if(effect.stackCount > 0) this.display.edManager.updateDescriptions(this.effects);
            else this.display.edManager.removeDescription(effect);
        }
        foreach(Effect effect in this.effectsToAddAfter) {
            this.addEffect(effect);
        }
        this.effectsToAddAfter.Clear();
    }

    /// <summary> Apply all effects on the unit </summary>
    public void applyEffects(int index) {
        if(this.effects.Count == 0) return;
        Effect effect = this.effects[index];
        if(effect.type != EffectType.Bomb) {
            effect.apply(this);
        }
        index++;
        if(index < this.effects.Count) this.applyEffects(index);
        else this.display.updateIcons(this.effects, 0);
        this.display.edManager.updateDescriptions(this.effects);
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
        this.display.clearIcons();
    }

    #endregion

    #region TILES
    
    public List<Card> cards = new List<Card>();
    public List<Movedisplay> tiles = new List<Movedisplay>();

    public void addTile(Card card, Unit selectedUnit = null, Unit targetedUnit = null) {
        card.cardBehavior.handManager = BattleManager.instance.handManager;
        this.cards.Add(card);
        int index = this.cards.Count - 1;
        this.tiles[index].updateCardData(card, selectedUnit, targetedUnit);
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

    public void EventTrigger(TriggerType type, int value) {
        foreach(Skill skill in this.skills) {
            skill.eventTrigger(type, this, this, value);
        }
    }

    public void EventTrigger(TriggerType type, EffectType effectType) {
        foreach(Skill skill in this.skills) {
            skill.eventTrigger(type, effectType, this);
        }
    }

    public void EventTrigger(TriggerType type, int value, Unit unit, Unit target) {
        foreach(Skill skill in unit.skills) {
            if(skill.target == targetType.ENEMY) skill.eventTrigger(type, target, unit, value);
            else skill.eventTrigger(type, unit, target, value);
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
