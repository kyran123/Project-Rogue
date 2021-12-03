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

    public equipmentType type;
    public int amount;

    public Effect effect;

    public targetType targetType;
    public bool isRandom;

    public GameObject descriptionObject;
    public TMP_Text description;

    public GameObject highlight;
    public bool isHighlighted;

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
        this.isHighlighted = !this.isHighlighted;
        this.highlight.SetActive(this.isHighlighted);
        if(this.isHighlighted) BattleManager.instance.highlightTargets(this.targetType);
        else BattleManager.instance.unHighlightTargets();
    }

    public void use(List<Unit> targets, Unit selectedUnit) {
        if(this.targetType == targetType.FRIENDLY || this.targetType == targetType.ENEMY) {
            targets.Clear();
            targets.Add(selectedUnit);
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
