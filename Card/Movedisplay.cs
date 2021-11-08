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

    public void updateCardData(Card card) {
        this.title.text = card.title;

         if(card.APCost > 0) {
            APCost.SetActive(true);
            APCostText.text = $"{card.APCost}";
        } 
        else APCost.SetActive(false);

        if(card.speedCost > 0) {
            SpeedCost.SetActive(true);
            SpeedCostText.text = $"{card.speedCost}";
        } 
        else SpeedCost.SetActive(false);

        if(card.HPCost > 0) {
            HPCost.SetActive(true);
            HPCostText.text = $"{card.HPCost}";
        } 
        else HPCost.SetActive(false);

        string desc = "";
        if(card.Move.damage > 0) desc += $"Deal {card.Move.damage} damage.";
        if(card.speedCost < 0) desc += $"+{-card.speedCost} speed.";
        if(card.HPCost < 0) desc += $"+{-card.HPCost} HP";
        foreach(Effect effect in card.Move.effects) {
            desc += $"{effect.generateDescription()}";
        }
        description.text = desc;
    }
}
