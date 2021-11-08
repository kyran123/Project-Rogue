using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

[System.Serializable]
public class PileManager : MonoBehaviour {
    
    public GameObject goDrawPile;
    public TMP_Text DrawPileCount;
    public GameObject goDiscardPile;
    public TMP_Text DiscardPilecount;

    public List<Card> drawPile = new List<Card>();
    public List<Card> discardPile = new List<Card>();

    public void discardCard(Card card) {
        this.discardPile.Add(card);
        card.transform.SetParent(this.goDiscardPile.transform);
        this.updateCardCounts();
    }

    public void moveCards() {
        if(this.drawPile.Count <= 0) {
            List<Card> randomizedCards = this.discardPile.OrderBy(card => Random.Range(0, this.discardPile.Count)).ToList();
            foreach(Card card in randomizedCards) {
                this.drawPile.Add(card);
                card.transform.SetParent(this.goDrawPile.transform);
            }
            this.discardPile.Clear();
        }
    }

    public Card draw() {
        if(this.drawPile.Count <= 0) this.moveCards();
        Card card = this.drawPile[0];
        drawPile.Remove(card);
        this.updateCardCounts();
        card.gameObject.SetActive(true);
        return card;
    }

    public void updateCardCounts() {
        DiscardPilecount.text = $"{this.discardPile.Count}";
        DrawPileCount.text = $"{this.drawPile.Count}";
    }
}
