using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public HandManager handManager;
    public GameObject highlight;

    public void OnPointerEnter(PointerEventData pointerEventData) {
        if(handManager.selectedCard != null) return;
        onHover();
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        if(handManager.selectedCard != null) return;
        onExit();
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
        handManager.deselectCard();
        if(handManager.selectedCard == this) {
            handManager.selectedCard = null;
            highlight.SetActive(false);
            onExit();
        }
        else {
            handManager.selectedCard = this;
            highlight.SetActive(true);
            BattleManager.instance.addSelectedCard(transform.GetComponent<Card>());
            BattleManager.instance.setGamePhase(phase.Select);
            onHover();
        } 
    }

    public void deselect() {
        highlight.SetActive(false);
        BattleManager.instance.setGamePhase(phase.Pick);
        BattleManager.instance.gpManager.unHighlightTargets();
        onExit();
    }

    public void onHover() {
        this.handManager.onHoverCard = transform.GetComponent<Card>();
        this.handManager.updateHand();
    }

    public void onExit() {
        this.handManager.onHoverCard = null;
        this.handManager.updateHand();
    }

}
