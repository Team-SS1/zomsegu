using ItemEnum;
using PlayerEnum;

public static class EquipmentTransferService
{
    internal static bool TryInventoryToEquipment(SlotRef from, SlotRef to, bool autoBindToFirstQuickSlot) // 인벤 -> 장비
    {
        PlayerData data = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (data == null) return false;

        Inventory inventory = data.Inventory;
        Equipment equipment = data.Equipment;

        if (inventory == null || equipment == null) return false;

        InventorySlot fromSlot = inventory.GetSlot(from.index);
        if (fromSlot == null || fromSlot.isEmpty) return false;

        int itemId = fromSlot.itemId;
        if (!ItemDB.CanEquipToSlot(itemId, to.equipSlot)) return false;

        EquipmentSlot equipSlot = equipment.GetSlot(to.equipSlot);
        if (equipSlot == null) return false;

        if (fromSlot.IsStack && ItemDB.IsRangedWeapon(itemId)) //인벤 아이템 : 원거리 아이템
        {
            if (equipSlot.isEmpty)
            {
                bool success = equipment.EquipRangedItem(to.equipSlot, itemId);
                if (success)
                    HandleAutoBindOnEquip(autoBindToFirstQuickSlot, from.playerType, itemId, null);
                return success;
            }

            if (equipSlot.HasRangedWeapon)
            {
                bool success = equipment.SwapRangedItem(to.equipSlot, itemId, out _, out _);
                if (success)
                    HandleAutoBindOnEquip(autoBindToFirstQuickSlot, from.playerType, itemId, null);
                return success;
            }
            else if (equipSlot.HasInstance && equipSlot.equippedItem != null)
            {
                ItemStack oldInstance = equipSlot.equippedItem;

                if (!inventory.TryAddInstance(oldInstance.itemId, oldInstance)) return false;

                if (!equipment.UnEquip(to.equipSlot, out _, out _))
                {
                    inventory.TryRemoveInstance(oldInstance.guid, out _);
                    return false;
                }
                bool success = equipment.EquipRangedItem(to.equipSlot, itemId);
                if (success)
                    HandleAutoBindOnEquip(autoBindToFirstQuickSlot, from.playerType, itemId, null);
                else
                {
                    inventory.TryRemoveInstance(oldInstance.guid, out _);
                    equipment.EquipInstance(to.equipSlot, oldInstance);
                }
                    return success;
            }
            return false;
        }

        if (!fromSlot.IsInstance || fromSlot.instance == null) return false; // 여기서부턴 인스턴스형 아이템 장착 

        ItemStack instance = fromSlot.instance;
        string guid = instance.guid;

        if (equipSlot.isEmpty)
        {
            if (!inventory.TryRemoveInstance(guid, out ItemStack removed)) return false;
            if (!equipment.EquipInstance(to.equipSlot, removed))
            {
                inventory.TryAddInstance(removed.itemId, removed);
                return false;
            }
            HandleAutoBindOnEquip(autoBindToFirstQuickSlot, from.playerType, removed.itemId, removed);
            return true;
        }

        if (equipSlot.HasRangedWeapon) //기존 장착 아이템이 원거리면 인벤쪽 인스턴스형 아이템 제거
        {
            int oldRangedItemId = equipSlot.rangedWeaponItem;
            if (!inventory.TryRemoveInstance(guid, out ItemStack removed)) return false;
            if (!equipment.UnEquip(to.equipSlot, out _, out _))
            {
                inventory.TryAddInstance(removed.itemId, removed);
                return false;
            }
            if (!equipment.EquipInstance(to.equipSlot, removed))
            {
                inventory.TryAddInstance(removed.itemId, removed);
                equipment.EquipRangedItem(to.equipSlot, oldRangedItemId);
                return false;
            }
            HandleAutoBindOnEquip(autoBindToFirstQuickSlot, from.playerType, removed.itemId, removed);
            return true;
        }
        if (equipSlot.HasInstance && equipSlot.equippedItem != null) // 기존 장착 아이템이 인스턴스형이면 스왑
        {
            if (!inventory.TryRemoveInstance(guid, out ItemStack removedItem)) return false;
            if (!equipment.SwapInstance(to.equipSlot, removedItem, out ItemStack oldInstance, out _))
            {
                inventory.TryAddInstance(removedItem.itemId, removedItem);
                return false;
            }
            if (!inventory.TryPlaceInstanceAt(from.index, oldInstance))
            {
                equipment.SwapInstance(to.equipSlot, oldInstance, out _, out _);
                inventory.TryAddInstance(removedItem.itemId, removedItem);
                return false;
            }
            HandleAutoBindOnEquip(autoBindToFirstQuickSlot, from.playerType, removedItem.itemId, removedItem);
            return true;
        }
        return false;
    }
    private static void HandleAutoBindOnEquip(bool autoBindToFirstQuickSlot, PlayerType playerType, int itemId, ItemStack instance)
    {
        if(!autoBindToFirstQuickSlot) return;
        QuickSlotService.AutoBindEquippedItemToFirstQuickSlot(playerType, itemId, instance);
    }
    internal static bool TryEquipWeaponFromQuickSlot(PlayerType playerType, QuickSlotSlot slot) // 퀵슬롯에서 무기 장착 시도
    {
        if(slot == null || slot.isEmpty) return false;

        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null) return false;

