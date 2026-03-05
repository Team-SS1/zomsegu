using ItemEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SlotRef //슬롯 주소값
{
    public SlotType slotType;
    public SlotKeyType keyType;

    public int index; // 인벤, 퀵슬롯, 드롭아이템용
    public EquipSlotType equipSlot; // 장비 슬롯용

    public static SlotRef Inv(int index) => new SlotRef
    {
        slotType = SlotType.Inventory,
        keyType = SlotKeyType.Index,
        index = index,
        equipSlot = default
    };
    public static SlotRef Quick(int index) => new SlotRef
    {
        slotType = SlotType.QuickSlot,
        keyType = SlotKeyType.Index,
        index = index,
        equipSlot = default
    };
    public static SlotRef Drop(int index) => new SlotRef
    {
        slotType = SlotType.DropItem,
        keyType = SlotKeyType.Index,
        index = index,
        equipSlot = default
    };
    public static SlotRef Equip(EquipSlotType equipSlot) => new SlotRef
    {
        slotType = SlotType.Equipment,
        keyType = SlotKeyType.EquipSlot,
        index = -1,
        equipSlot = equipSlot
    };
}
