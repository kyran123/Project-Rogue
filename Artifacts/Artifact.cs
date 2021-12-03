using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public enum ArtifactType {
    MaxHealth, //Increase unit (max)health
    Heal, //Heal unit
    modifyDamage, //Buff or Reduce damage on an unit
    Damage, //Damage unit
    Draw, //Draw a card
    Effect, //Apply effect OR buff effect stackCount
    Speed, //Modify speed
    AP, //Modify AP
    Equipment, //Gain equipment
    Reward, //Reward bonus
    CreateCard, //Create a completely random card
    CardCost, //Manipulate card cost
}

public class Artifact : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    #region Variables
    [Header("Trigger selection")]
    public ArtifactTrigger trigger;

    [Header("Artifact bonus")]
    public ArtifactType type;
    public int amount = 0;
    public ArtifactCreateCard createCard;
    public ArtifactCardCost cardCost;
    public Effect effect;

    [Header("Chance of trigger")]
    public bool isChanceBased;
    [Tooltip("% chance of it triggering")]
    [Range(0, 100)]
    public int chance = 0;

    [Header("Target selection")]
    public targetType tType;
    [Tooltip("Should a single target be randomly selected. Make sure to select FRIENDLY or ENEMY in tType variable")]
    public bool isRandom;

    [Space(10)]
    [Header("Game objects")]
    public GameObject DescriptionObject;
    public TMP_Text description;
    #endregion

    void Start() {
        this.description.text = $"{this.trigger.getDescription()} {this.getDescription()}";
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
        DescriptionObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        DescriptionObject.SetActive(false);
    }

    public void eventTrigger(ArtifactTriggerType type, Unit unit, int value) {
        if(type != this.trigger.type) return;
        if(this.trigger.type == ArtifactTriggerType.Turn) {
            if(value == 0 && !this.trigger.atStartofTurn) return;
            if(value == 1 && this.trigger.atStartofTurn) return;
        }
        if(this.trigger.isValid(unit, value)) this.apply(unit);
    }

    public void apply(Unit triggerUnit) {
        if(!this.applyGlobal()) {
            List<Unit> targets = this.getTargetsFromType(triggerUnit);
            foreach(Unit unit in targets) {
                switch(this.type) {
                    case ArtifactType.MaxHealth:
                        unit.MaxHealth += amount;
                        unit.Health += amount;
                        break;
                    case ArtifactType.Heal:
                        unit.modifyHealth(-amount);
                        break;
                    case ArtifactType.modifyDamage:
                        unit.bonusDamage += amount;
                        break;
                    case ArtifactType.Damage:
                        unit.receiveDamage(amount);
                        break;
                    case ArtifactType.Effect:
                        unit.addEffect(this.effect);
                        break;
                    case ArtifactType.Speed:
                        unit.modifySpeed(amount);
                        break;
                    case ArtifactType.AP:
                        unit.changeActionPoints(amount);
                        break;
                }
            }
        }
        this.trigger.isTriggered = true;
    }

    public bool applyGlobal() {
        switch(this.type) {
            case ArtifactType.Draw:
                BattleManager.instance.drawCard(this.amount);
                return true;
            case ArtifactType.Equipment:
                //TODO: add equipment
                return true;
            case ArtifactType.CreateCard:
                Card newCard = null;
                if(this.createCard.useExistingCard) newCard = BattleManager.instance.pileManager.createCard(this.createCard.minimum, this.createCard.maximum);
                else newCard = BattleManager.instance.pileManager.createCard(this.createCard.minimum, this.createCard.maximum, this.createCard.effectCount, this.createCard.consume);
                BattleManager.instance.drawCard(newCard);
                return true;
            case ArtifactType.CardCost:
                BattleManager.instance.getRandomCard().updateCost(this.cardCost);
                return true;
        }
        return false;
    }

    public List<Unit> getTargetsFromType(Unit triggerUnit) {
        BattleManager bm = BattleManager.instance;
        Unit unit;
        switch(tType) {
            case targetType.FRIENDLY:
                unit = triggerUnit;
                if(this.isRandom) {
                    unit = bm.friendlyUnits[Random.Range(0, bm.friendlyUnits.Count - 1)];
                }
                return new List<Unit>() { unit };
            case targetType.ENEMY:
                unit = triggerUnit;
                if(this.isRandom) {
                    unit = bm.enemyUnits[Random.Range(0, bm.enemyUnits.Count - 1)];
                }
                return new List<Unit>() { unit };
            case targetType.ALLFRIENDLIES:
                return bm.friendlyUnits;
            case targetType.ALLENEMIES:
                return bm.enemyUnits;
            case targetType.BOTH:
                return bm.allUnits;
        }
        return new List<Unit>();
    }

    public string getDescription() {
        switch(this.type) {
            case ArtifactType.MaxHealth:
                return $"{this.getPositiveOrNegative()} health of {this.getTargetDescription()} by {this.amount}.";
            case ArtifactType.Heal:
                return $"heal {this.getTargetDescription()} for {this.amount}.";
            case ArtifactType.modifyDamage:
                return $"{this.getPositiveOrNegative()} damage of {this.getTargetDescription()} by {this.amount}.";
            case ArtifactType.Damage:
                return $"deal {this.amount} damage to {this.getTargetDescription()}.";
            case ArtifactType.Draw:
                return $"draw 1 card.";
            case ArtifactType.Effect:
                return $"apply {this.effect.stackCount} {this.effect.type.ToString()} to {this.getTargetDescription()}.";
            case ArtifactType.Speed:
                return $"{this.getPositiveOrNegative()} speed of {this.getTargetDescription()} by {this.amount}.";
            case ArtifactType.AP:
                return $"{this.getPositiveOrNegative()} action points of {this.getTargetDescription()} by {this.amount}.";
            case ArtifactType.Equipment:
                return $"gain 1 equipment.";
            case ArtifactType.Reward:
                return $"gain better rewards.";
            case ArtifactType.CreateCard:
                return $"draw a completely random card.";
            case ArtifactType.CardCost:
                string cardCostString = "reduce card ";
                if(this.cardCost.APCost != 0) cardCostString += $"AP cost by {this.cardCost.APCost}";
                if(this.cardCost.SpeedCost != 0) {
                    if(this.cardCost.APCost != 0) cardCostString += " and ";
                    cardCostString += $"Speed cost by {this.cardCost.SpeedCost}";
                } 
                if(this.cardCost.HPCost != 0) {
                    if(this.cardCost.APCost != 0 || this.cardCost.SpeedCost != 0) cardCostString += " and ";
                    cardCostString += $"Health cost by {this.cardCost.HPCost}";
                }
                return $"{cardCostString}.";
        }
        return "";
    }

    public string getTargetDescription() {
        switch(this.tType) {
            case targetType.FRIENDLY:
                return $"a {this.isRandomTarget()} friendly unit";
            case targetType.ENEMY:
                return $"a {this.isRandomTarget()} enemy unit";
            case targetType.ALLFRIENDLIES:
                return $"all friendly units";
            case targetType.ALLENEMIES:
                return $"all enemy units";
            case targetType.BOTH:
                return $"all units";
        }
        return "";
    }

    public string isRandomTarget() {
        if(this.isRandom) return "random";
        else return "";
    }

    public string getPositiveOrNegative() {
        if(this.amount > 0) return "increase";
        else return "decrease";
    }
}

[System.Serializable]
public class ArtifactCreateCard {
    public bool useExistingCard;
    public int minimum = 0;
    public int maximum = 1;
    public int effectCount = 0;
    public bool consume = false;
}

[System.Serializable]
public class ArtifactCardCost {
    public int APCost;
    public int SpeedCost;
    public int HPCost;
}

