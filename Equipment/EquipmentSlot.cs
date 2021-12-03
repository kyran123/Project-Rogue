using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentSlot : MonoBehaviour {
    
    public GameObject equipment;

    public void addEquipment(Equipment equipment) {
        equipment.transform.SetParent(this.transform);
        this.equipment = equipment.gameObject;
        this.equipment.transform.localPosition = new Vector3(0, 0, 0);
        this.equipment.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public Equipment getEquipment() {
        if(this.equipment != null) return this.equipment.GetComponent<Equipment>();
        else return null;
    }

}
