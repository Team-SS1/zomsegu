using UnityEngine;
using ItemEnum;
using PlayerEnum;


public static class ItemUseService
{
    internal static bool TryUseOrEquipFromInventory(SlotRef from) // 인벤 아이템 사용 또는 장착하기 (퀵슬롯 기능에서 사용)
    {
        if (from.slotType != SlotType.Inventory) return false;

        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (playerData == null) return false;

        Inventory inventory = playerData.Inventory;
        Equipment equipment = playerData.Equipment;

        if (inventory == null) return false;

        InventorySlot inventorySlot = inventory.GetSlot(from.index);
        if (inventorySlot == null || inventorySlot.isEmpty) return false;

        CommonItemData common = ItemDB.GetCommon(inventorySlot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        if(itemType == ItemType.Consumable)
            return TryUseConsumableFromInventory(from);

        if(!TryGetTargetEquipSlot(itemType, equipment, out EquipSlotType targetSlot))
            return false;

        return TryTransferInventoryItemToEquip(from, targetSlot);
    }
    internal static bool TryUseConsumableFromInventory(SlotRef from) // 인벤 아이템 사용하기
    {
        if (from.slotType != SlotType.Inventory) return false;

        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (playerData == null || playerData.Inventory == null) return false;

        Inventory inventory = playerData.Inventory;
        InventorySlot inventorySlot = inventory.GetSlot(from.index);
        if (inventorySlot == null || inventorySlot.isEmpty) return false;
        if (!inventorySlot.IsStack) return false;

        CommonItemData common = ItemDB.GetCommon(inventorySlot.itemId);
        if (common == null) return false;

        Debug.Log("아이템 사용");
        return inventory.TryRemoveStack(from.index, 1);
    }
    //퀵슬롯 등록된 소비 아이템 사용하기
    internal static bool TryUseConsumableFromQuickSlot(PlayerType playerType, int quickSlotIndex)
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null || playerData.QuickSlot == null || playerData.Inventory == null) return false;

        QuickSlot quickSlot = playerData.QuickSlot;
        Inventory inventory = playerData.Inventory;

        QuickSlotSlot quickSlotSlot = quickSlot.GetSlot(quickSlotIndex);
        if (quickSlotSlot == null || quickSlotSlot.isEmpty) return false;
        if (!quickSlotSlot.IsStack) return false;

        CommonItemData common = ItemDB.GetCommon(quickSlotSlot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;
        if (itemType != ItemType.Consumable) return false;

        int itemId = quickSlotSlot.itemId;
        int invIndex = -1;

        for (int i = 0; i < inventory.Capacity; i++)
        {
            InventorySlot inventorySlot = inventory.GetSlot(i);
            if (inventorySlot == null || inventorySlot.isEmpty) continue;

            if (inventorySlot.IsStack && inventorySlot.itemId == itemId)
            {
                invIndex = i;
                break;
            }
        }
        if (invIndex < 0) return false;

        //아이템 사용함

        bool success = inventory.TryRemoveStack(invIndex, 1);
        if (!success) return false;

        return true;
    }
    private static bool TryTransferInventoryItemToEquip(SlotRef from, EquipSlotType targetSlot) // 인벤 아이템 장비 슬롯으로 옮기기
    {
        SlotRef to = SlotRef.Equip(from.playerType, targetSlot);

        DragPayload payload = new DragPayload(from);
        payload.to = to;

        return ItemTransferService.TryTransferBetweenSlots(payload);
    }
    private static bool TryGetTargetEquipSlot(ItemType itemType, Equipment equipment, out EquipSlotType targetSlot) // 아이템 타입에 맞는 장비 슬롯 찾기
    {
        targetSlot = EquipSlotType.Weapon; // 기본값

        switch (itemType)
        {
            case ItemType.Head:
                targetSlot = EquipSlotType.Head;
                return true;
            case ItemType.Body:
                targetSlot = EquipSlotType.Body;
                return true;
            case ItemType.Leg:
                targetSlot = EquipSlotType.Leg;
                return true;
            case ItemType.Shoes:
                targetSlot = EquipSlotType.Shoes;
                return true;
            case ItemType.Bag:
                targetSlot = EquipSlotType.Bag;
                return true;
            case ItemType.Weapon:
                targetSlot = EquipSlotType.Weapon;
                return true;
            case ItemType.Accessory:
                if(equipment == null) return false;

                EquipmentSlot acc1 = equipment.GetSlot(EquipSlotType.Accessory1);
                EquipmentSlot acc2 = equipment.GetSlot(EquipSlotType.Accessory2);

                if (acc1 == null || acc2 == null) return false;

                if(acc1.isEmpty)
                    targetSlot = EquipSlotType.Accessory1;
                else if (acc2.isEmpty)
                    targetSlot = EquipSlotType.Accessory2;
                else
                    targetSlot = EquipSlotType.Accessory1; // 둘 다 차있으면 액세서리1에 덮어쓰기

                return true;
            default:
                return false;
        }
    }
}
