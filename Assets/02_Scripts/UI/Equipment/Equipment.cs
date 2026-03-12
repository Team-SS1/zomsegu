using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputEnum;
using PlayerEnum;
using EventEnum;
using ItemEnum;
using System.Collections.Specialized;

public class Equipment
{
    private readonly Dictionary<EquipSlotType, EquipmentSlot> slots = new();
    private readonly PlayerType playerType;

    public Equipment(PlayerType playerType)
    {
        this.playerType = playerType;

        slots[EquipSlotType.Head] = new EquipmentSlot(EquipSlotType.Head);
        slots[EquipSlotType.Body] = new EquipmentSlot(EquipSlotType.Body);
        slots[EquipSlotType.Leg] = new EquipmentSlot(EquipSlotType.Leg);
        slots[EquipSlotType.Shoes] = new EquipmentSlot(EquipSlotType.Shoes);
        slots[EquipSlotType.Bag] = new EquipmentSlot(EquipSlotType.Bag);
        slots[EquipSlotType.Weapon] = new EquipmentSlot(EquipSlotType.Weapon);
        slots[EquipSlotType.Accessory1] = new EquipmentSlot(EquipSlotType.Accessory1);
        slots[EquipSlotType.Accessory2] = new EquipmentSlot(EquipSlotType.Accessory2);
    }

    public EquipmentSlot GetSlot(EquipSlotType slotType)
    {
        slots.TryGetValue(slotType, out var slot);
        return slot;
    }

    public bool Equip(EquipSlotType slotType, ItemStack item)
    {
        if (item == null) return false;
        if(!slots.TryGetValue(slotType, out var slot)) return false;
        if (!slot.isEmpty) return false;

        slot.equippedItem = item;
        NotifyChanged();
        return true;
    }
    public bool UnEquip(EquipSlotType slotType, out ItemStack item)
    {
        item = null;

        if(!slots.TryGetValue(slotType, out var slot)) return false;
        if (slot.isEmpty) return false;

        item = slot.equippedItem;
        slot.Clear();

        NotifyChanged();
        return true;
    }
    public bool SwapEquip(EquipSlotType slotType, ItemStack newItem, out ItemStack oldItem)
    {
        oldItem = null;

        if(newItem == null) return false;
        if(!slots.TryGetValue(slotType, out var slot)) return false;

        oldItem = slot.equippedItem;
        slot.equippedItem = newItem;

        NotifyChanged();
        return true;
    }
    private void NotifyChanged()
    {
        EventManager.TriggerEvent(EventKey.EquipmentChanged, playerType);
    }
}
