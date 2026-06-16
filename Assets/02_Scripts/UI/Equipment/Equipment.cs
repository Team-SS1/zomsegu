using System.Collections.Generic;
using PlayerEnum;
using EventEnum;
using ItemEnum;
using UnityEngine;

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

    public bool EquipInstance(EquipSlotType slotType, ItemStack item)
    {
        if(item == null)return false;
        if(!slots.TryGetValue(slotType, out var slot)) return false;
        if (!slot.isEmpty) return false;
        slot.SetInstance(item);
        NotifyChanged();
        return true;
    }
    public bool EquipRangedItem(EquipSlotType slotType, int itemId)
    {
        if (itemId == 0) return false;
        if (!slots.TryGetValue(slotType, out var slot)) return false;
        if(!slot.isEmpty) return false;

        slot.SetRangedItem(itemId);
        NotifyChanged();
        return true;
    }
    public bool UnEquip(EquipSlotType slotType, out ItemStack item, out int itemId)
    {
        item = null;
        itemId = 0;

        if(!slots.TryGetValue(slotType, out var slot)) return false;
        if (slot.isEmpty) return false;

        item = slot.equippedItem;
        itemId = slot.rangedWeaponItem;

        slot.Clear();
        NotifyChanged();
        return true;
    }
    //기존 아이템 교환 메서드 / 새로운 아이템 : 인스턴스 형
    public bool SwapInstance(EquipSlotType slotType, ItemStack newInstance, out ItemStack oldInstance, out int oldRangedItem)
    {
        oldInstance = null;
        oldRangedItem = 0;

        if(newInstance == null) return false;
        if(!slots.TryGetValue(slotType,out var slot)) return false;

        oldInstance = slot.equippedItem;
        oldRangedItem= slot.rangedWeaponItem;

        slot.SetInstance(newInstance);
        NotifyChanged();
        return true;
    }
    public bool SwapRangedItem(EquipSlotType slotType, int newRangedItem, out ItemStack oldInstance, out int oldRangedItem)
    {
        oldInstance = null;
        oldRangedItem = 0;

        if (newRangedItem == 0) return false;
        if(!slots.TryGetValue(slotType, out var slot) ) return false;

        oldInstance = slot.equippedItem;
        oldRangedItem = slot.rangedWeaponItem;

        slot.SetRangedItem(newRangedItem);
        NotifyChanged();
        return true;
    }
    public DurabilityDamageResult TryDamageDurability(EquipSlotType slotType, int damage)
    {
        if (damage <= 0) return DurabilityDamageResult.None;

        if (!slots.TryGetValue(slotType, out EquipmentSlot slot)) return DurabilityDamageResult.None;
        if(slot == null || slot.isEmpty) return DurabilityDamageResult.None;

        if(!slot.HasInstance || slot.equippedItem == null)
            return DurabilityDamageResult.None;

        ItemStack item = slot.equippedItem;

        if(!item.HasDurability || item.durability <= 0)
            return DurabilityDamageResult.None;

        item.durability = Mathf.Max(0, item.durability - damage);

        NotifyDurabilityChanged();

        if(item.durability <= 0)
        {
            slot.Clear();
            NotifyChanged();
            return DurabilityDamageResult.Broken;
        }

        return DurabilityDamageResult.Damaged;
    }
    private void NotifyDurabilityChanged()
    {
        EventManager.TriggerEvent(EventKey.EquipmentDurabilityChanged, playerType);
    }
    private void NotifyChanged()
    {
        EventManager.TriggerEvent(EventKey.EquipmentChanged, playerType);
    }
}
