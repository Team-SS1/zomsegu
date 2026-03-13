using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemEnum;
using System;

[Serializable]
public class EquipmentSlot
{
    public EquipSlotType slotType;
    public ItemStack equippedItem; // 실제 인벤에 없을 장착 아이템

    public int rangedWeaponItem; // 장착해도 인벤에 남아 있을 원거리 아이템 (itemId)

    public bool isEmpty => equippedItem == null && rangedWeaponItem == 0;
    public bool HasInstance => equippedItem != null;
    public bool HasRangedWeapon => rangedWeaponItem != 0;

    public EquipmentSlot(EquipSlotType slotType)
    {
        this.slotType = slotType;
        equippedItem = null;
        rangedWeaponItem = 0;
    }
    public int GetItemId()
    {
        if (equippedItem != null) return equippedItem.itemId;
        else if (rangedWeaponItem != 0) return rangedWeaponItem;
        return 0;
    }
    public void SetInstance(ItemStack instance)
    {
        equippedItem = instance;
        rangedWeaponItem = 0;
    }
    public void SetRangedItem(int itemId)
    {
        rangedWeaponItem = itemId;
        equippedItem = null;
    }
    public void Clear()
    {
        equippedItem = null;
        rangedWeaponItem = 0;
    }
}
