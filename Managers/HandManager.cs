using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour {

    public Dictionary<int, List<float>> cardPos = new Dictionary<int, List<float>> {
        { 0, new List<float> { 0f } },
        { 1, new List<float> { -8.6f } },
        { 2, new List<float> { -8.6f, -8.6f } },
        { 3, new List<float> { -13.92f, -8.6f, -13.92f } },
        { 4, new List<float> { -13.92f, -8.6f, -8.6f, -13.92f } },
        { 5, new List<float> { -13.92f, -8.6f, -6.9f, -8.6f, -13.92f } },
        { 6, new List<float> { -22f, -13.92f, -8.6f, -8.6f, -13.92f, -22f } },
        { 7, new List<float> { -22f, -13.92f, -8.6f, -6.9f, -8.6f, -13.92f, -22f } },
        { 8, new List<float> { -22f, -13.92f, -8.6f, -6.9f, -6.9f, -8.6f, -13.92f, -22f } }
    };

    public Dictionary<int, float> totalTwist = new Dictionary<int, float> {
        { 0, 0f },
        { 1, 0f },
        { 2, 3f },
        { 3, 5f },
        { 4, 8f },
        { 5, 12f },
        { 6, 20f },
        { 7, 25f },
        { 8, 30f }
    };

    public List<Card> cards;
    public Card onHoverCard;
    public int maxCardsInHand = 8;

    public CardBehavior selectedCard = null;

    public float baseOverlap = 40f;
    public float baseExpand = 40f;
    public float cardHeight = 100f;
    public float cardWidth = 80f;

    void Start() {
        this.updateHand();
        foreach(Card card in this.cards) {
            card.cardDisplay.updateCardDisplay(card);
        }
    }

    public void addCard(Card card) {
        cards.Add(card);
        card.transform.SetParent(this.transform);
        this.updateHand();
    }

    /// <summary> Discards entire hand </summary>
    public void discardCards() {
        for(int i = this.cards.Count; i > 0; i--) {
            this.discardCard(this.cards[i-1]);
        }
    }

    /// <summary> Discards card given in parameter </summary>
    public void discardCard(Card card) {
        this.removeCard(card);
        BattleManager.instance.pileManager.discardCard(card);
        this.selectedCard = null;
        card.GetComponent<CardBehavior>().highlight.SetActive(false);
        card.gameObject.SetActive(false);
    }

    public void drawCard(int count) {
        for(int i = 0; i < count; i++) {
            if(this.cards.Count < maxCardsInHand) {
                Card card = BattleManager.instance.pileManager.draw();
                this.addCard(card);
            } else {
                return;
            }
        }
    }

    public void removeCard(Card card) {
        cards.Remove(card);
        this.onHoverCard = null;
        this.updateHand();
    }

    public void updateHand() {
        this.calculateCardRotations();
        this.calculateCardPositions();
    }

    public void calculateCardRotations() {
        int count = this.cards.Count;
        for(int i = 0; i < count; i++) {
            float anglePerCard = this.totalTwist[count] / count; //Get the average angle per card
            float startAngle = -1f * (this.totalTwist[count] / 2f); //Get the starting angle
            float rotationOfCard = -(startAngle + (i * anglePerCard)); //Calculate the exact angle this specific card is at
            this.cards[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationOfCard);
        }
    }

    public void calculateCardPositions() {
        //Get the card index of the card that is being hovered on
        int cardIndex = 9999;
        if(this.onHoverCard != null) cardIndex = this.cards.IndexOf(this.onHoverCard);
        float hoverWidth = 0f;
        if(cardIndex < 999) hoverWidth = this.baseOverlap;

        //Handwidth increases a total of baseOverlap * 2 when card is hovered
        float handWidth = ((this.cards.Count - 1) * (this.cardWidth - this.baseOverlap)) + hoverWidth; //Gets the total width of the hand

        if(cardIndex == 0) {
            handWidth -= this.baseOverlap;
        }
        
        float cardSpacing = this.cardWidth - this.baseOverlap;
        float startPosition = -(handWidth / 2f); //Gets the starting position

        for(int i = 0; i < this.cards.Count; i++) {
            float x = 0f;
            float y = 0f;
            float z = 0f;

            if(i == cardIndex && i > 0) x += this.baseOverlap; //Add spacing to the left of the hover card
            if(cardIndex == 0) {
                if(i > cardIndex && cardIndex < this.cards.Count) x += this.baseOverlap; //Add spacing to the right of the hover card
            } else {
                if(i > cardIndex && cardIndex < this.cards.Count) x += this.cardWidth; //Add spacing to the right of the hover card
            }
            
            if(i == cardIndex) y = (5f + y + -this.cardPos[this.cards.Count][i]);

            this.cards[i].transform.localPosition = new Vector3(
                (float)((x + startPosition) + (i * cardSpacing)), 
                (float)(y + this.cardPos[this.cards.Count][i]), 
                z
            );
        }
    }

    public void deselectCard() {
        if(this.selectedCard != null) {
            this.selectedCard.deselect();
        }
    }









    
}
