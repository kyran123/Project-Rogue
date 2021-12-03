using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Movedisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public TMP_Text title;
    public GameObject hover;

    public Image graphics;

    public GameObject APCost;
    public TMP_Text APCostText;

    public GameObject SpeedCost;
    public TMP_Text SpeedCostText;

    public GameObject HPCost;
    public TMP_Text HPCostText;

    public TMP_Text description;


    public void OnPointerEnter(PointerEventData pointerEventData) {
        hover.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        hover.SetActive(false);        
    }

    public void updateCardData(Card card, Unit selectedUnit = null, Unit targetedUnit = null) {
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
        if(card.Move.damage > 0) desc += $"Deal {this.calculateDamage(card.Move, selectedUnit, targetedUnit)} damage. ";
        if(card.speedCost != 0) {
            if(card.speedCost > 0) desc += $"Lose {card.speedCost} speed. ";
            else desc += $"Increase speed by {Mathf.Abs(card.speedCost)}. ";
        }
        if(card.HPCost != 0) {
            if(card.HPCost > 0) desc += $"Take {card.HPCost} damage. ";
            else desc+= $"Heal by {Mathf.Abs(card.HPCost)} HP. ";
        }
        desc += this.calculateEffect(card.Move, selectedUnit, targetedUnit);
        description.text = desc;
    }

    public int calculateDamage(Move move, Unit selectedUnit = null, Unit targetedUnit = null) {
        float totalDamage = move.damage;
        if(selectedUnit != null) {
            if(selectedUnit.hasEffect(EffectType.Strength)) totalDamage *= 1.25f;
            if(selectedUnit.hasEffect(EffectType.Weak)) totalDamage *= 0.75f;
        }
        if(targetedUnit != null) {
            if(targetedUnit.hasEffect(EffectType.Frail)) totalDamage *= 1.25f;
            if(targetedUnit.hasEffect(EffectType.Resistance)) totalDamage *= 0.5f;
        }
        return (int)totalDamage;
    }

    public string calculateEffect(Move move, Unit selectedUnit = null, Unit targetedUnit = null) {
        string effectText = "";
        foreach(Effect effect in move.effects) {
            effectText += $"{effect.generateDescription()}. ";
        }
        return effectText;
    }
}
