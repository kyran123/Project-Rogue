using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card : MonoBehaviour {

    public CardBehavior cardBehavior;
    public CardDisplay cardDisplay;

    [SerializeField]
    public string title;

    [SerializeField]
    //If it's positive, we reduce it. If negative we add to it
    protected int APcost;
    public int APCost {
        get { return this.APcost; }
    }

    public int speedCost;

    public int HPCost;

    public bool consumable;


    [SerializeField]
    protected Move move;
    public Move Move {
        get { return this.move; }
    }
}