        Inventory inventory = playerData.Inventory;
        Equipment equipment = playerData.Equipment;

        if (inventory == null || equipment == null) return false;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;
        if(itemType != ItemType.Weapon) return false;

        if (slot.IsStack)
            return TryEquipRangedWeaponFromQuickSlot(slot.itemId, inventory, equipment);

        if (slot.IsInstance)
        {
            int invIndex = inventory.FindIndexByGuid(slot.guid);
            if(invIndex < 0) return false;

            SlotRef from = SlotRef.Inv(playerType, invIndex);
            SlotRef to = SlotRef.Equip(playerType, EquipSlotType.Weapon);

            return TryInventoryToEquipment(from, to, false);
        }
        return false;
    }
    private static bool TryEquipRangedWeaponFromQuickSlot(int itemId, Inventory inventory, Equipment equipment)
    {
        EquipmentSlot weaponSlot = equipment.GetSlot(EquipSlotType.Weapon);
        if (weaponSlot == null) return false;

        if(weaponSlot.HasRangedWeapon && weaponSlot.rangedWeaponItem == itemId) // 이미 장착된 원거리 무기와 같은 아이템이면 true 반환
            return true; 

        if(weaponSlot.isEmpty)
            return equipment.EquipRangedItem(EquipSlotType.Weapon, itemId);

        if(weaponSlot.HasRangedWeapon)
            return equipment.SwapRangedItem(EquipSlotType.Weapon, itemId, out _, out _);

        if(weaponSlot.HasInstance && weaponSlot.equippedItem != null)
        {
            ItemStack oldInstance = weaponSlot.equippedItem;

            if (!inventory.TryAddInstance(oldInstance.itemId, oldInstance))
                return false;

            if (!equipment.UnEquip(EquipSlotType.Weapon, out _, out _))
            {
                inventory.TryRemoveInstance(oldInstance.guid, out _);
                return false;
            }

            if(!equipment.EquipRangedItem(EquipSlotType.Weapon, itemId))
            {
                inventory.TryRemoveInstance(oldInstance.guid, out _);
                equipment.EquipInstance(EquipSlotType.Weapon, oldInstance);
                return false;
            }

            return true;
        }

        return false;
    }
    internal static bool TryEquipmentToInventory(SlotRef from, SlotRef to) // 장비 -> 인벤
    {
        PlayerData data = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (data == null) return false;

        Inventory inventory = data.Inventory;
        Equipment equipment = data.Equipment;

        if (equipment == null || inventory == null) return false;

        InventorySlot inventorySlot = inventory.GetSlot(to.index);
        if (inventorySlot == null) return false;

        EquipmentSlot equipSlot = equipment.GetSlot(from.equipSlot);
        if (equipSlot == null) return false;

        if (equipSlot.HasRangedWeapon) //장착 아이템 : 원거리
        {
            int equippedRangedItemId = equipSlot.rangedWeaponItem;

            if (inventorySlot.isEmpty) //빈슬롯이면 장착 해제
            {
                return equipment.UnEquip(from.equipSlot, out _, out _);
            }
            else if (inventorySlot.IsInstance && inventorySlot.instance != null) //근접 무기면 스왑
            {
                if (!ItemDB.CanEquipToSlot(inventorySlot.itemId, from.equipSlot)) return false;

                ItemStack inventoryItem = inventorySlot.instance;
                string inventoryGuid = inventoryItem.guid;

                if (!inventory.TryRemoveInstance(inventoryGuid, out ItemStack removed)) return false;

                if (!equipment.UnEquip(from.equipSlot, out _, out _))
                {
                    inventory.TryAddInstance(removed.itemId, removed);
                    return false;
                }
                if (!equipment.EquipInstance(from.equipSlot, removed))
                {
                    inventory.TryAddInstance(removed.itemId, removed);
                    equipment.EquipRangedItem(from.equipSlot, equippedRangedItemId);
                    return false;
                }
                return true;
            }
            else if (inventorySlot.IsStack && ItemDB.IsRangedWeapon(inventorySlot.itemId)) // 원거리 아이템이라면 스왑
            {
                if (!ItemDB.CanEquipToSlot(inventorySlot.itemId, from.equipSlot)) return false;

                int targetRangedItemId = inventorySlot.itemId;

                if (targetRangedItemId == equippedRangedItemId) return true;

                if (!equipment.UnEquip(from.equipSlot, out _, out _)) return false;

                if (!equipment.EquipRangedItem(from.equipSlot, targetRangedItemId))
                {
                    equipment.EquipRangedItem(from.equipSlot, equippedRangedItemId);
                    return false;
                }
                return true;
            }
            return false;
        }
        if (!equipSlot.HasInstance || equipSlot.equippedItem == null) return false; //장착 아이템 : 인스턴스형 아이템

        if (inventorySlot.isEmpty) // 빈슬롯이면 장착 해제
        {
            if (!equipment.UnEquip(from.equipSlot, out ItemStack removed, out _)) return false;

            if (!inventory.TryPlaceInstanceAt(to.index, removed))
            {
                equipment.EquipInstance(from.equipSlot, removed);
                return false;
            }
            return true;
        }
        if (inventorySlot.IsStack && ItemDB.IsRangedWeapon(inventorySlot.itemId)) // 인벤 아이템 : 원거리 아이템 (스왑)
        {
            if (!ItemDB.CanEquipToSlot(inventorySlot.itemId, from.equipSlot)) return false;

            int emptyIndex = ItemTransferCommon.FindEmptySlot(inventory);
            if (emptyIndex < 0) return false;

            if (!equipment.UnEquip(from.equipSlot, out ItemStack removed, out _)) return false;

            if (!inventory.TryPlaceInstanceAt(emptyIndex, removed))
            {
                equipment.EquipInstance(from.equipSlot, removed);
                return false;
            }

            if (!equipment.EquipRangedItem(from.equipSlot, inventorySlot.itemId))
            {
                inventory.TryRemoveInstance(removed.guid, out _);
                equipment.EquipInstance(from.equipSlot, removed);
                return false;
            }

            return true;
        }

        if (!inventorySlot.IsInstance || inventorySlot.instance == null) return false; // 인벤 아이템 : 근접 아이템 (스왑)
        if (!ItemDB.CanEquipToSlot(inventorySlot.itemId, from.equipSlot)) return false;

        ItemStack invItem = inventorySlot.instance;
        string invGuid = invItem.guid;

        if (!inventory.TryRemoveInstance(invGuid, out ItemStack removedItem)) return false;
        if (!equipment.SwapInstance(from.equipSlot, invItem, out ItemStack oldInstance, out _))
        {
            inventory.TryAddInstance(removedItem.itemId, removedItem);
            return false;
        }


        if (!inventory.TryPlaceInstanceAt(to.index, oldInstance))
        {
            equipment.SwapInstance(from.equipSlot, oldInstance, out _, out _);
            inventory.TryAddInstance(removedItem.itemId, removedItem);
            return false;
        }
        return true;
    }
    internal static bool TryUnEquipToFirstEmptyInventory(SlotRef from) // 아이템 장착 해제 (인벤 제일 앞 빈칸으로)
    {
        if (from.slotType != SlotType.Equipment) return false;

        PlayerData data = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (data == null) return false;

        Inventory inventory = data.Inventory;
        Equipment equipment = data.Equipment;

        if (inventory == null || equipment == null) return false;

        EquipmentSlot equipmentSlot = equipment.GetSlot(from.equipSlot);
        if (equipmentSlot == null || equipmentSlot.isEmpty) return false;

        int emptyIndex = ItemTransferCommon.FindEmptySlot(inventory);
        if (emptyIndex < 0) return false;

        if (equipmentSlot.HasRangedWeapon) //원거리 아이템의 경우 그냥 해제
        {
            return equipment.UnEquip(from.equipSlot, out _, out _);
        }

        if (equipmentSlot.HasInstance && equipmentSlot.equippedItem != null)
        {
            if (!equipment.UnEquip(from.equipSlot, out ItemStack removed, out _)) return false;

            if (!inventory.TryPlaceInstanceAt(emptyIndex, removed))
            {
                equipment.EquipInstance(from.equipSlot, removed);
                return false;
            }
            return true;
        }
        return false;
    }
    internal static int GetEquippedItemId(SlotRef from) // 장착한 아이템 itemId 가져오기
    {
        if (from.slotType != SlotType.Equipment) return 0;

        PlayerData data = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (data == null || data.Equipment == null) return 0;

        EquipmentSlot equipmentSlot = data.Equipment.GetSlot(from.equipSlot);
        if (equipmentSlot == null || equipmentSlot.isEmpty) return 0;

        return equipmentSlot.GetItemId();
    }
}
