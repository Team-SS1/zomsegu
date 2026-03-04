using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemDB
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

    public static bool IsStackable(int itemId) => GetCommon(itemId)?.IsStackable ?? false;

    public static float GetWeight(int itemId) => GetCommon(itemId)?.ItemWeight ?? 0f;

    public static float GetVolume(int itemId) => GetCommon(itemId)?.ItemVolume ?? 0f;

    public static int GetDefaultDurability(int itemId) // 인스턴스형 아이템의 기본 내구도를 반환하는 메서드
    {
        if (WeaponStat.tableDic.TryGetValue(itemId, out var weaponStat)) return weaponStat.Durability;
        else if(ArmorStat.tableDic.TryGetValue(itemId, out var armorStat)) return armorStat.Durability;
        return 0;
    }
}
