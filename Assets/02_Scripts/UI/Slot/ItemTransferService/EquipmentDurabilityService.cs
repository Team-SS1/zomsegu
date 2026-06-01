using UnityEngine;
using PlayerEnum;
using ItemEnum;
using EventEnum;

public static class EquipmentDurabilityService
{
    public static DurabilityDamageResult DamageEquipSlot(PlayerType playerType, EquipSlotType equipSlotType, int damage = 1)
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);

        if (playerData == null || playerData.Equipment == null)
            return DurabilityDamageResult.None;

        DurabilityDamageResult result = playerData.Equipment.TryDamageDurability(equipSlotType, damage);

        if (result == DurabilityDamageResult.Broken)
        {
            HandleBrokenEquipment(playerType, equipSlotType);
        }

        return result;
    }
    public static DurabilityDamageResult DamageWeapon(PlayerType playerType, int damage = 1)
    {
        return DamageEquipSlot(playerType, EquipSlotType.Weapon, damage);
    }
    public static DurabilityDamageResult DamageArmor(PlayerType playerType, EquipSlotType armorSlotType, int damage = 1)
    {
        if (!IsArmorSlot(armorSlotType)) return DurabilityDamageResult.None;

        return DamageEquipSlot(playerType, armorSlotType, damage);
    }
    private static bool IsArmorSlot(EquipSlotType slotType)
    {
        return slotType == EquipSlotType.Head ||
               slotType == EquipSlotType.Body ||
               slotType == EquipSlotType.Leg;
    }
    private static void HandleBrokenEquipment(PlayerType playerType, EquipSlotType slotType)
    {
        if (slotType == EquipSlotType.Weapon) // 무기가 파괴된 경우 퀵슬롯 업데이트(장비는 퀵슬롯에 없으니)
        {
            EventManager.TriggerEvent(EventKey.QuickSlotChanged, playerType);
        }
    }
}
