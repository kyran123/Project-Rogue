using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

[System.Serializable]
public class PileManager : MonoBehaviour {
    
    public List<GameObject> tempCardPrefabs = new List<GameObject>();

    [Header("Draw Pile")]
    [SerializeField]
    public GameObject goDrawPile;
    public TMP_Text DrawPileCount;
    public List<Card> drawPile = new List<Card>();

    [Header("Discard Pile")]
    [SerializeField]
    public GameObject goDiscardPile;
    public TMP_Text DiscardPilecount;
    public List<Card> discardPile = new List<Card>();

    public void init() {
        for(int i = 0; i <= 20; i++) {
            GameObject obj = Instantiate(this.tempCardPrefabs[Random.Range(0, this.tempCardPrefabs.Count)]);
            obj.transform.SetParent(this.goDrawPile.transform);
            obj.SetActive(false);
            obj.transform.localScale = new Vector3(1, 1, 1);
            Card card = obj.GetComponent<Card>();
            card.cardBehavior.handManager = BattleManager.instance.handManager;
            card.cardDisplay.updateCardDisplay(card);
            drawPile.Add(card);
        }
    }

    /// <summary> Discards the card given in parameter </summary>
    public void discardCard(Card card) {
        this.discardPile.Add(card);
        card.transform.SetParent(this.goDiscardPile.transform);
        this.updateCardCounts();
    }

    /// <summary> Move the cards from the discard to the draw pile </summary>
    public void moveCards() {
        if(this.drawPile.Count <= 0) {
            //Randomly sort the discard pile and add it to the new list of randomized cards
            List<Card> randomizedCards = this.discardPile.OrderBy(card => Random.Range(0, this.discardPile.Count)).ToList();
            foreach(Card card in randomizedCards) {
                this.drawPile.Add(card);
                card.transform.SetParent(this.goDrawPile.transform);
            }
            this.discardPile.Clear();
        }
    }

    /// <summary> Draw a card </summary>
    public Card draw() {
        if(this.drawPile.Count <= 0) this.moveCards();
        Card card = this.drawPile[0];
        drawPile.Remove(card);
        this.updateCardCounts();
        card.gameObject.SetActive(true);
        return card;
    }

    /// <summary> Update the text indicating the amount of cards in each pile </summary>
    public void updateCardCounts() {
        DiscardPilecount.text = $"{this.discardPile.Count}";
        DrawPileCount.text = $"{this.drawPile.Count}";
    }
}
