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
        this.edManager.updateDescriptions(this.GetComponentInParent<Unit>().effects);
        canvas.sortingOrder = 5;
        this.edManager.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        this.edManager.gameObject.SetActive(false);
        canvas.sortingOrder = 1;
    }
}
