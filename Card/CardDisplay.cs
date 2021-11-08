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

    public void updateCardDisplay(Card card) {
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
        if(card.Move.damage > 0) desc += $"Deal {card.Move.damage} damage. ";
        if(card.speedCost != 0) {
            if(card.speedCost > 0) desc += $"Lose {card.speedCost} speed. ";
            else desc += $"Increase speed by {Mathf.Abs(card.speedCost)}. ";
        }
        if(card.HPCost != 0) {
            if(card.HPCost > 0) desc += $"Take {card.HPCost} damage. ";
            else desc+= $"Heal by {Mathf.Abs(card.HPCost)} HP. ";
        }
        foreach(Effect effect in card.Move.effects) {
            desc += $"{effect.generateDescription()}. ";
        }
        description.text = desc;
    }
}
