using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemEnum;
using System;

[Serializable]
public class EquipmentSlot
{
    public EquipSlotType slotType;
    public ItemStack equippedItem;

    public bool isEmpty => equippedItem == null;

    public EquipmentSlot(EquipSlotType slotType)
    {
        this.slotType = slotType;
        equippedItem = null;
    }
    public void Clear()
    {
        equippedItem = null;
    }
}
