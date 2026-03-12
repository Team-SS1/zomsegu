using ItemEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemDB // 스택이 가능한 아이템 : 스택형 , 스택이 불가능한 아이템 : 인스턴스형
{
    public static bool TryGetCommon(int itemId, out CommonItemData data) => CommonItemData.tableDic.TryGetValue(itemId, out data);

    public static CommonItemData GetCommon(int itemId)
    {
        if (TryGetCommon(itemId, out var data)) return data;
#if UNITY_EDITOR
        Debug.LogError($"ItemId CommonItemData is not found. ItemID={itemId}");
#endif
        return null;
    }
    public static bool UseInstance(int itemId)
    {
        CommonItemData common = GetCommon(itemId);
        if(common == null) return false;
        return !common.IsStackable;
    }
    public static bool HasDurability(int itemId)
    {
        CommonItemData common = GetCommon(itemId);
        if (common == null) return false;

        ItemType type = (ItemType)common.ItemType;

        switch (type)
        {
            case ItemType.Weapon:
            case ItemType.Head:
            case ItemType.Body:
            case ItemType.Leg:
                return true;
            case ItemType.Shoes:
            case ItemType.Bag:
            case ItemType.Accessory:
            case ItemType.Consumable:
            case ItemType.Misc:
            default:
                return false;
        }
    }
    public static bool IsStackable(int itemId) => GetCommon(itemId)?.IsStackable ?? false;

    public static float GetWeight(int itemId) => GetCommon(itemId)?.ItemWeight ?? 0f;

    public static float GetVolume(int itemId) => GetCommon(itemId)?.ItemVolume ?? 0f;

    public static int GetDefaultDurability(int itemId) // 인스턴스형 아이템의 기본 내구도를 반환하는 메서드
    {
        if(!HasDurability(itemId)) return 0; // 내구도가 없는 아이템은 0 반환
        if (WeaponStat.tableDic.TryGetValue(itemId, out var weaponStat)) return weaponStat.Durability;
        else if(ArmorStat.tableDic.TryGetValue(itemId, out var armorStat)) return armorStat.Durability;
        return 0;
    }
    public static bool CanEquipToSlot(int itemId, EquipSlotType slotType)
    {
        CommonItemData common = GetCommon(itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        switch (slotType)
        {
            case EquipSlotType.Head:
                return itemType == ItemType.Head;
            case EquipSlotType.Body:
                return itemType == ItemType.Body;
            case EquipSlotType.Leg:
                return itemType == ItemType.Leg;
            case EquipSlotType.Shoes:
                return itemType == ItemType.Shoes;
            case EquipSlotType.Bag:
                return itemType == ItemType.Bag;
            case EquipSlotType.Weapon:
                return itemType == ItemType.Weapon;
            case EquipSlotType.Accessory1:
            case EquipSlotType.Accessory2:
                return itemType == ItemType.Accessory;
            default:
                return false;
        }
    }
}
