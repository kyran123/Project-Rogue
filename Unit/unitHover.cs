using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class unitHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField]
    EffectDescriptionManager edManager;

    [SerializeField]
    Canvas canvas;

    public void OnPointerEnter(PointerEventData pointerEventData) {
        BattleManager bm = BattleManager.instance;
        Unit unit = this.GetComponentInParent<Unit>();
        Card card = bm.gpManager.selectedCard;
        if(card != null && unit.canBeSelected) {
            Unit selectedUnit = bm.gpManager.selectedUnit;
            if(selectedUnit != null) card.cardDisplay.updateCardDisplay(card, selectedUnit, unit);
            else card.cardDisplay.updateCardDisplay(card, unit);
        }
        if(unit.skills.Count > 0) {
            canvas.sortingOrder = 5;
            this.edManager.gameObject.SetActive(true);
        }
        if(unit.effects.Count > 0) {
            this.edManager.updateDescriptions(unit.effects);
            canvas.sortingOrder = 5;
            this.edManager.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        BattleManager bm = BattleManager.instance;
        Card card = bm.gpManager.selectedCard;
        if(this.edManager.gameObject.activeSelf) {
            this.edManager.gameObject.SetActive(false);
            if(card != null) card.cardDisplay.updateCardDisplay(card);
        } 
        canvas.sortingOrder = 1;
    }
}
