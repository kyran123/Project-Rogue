using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour {
    
    public List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();

    public void addEquipment(Equipment equipment) {
        foreach(EquipmentSlot equipmentSlot in this.equipmentSlots) {
            Equipment e = equipmentSlot.getEquipment();
            if(e == null) equipmentSlot.addEquipment(equipment);
        }
    }

    public void removeEquipment(Equipment equipment) {
        foreach(EquipmentSlot equipmentSlot in this.equipmentSlots) {
            Equipment e = equipmentSlot.getEquipment();
            if(e != null) {
                if(equipmentSlot.getEquipment().type == equipment.type) {
                    equipmentSlot.equipment = null;
                }
            }
        }
    }

    public void use(List<Unit> targets, Unit selectedUnit) {
        foreach(EquipmentSlot equipmentSlot in this.equipmentSlots) {
            Equipment equipment = equipmentSlot.getEquipment();
            if(equipment != null) {
                if(equipment.isHighlighted) equipmentSlot.getEquipment().use(targets, selectedUnit);
            }
        }
    }

}
