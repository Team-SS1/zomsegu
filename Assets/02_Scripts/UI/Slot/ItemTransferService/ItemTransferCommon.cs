using UnityEngine;
using ItemEnum;
public static class ItemTransferCommon
{
    private static readonly EquipSlotType[] EquipSlots =
    {
        EquipSlotType.Head,
        EquipSlotType.Body,
        EquipSlotType.Leg,
        EquipSlotType.Shoes,
        EquipSlotType.Bag,
        EquipSlotType.Weapon,
        EquipSlotType.Accessory1,
        EquipSlotType.Accessory2
    };
    internal static int FindEmptySlot(Inventory inventory) // 앞에서부터 인벤 빈슬롯 찾기 
    {
        if (inventory == null) return -1;

        for (int i = 0; i < inventory.Capacity; i++)
        {
            InventorySlot slot = inventory.GetSlot(i);
            if (slot != null && slot.isEmpty) return i;
        }
        return -1;
    }
    //플레이어 내부에서 아이템 찾기 (인벤, 장비)
    internal static ItemStack FindInstancePlayer(PlayerData playerData, string guid)
    {
        if (playerData == null || string.IsNullOrEmpty(guid)) return null;

        if (playerData.Inventory != null) //인벤에서 먼저 찾기
        {
            int invIndex = playerData.Inventory.FindIndexByGuid(guid);
            if (invIndex >= 0)
            {
                InventorySlot invSlot = playerData.Inventory.GetSlot(invIndex);
                if (invSlot != null && invSlot.IsInstance && invSlot.instance != null)
                    return invSlot.instance;
            }
        }

        if (playerData.Equipment != null)
        {
            for (int i = 0; i < EquipSlots.Length; i++)
            {
                EquipmentSlot equipSlot = playerData.Equipment.GetSlot(EquipSlots[i]);
                if (equipSlot != null && equipSlot.HasInstance && equipSlot.equippedItem != null)
                {
                    if (equipSlot.equippedItem.guid == guid)
                        return equipSlot.equippedItem;
                }
            }
        }
        return null;
    }
}
