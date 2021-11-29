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
        Unit unit = this.GetComponentInParent<Unit>();
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
        if(this.edManager.gameObject.activeSelf) this.edManager.gameObject.SetActive(false);
        canvas.sortingOrder = 1;
    }
}
