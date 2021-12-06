using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public enum equipmentType {
    Damage,
    Heal,
    Effect,
    AP
}

public class Equipment : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField]
    [Tooltip("Defines the type of equipment it is.")]
    equipmentType type;

    [SerializeField]
    int amount;

    [Tooltip("Do not use targetType in the effect class")]
    [SerializeField]
    Effect effect;

    [Space(10)]
    [SerializeField]
    [Tooltip("The targets that this equipment will be used on! Note: the effect targets are not used here (leave it on NONE)")]
    public targetType targetType;

    [SerializeField]
    [Tooltip("If the target should be randomly chosen. Note: the targetType field HAS to be ENEMY or FRIENDLY!")]
    bool isRandom;

    [SerializeField]
    GameObject descriptionObject;

    [SerializeField]
    TMP_Text description;

    [Space(10)]
    [SerializeField]
    GameObject highlight;

    [SerializeField]
    bool isHighlighted;

    /// <summary> Get the equipment type from this equipment </summary>
    public equipmentType getType() {
        return this.type;
    }

    public bool getHighlightStatus() {
        return this.isHighlighted;
    }

    public void setHighlight(bool status) {
        this.isHighlighted = status;
    }

    void Start() {
        this.description.text = this.getDescription();
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
        descriptionObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        descriptionObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
        this.onHighlight();
    }

    public void onHighlight() {
        this.isHighlighted = !this.isHighlighted;
        if(this.isHighlighted) {
            this.highlight.SetActive(this.isHighlighted);
            BattleManager.instance.highlightTargets(this.targetType);
        } 
        else { 
            this.GetComponentInParent<EquipmentManager>().unHighlightEquipment();
            BattleManager.instance.unHighlightTargets();
        }
    }

    public void use(List<Unit> targets, Unit selectedUnit) {
        if(this.targetType == targetType.FRIENDLY || this.targetType == targetType.ENEMY) {
            targets.Clear();
            if(this.isRandom) {
                BattleManager bm = BattleManager.instance;
                List<Unit> unitsToPick = new List<Unit>();
                if(this.targetType == targetType.FRIENDLY) unitsToPick.AddRange(bm.friendlyUnits);
                if(this.targetType == targetType.ENEMY) unitsToPick.AddRange(bm.enemyUnits);
                targets.Add(unitsToPick[Random.Range(0, unitsToPick.Count)]);
            } else {
                targets.Add(selectedUnit);
            }
        }
        foreach(Unit target in targets) {
            switch(this.type) {
                case equipmentType.Damage:
                    target.receiveDamage(this.amount);
                    break;
                case equipmentType.Heal:
                    target.modifyHealth(-this.amount);
                    break;
                case equipmentType.Effect:
                    target.addEffect(new Effect().instantiate(this.effect));
                    break;
                case equipmentType.AP:
                    target.changeActionPoints(this.amount);
                    break;
            }
        }
        Destroy(this.gameObject);
    }

    public string getDescription() {
        switch(this.type) {
            case equipmentType.Damage:
                return $"Deal {this.amount} damage to {this.getTargetDescription()}.";
            case equipmentType.Heal:
                return $"Heal {this.getTargetDescription()} for {this.amount}";
            case equipmentType.Effect:
                return $"apply {this.effect.stackCount} {this.effect.type.ToString()} to {this.getTargetDescription()}";
            case equipmentType.AP:
                return $"{this.getPositiveOrNegative()} action points of {this.getTargetDescription()} by {this.amount}";
        }
        return "";
    }

    public string getTargetDescription() {
        switch(this.targetType) {
            case targetType.FRIENDLY:
                return $"a friendly unit";
            case targetType.ENEMY:
                return $"an enemy unit";
            case targetType.ALLFRIENDLIES:
                return $"all friendly units";
            case targetType.ALLENEMIES:
                return $"all enemy units";
            case targetType.BOTH:
                return $"all units";
        }
        return "";
    }

    public string getPositiveOrNegative() {
        if(this.amount > 0) return "increase";
        else return "decrease";
    }
}
