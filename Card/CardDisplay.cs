using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour {

    public Image graphics;
    public TMP_Text title;

    public GameObject APCost;
    public Image APCostGraphic;
    public TMP_Text APCostText;

    public GameObject SpeedCost;
    public Image SpeedCostGraphic;
    public TMP_Text SpeedCostText;

    public GameObject HPCost;
    public Image HPCostGraphic;
    public TMP_Text HPCostText;

    public TMP_Text description;

    Unit selectedUnit;

    public void updateCardDisplay(Card card, Unit selectedUnit = null, Unit targetedUnit = null) {
        if(selectedUnit != null) this.selectedUnit = selectedUnit;
        this.title.text = card.title;

        if(card.APCost > 0) APCost.SetActive(true);
        else APCost.SetActive(false);
        APCostText.text = $"{card.APcost}";

        if(card.speedCost > 0) SpeedCost.SetActive(true);
        else SpeedCost.SetActive(false);
        SpeedCostText.text = $"{card.speedCost}";

        if(card.HPCost > 0) HPCost.SetActive(true);
        else HPCost.SetActive(false);
        HPCostText.text = $"{card.HPCost}";

        string desc = "";
        if(card.APcost < 0) desc += $"Gain {Mathf.Abs(card.APcost)} AP. ";
        if(card.Move.damage > 0) desc += $"Deal {this.calculateDamage(card.Move, this.selectedUnit, targetedUnit)} damage. ";
        if(card.speedCost != 0) {
            if(card.speedCost > 0) desc += $"Lose {card.speedCost} speed. ";
            else desc += $"Increase speed by {Mathf.Abs(card.speedCost)}. ";
        }
        if(card.HPCost != 0) {
            if(card.HPCost > 0) desc += $"Take {card.HPCost} damage. ";
            else desc+= $"Heal by {Mathf.Abs(card.HPCost)} HP. ";
        }
        desc += this.calculateEffect(card.Move, this.selectedUnit, targetedUnit);
        description.text = desc;
    }

    public string calculateDamage(Move move, Unit selectedUnit = null, Unit targetedUnit = null) {
        float totalDamage = move.damage;
        if(selectedUnit != null) {
            if(selectedUnit.hasEffect(EffectType.Strength)) totalDamage *= 1.25f;
            if(selectedUnit.hasEffect(EffectType.Weak)) totalDamage *= 0.75f;
        }
        if(targetedUnit != null) {
            if(targetedUnit.hasEffect(EffectType.Frail)) totalDamage *= 1.25f;
            if(targetedUnit.hasEffect(EffectType.Resistance)) totalDamage *= 0.5f;
        }
        totalDamage = Mathf.Abs((int)totalDamage);
        string text = totalDamage.ToString();
        if(move.damage != totalDamage) text = $"<color=green>{totalDamage}</color>";
        return text;
    }

    public string calculateEffect(Move move, Unit selectedUnit = null, Unit targetedUnit = null) {
        string effectText = "";
        foreach(Effect effect in move.effects) {
            effectText += $"{effect.generateDescription()}. ";
        }
        return effectText;
    }
}
