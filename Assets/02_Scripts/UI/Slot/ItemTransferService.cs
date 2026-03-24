using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemEnum;
using PlayerEnum;
using Unity.VisualScripting;
using TMPro.EditorUtilities;
public static class ItemTransferService
{
    public static bool TryTransferBetweenSlots(DragPayload payload) // 슬롯 간 이동
    {
        if (payload == null || !payload.hasTo) return false;

        SlotRef from = payload.from;
        SlotRef to = payload.to;

        if (from.playerType != to.playerType) return false; // 플레이어 간 이동은 원래도 안되지만 일단 여기서도 막아둠

        if (from.slotType == SlotType.Inventory && to.slotType == SlotType.Inventory) // 인벤 -> 인벤 이동
            return TryInventoryToInventory(from, to);

        if (from.slotType == SlotType.Inventory && to.slotType == SlotType.Equipment) // 인벤 -> 장비 이동
            return TryInventoryToEquipment(from, to, true);

        if (from.slotType == SlotType.Equipment && to.slotType == SlotType.Inventory) // 장비 -> 인벤 이동
            return TryEquipmentToInventory(from, to);

        if (from.slotType == SlotType.Inventory && to.slotType == SlotType.QuickSlot) // 인벤 -> 퀵슬롯 이동
            return TryInventoryToQuickSlot(from, to);

        if (from.slotType == SlotType.QuickSlot && to.slotType == SlotType.QuickSlot) // 퀵슬롯 -> 퀵슬롯 이동
            return TryQuickSlotToQuickSlot(from, to);

        if (from.slotType == SlotType.DropItem && to.slotType == SlotType.Inventory) // 드롭아이템 -> 인벤 이동
            return TryDropItemToInventory(from, to);

        return false;
    }
    public static bool TryGiveToOtherPlayer(SlotRef from, PlayerType targetPlayer) // 슬롯에서 다른 플레이어에게 주기
    {
        return TryGiveToOtherPlayer(from, targetPlayer, -1);
    }
    public static bool TryGiveToOtherPlayer(SlotRef from, PlayerType targetPlayer, int amount)
    {
        if (from.slotType != SlotType.Inventory) return false;
        if (from.playerType == targetPlayer) return false;

        PlayerData fromData = PlayerManager.Instance.GetPlayerData(from.playerType);
        PlayerData toData = PlayerManager.Instance.GetPlayerData(targetPlayer);

        if (fromData == null || toData == null) return false;

        Inventory fromInv = fromData.Inventory;
        Inventory toInv = toData.Inventory;

        if (fromInv == null || toInv == null) return false;

        InventorySlot fromSlot = fromInv.GetSlot(from.index);
        if (fromSlot == null || fromSlot.isEmpty) return false;

        int emptySlotIndex = FindEmptySlot(toInv);
        if (emptySlotIndex < 0) return false;

        bool success = MoveBetweenInventories(fromInv, from.index, toInv, emptySlotIndex, amount);
        if (success)
        {
            ValidateQuickSlots(from.playerType);
            ValidateQuickSlots(targetPlayer);
        }
        return success;
    }
    public static bool TryDropOutside(SlotRef from) // 슬롯에서 외부로 드롭
    {
        return TryDropOutside(from, -1);
    }
    public static bool TryDropOutside(SlotRef from, int amount)
    {
        if (from.slotType == SlotType.Inventory)
            return TryInventorytoWorldDrop(from, amount);

        if (from.slotType == SlotType.QuickSlot)
            return TryClearQuickSlot(from);

        return false;
    }

    //실제 슬롯 이동 코드
    private static bool TryInventoryToInventory(SlotRef from, SlotRef to) // 인벤에서 인벤으로 이동
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(from.playerType);
        if (data == null || data.Inventory == null) return false;

        return data.Inventory.Move(from.index, to.index);
    }
    private static bool TryInventoryToEquipment(SlotRef from, SlotRef to, bool autoBindToFirstQuickSlot) // 인벤에서 장비로 이동
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(from.playerType);
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
                bool  success = equipment.EquipRangedItem(to.equipSlot, itemId);
                if (success)
                    AutoBindEquippedItemToFirstQuickSlot(from.playerType, itemId, null);
                return success;
            }

            if (equipSlot.HasRangedWeapon)
            {
                bool success = equipment.SwapRangedItem(to.equipSlot, itemId, out _, out _);
                if (success)
                    AutoBindEquippedItemToFirstQuickSlot(from.playerType, itemId, null);
                return success;
            } else if (equipSlot.HasInstance && equipSlot.equippedItem != null)
            {
                ItemStack oldInstance = equipSlot.equippedItem;

                if (!inventory.TryAddInstance(oldInstance.itemId, oldInstance)) return false;

                if (!equipment.UnEquip(to.equipSlot, out _, out _))
                {
                    inventory.TryRemoveInstance(oldInstance.guid, out _);
                    return false;
                }
                bool success = equipment.SwapRangedItem(to.equipSlot, itemId, out _, out _);
                if (success)
                    AutoBindEquippedItemToFirstQuickSlot(from.playerType, itemId, null);
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
            if(autoBindToFirstQuickSlot)
                AutoBindEquippedItemToFirstQuickSlot(from.playerType, removed.itemId, removed);
            return true;
        }

        if (equipSlot.HasRangedWeapon) //기존 장착 아이템이 원거리면 인벤쪽 인스턴스형 아이템 제거
        {
            if (!inventory.TryRemoveInstance(guid, out ItemStack removed)) return false;
            if (!equipment.UnEquip(to.equipSlot, out _, out _))
            {
                inventory.TryAddInstance(removed.itemId, removed);
                return false;
            }
            if (!equipment.EquipInstance(to.equipSlot, removed))
            {
                inventory.TryAddInstance(removed.itemId, removed);
                return false;
            }
            if(autoBindToFirstQuickSlot)
                AutoBindEquippedItemToFirstQuickSlot(from.playerType, removed.itemId, removed);
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
            if(autoBindToFirstQuickSlot)
                AutoBindEquippedItemToFirstQuickSlot(from.playerType, removedItem.itemId, removedItem);
            return true;
        }
        return false;
    }
    private static bool TryEquipmentToInventory(SlotRef from, SlotRef to) // 장비에서 인벤으로 이동
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(from.playerType);
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

            int emptyIndex = FindEmptySlot(inventory);
            if (emptyIndex < 0) return false;

            if (!equipment.UnEquip(from.equipSlot, out ItemStack removed, out _)) return false;

            if (!inventory.TryPlaceInstanceAt(emptyIndex, removed)) {
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
    public static bool TryUnEquipToFirstEmptyInventory(SlotRef from)
    {
        if (from.slotType != SlotType.Equipment) return false;

        PlayerData data = PlayerManager.Instance.GetPlayerData(from.playerType);
        if (data == null) return false;

        Inventory inventory = data.Inventory;
        Equipment equipment = data.Equipment;

        if (inventory == null || equipment == null) return false;

        EquipmentSlot equipmentSlot = equipment.GetSlot(from.equipSlot);
        if (equipmentSlot == null || equipmentSlot.isEmpty) return false;

        int emptyIndex = FindEmptySlot(inventory);
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
    private static bool CanRegisterQuickSlot(InventorySlot slot)
    {
        if (slot == null || slot.isEmpty) return false;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        if (itemType == ItemType.Consumable || itemType == ItemType.Weapon) return true;

        return false;
    }
    private static bool TryInventoryToQuickSlot(SlotRef from, SlotRef to) // 인벤에서 퀵슬롯으로 이동
    {
        PlayerData playerData = PlayerManager.Instance.GetPlayerData(from.playerType);
        if (playerData == null) return false;

        Inventory inventory = playerData.Inventory;
        QuickSlot quickSlot = playerData.QuickSlot;

        if (quickSlot == null || inventory == null) return false;

        InventorySlot inventorySlot = inventory.GetSlot(from.index);
        if (inventorySlot == null || inventorySlot.isEmpty) return false;

        if (!CanRegisterQuickSlot(inventorySlot)) return false;

        int existingIndex = -1;

        if (inventorySlot.IsStack) //퀵슬롯에 같은 아이템 있나 확인
        {
            existingIndex = quickSlot.FindSlotByItemId(inventorySlot.itemId);
        }else if(inventorySlot.IsInstance && inventorySlot.instance != null)
        {
            existingIndex = quickSlot.FindSlotByGuid(inventorySlot.instance.guid);
        }

        if(existingIndex >= 0 && existingIndex != to.index)
        {
            quickSlot.ClearSlot(existingIndex);
        }

        bool success = false;

        if (inventorySlot.IsStack)
        {
            success = quickSlot.BindStack(to.index, inventorySlot.itemId);
        } else if (inventorySlot.IsInstance && inventorySlot.instance != null)
        {
            success = quickSlot.BindInstance(to.index, inventorySlot.instance);
        }

        if (!success) return false;

        TryAutoEquipIfSelectedQuickSlot(from.playerType, to.index);
        return true;
    }
    private static bool TryQuickSlotToQuickSlot(SlotRef from, SlotRef to) // 퀵슬롯에서 퀵슬롯으로 이동
    {
        PlayerData playerData = PlayerManager.Instance.GetPlayerData(from.playerType);
        if (playerData == null || playerData.QuickSlot == null) return false;

        return playerData.QuickSlot.Swap(from.index, to.index);
    }
    private static bool TryDropItemToInventory(SlotRef from, SlotRef to) // 드롭아이템에서 인벤으로 이동
    {
        //아직 구현 안함
        Debug.Log("드롭 -> 인벤");
        return false;
    }

    //외부로 드래그 관련
    private static bool TryInventorytoWorldDrop(SlotRef from, int amount) // 인벤에서 월드 드롭
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(from.playerType);
        if (data == null || data.Inventory == null) return false;

        Inventory inventory = data.Inventory;
        InventorySlot slot = inventory.GetSlot(from.index);

        if(slot == null || slot.isEmpty) return false;

        if (slot.IsStack)
        {
            int removeAmount = amount <= 0 ? slot.amount : amount;
            removeAmount = Mathf.Clamp(removeAmount, 1, amount);

            int itemId = slot.itemId;

            bool removed = inventory.TryRemoveStack(from.index, removeAmount);
            if (!removed) return false;

            Debug.Log($"스택 아이템 {itemId} 버리기 {removeAmount}개"); //아직 구현 다 안끝남

            return true;
        }
        
        if(slot.IsInstance && slot.instance != null)
        {
            string guid = slot.instance.guid;
            int itemId = slot.instance.itemId;

            bool removed = inventory.TryRemoveInstance(guid, out ItemStack removedItem);
            if (!removed) return false;

            ValidateQuickSlots(from.playerType);

            Debug.Log($"인스턴스 아이템 {itemId} 버리기");
            return true;
        }
        
        return false;
    }
    public static bool TryClearQuickSlot(SlotRef from) // 퀵슬롯에서 외부로 드롭 (퀵슬롯 제외)
    {
        if(from.slotType != SlotType.QuickSlot) return false;

        PlayerData playerData = PlayerManager.Instance.GetPlayerData(from.playerType);
        if(playerData == null || playerData.QuickSlot == null) return false;

        return playerData.QuickSlot.ClearSlot(from.index);
    }
    private static void TryAutoEquipIfSelectedQuickSlot(PlayerType playerType, int quickSlotIndex)
    {
        PlayerData playerData = PlayerManager.Instance.GetPlayerData(playerType);
        if (playerData == null) return;

        QuickSlot quickSlot = playerData.QuickSlot;
        Inventory inventory = playerData.Inventory; 
        Equipment equipment = playerData.Equipment;

        if(equipment == null || inventory == null || quickSlot == null) return;
        if (quickSlot.SelectedIndex != quickSlotIndex) return;

        QuickSlotSlot slot = quickSlot.GetSlot(quickSlotIndex);
        if(slot == null || slot.isEmpty) return;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if(common == null) return;

        ItemType itemType = (ItemType)common.ItemType;
        if (itemType != ItemType.Weapon) return;

        if (slot.IsStack) //퀵슬롯에 슽택형 아이템 등록시
        {
            EquipmentSlot weaponSlot = equipment.GetSlot(EquipSlotType.Weapon);
            if (weaponSlot == null) return;

            if (weaponSlot.HasRangedWeapon && weaponSlot.rangedWeaponItem == slot.itemId) return; // 기존 장착 아이템이랑 똑같으면 return;

            if (weaponSlot.isEmpty)
            {
                equipment.EquipRangedItem(EquipSlotType.Weapon, slot.itemId);
                return;
            }

            if (weaponSlot.HasRangedWeapon)
            {
                equipment.SwapRangedItem(EquipSlotType.Weapon, slot.itemId, out _, out _);
                return;
            }

            if(weaponSlot.HasInstance && weaponSlot.equippedItem != null)
            {
                ItemStack oldInstance = weaponSlot.equippedItem;

                if (!inventory.TryAddInstance(oldInstance.itemId, oldInstance)) return;

                if(!equipment.UnEquip(EquipSlotType.Weapon, out _, out _)){
                    inventory.TryRemoveInstance(oldInstance.guid, out _);
                    return;
                }

                equipment.EquipRangedItem(EquipSlotType.Weapon, slot.itemId);
            }

            return;
        }
        if (slot.IsInstance) // 퀵슬롯에 인스턴스형 아이템 등록시
        {
            int invIndex = inventory.FindIndexByGuid(slot.guid);
            if(invIndex < 0) return;

            SlotRef from = SlotRef.Inv(playerType, invIndex);
            SlotRef to = SlotRef.Equip(playerType, EquipSlotType.Weapon);

            TryInventoryToEquipment(from, to, false);
        }
    }
    //기타
    private static int FindEmptySlot(Inventory inventory) // 빈 슬롯 찾기
    {
        if(inventory == null) return -1;

        for (int i = 0; i < inventory.Capacity; i++)
        {
            InventorySlot slot = inventory.GetSlot(i);
            if(slot != null && slot.isEmpty) return i;
        }
        return -1;
    }
    private static void AutoBindEquippedItemToFirstQuickSlot(PlayerType playerType, int itemId, ItemStack instance)
    {
        if (itemId == 0) return;

        CommonItemData common = ItemDB.GetCommon(itemId);
        if (common == null) return;

        ItemType itemType = (ItemType)common.ItemType;

        if (itemType != ItemType.Weapon) return;

        PlayerData playerData = PlayerManager.Instance.GetPlayerData(playerType);
        if (playerData == null) return;

        QuickSlot quickSlot = playerData.QuickSlot;
        if(quickSlot == null) return;

        int existingIndex = -1;

        if (instance != null)
            existingIndex = quickSlot.FindSlotByGuid(instance.guid);
        else
            existingIndex = quickSlot.FindSlotByItemId(itemId);

        if (existingIndex >= 0 && existingIndex != 0) //다른 퀵슬롯 칸에 이미 있는 경우 그 슬롯 등록 해제
            quickSlot.ClearSlot(existingIndex);


        if (instance != null)
            quickSlot.BindInstance(0, instance);
        else
            quickSlot.BindStack(0, itemId);

        quickSlot.SetSelectedIndex(0);
    }
    //플레이어 -> 플레이어 아이템 이동
    private static bool MoveBetweenInventories(Inventory fromInv, int fromIndex, Inventory toInv, int toIndex, int amount) 
    {
        if(fromInv == null || toInv == null) return false;

        InventorySlot fromSlot = fromInv.GetSlot(fromIndex);
        InventorySlot toSlot = toInv.GetSlot(toIndex);

        if(fromSlot == null || fromSlot.isEmpty) return false;
        if(toSlot == null || !toSlot.isEmpty) return false;

        if (fromSlot.IsStack)  
        {
            int itemId = fromSlot.itemId;
            int moveAmount = amount <= 0 ? fromSlot.amount : amount;
            moveAmount = Mathf.Clamp(moveAmount, 1, fromSlot.amount);

            if(!toInv.TryAddStack(itemId, moveAmount)) return false;

            return fromInv.TryRemoveStack(fromIndex, moveAmount);
        }
        if (fromSlot.IsInstance)
        {
            ItemStack instance = fromSlot.instance;
            string guid = instance.guid;

            if(!toInv.TryAddInstance(instance.itemId, instance)) return false;

            bool removed = fromInv.TryRemoveInstance(guid, out _);
            if(!removed) return false;
            return true;
        }

        return false;
    }
    private static ItemStack FindInstancePlayer(PlayerData playerData, string guid)
    {
        if(playerData == null || string.IsNullOrEmpty(guid)) return null;

        if(playerData.Inventory != null) //인벤에서 먼저 찾기
        {
            int invIndex = playerData.Inventory.FindIndexByGuid(guid);
            if(invIndex >= 0)
            {
                InventorySlot invSlot = playerData.Inventory.GetSlot(invIndex);
                if(invSlot != null && invSlot.IsInstance && invSlot.instance != null)
                    return invSlot.instance;
            }
        }

        if(playerData.Equipment != null)
        {
            EquipSlotType[] equipSlots =
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

            for(int i = 0; i< equipSlots.Length; i++)
            {
                EquipmentSlot equipSlot = playerData.Equipment.GetSlot(equipSlots[i]);
                if(equipSlot != null && equipSlot.HasInstance && equipSlot.equippedItem != null)
                {
                    if (equipSlot.equippedItem.guid == guid)
                        return equipSlot.equippedItem;
                }
            }
        }
        return null;
    }
    public static void ValidateQuickSlots(PlayerType playerType)
    {
        PlayerData playerData = PlayerManager.Instance.GetPlayerData(playerType);
        if (playerData == null) return;

        QuickSlot quickSlot = playerData.QuickSlot;
        if(quickSlot == null) return;

        for(int i = 0; i < quickSlot.Capacity; i++)
        {
            QuickSlotSlot quickSlotSlot = quickSlot.GetSlot(i);
            if(quickSlotSlot == null || quickSlotSlot.isEmpty) continue;

            if (quickSlotSlot.IsInstance)
            {
                ItemStack instance = FindInstancePlayer(playerData, quickSlotSlot.guid);

                if(instance == null)
                {
                    quickSlot.ClearSlot(i);
                    continue;
                }
                if(instance.HasDurability && instance.durability <= 0)
                {
                    quickSlot.ClearSlot(i);
                    continue; 
                }
            }
        }
    }
    public static bool TrySelectQuickSlot(PlayerType playerType, int quickSlotIndex)
    {
        PlayerData playerData = PlayerManager.Instance.GetPlayerData(playerType);
        if (playerData == null || playerData.QuickSlot == null) return false;

        QuickSlot quickSlot = playerData.QuickSlot;
        if (quickSlotIndex < 0 || quickSlotIndex >= quickSlot.Capacity) return false;

        QuickSlotSlot slot = quickSlot.GetSlot(quickSlotIndex);
        if(slot == null) return false;

        quickSlot.SetSelectedIndex(quickSlotIndex);

        if (slot.isEmpty) return true;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        if(itemType == ItemType.Consumable)
            return TryUseConsumableFromQuickSlot(playerType, quickSlotIndex);
        else if(itemType == ItemType.Weapon)
        {
            TryAutoEquipIfSelectedQuickSlot(playerType, quickSlotIndex);
            return true;
        }       
        return true;
    }
    public static bool TryUseOrEquipFromInventory(SlotRef from)
    {
        if(from.slotType != SlotType.Inventory) return false;

        PlayerData playerData = PlayerManager.Instance.GetPlayerData(from.playerType);
        if(playerData == null) return false;

        Inventory inventory = playerData.Inventory;
        Equipment equipment = playerData.Equipment;

        if(inventory == null) return false;

        InventorySlot inventorySlot = inventory.GetSlot(from.index);
        if(inventorySlot == null || inventorySlot.isEmpty) return false;

        CommonItemData common = ItemDB.GetCommon(inventorySlot.itemId);
        if(common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        switch (itemType)
        {
            case ItemType.Weapon:
                {
                    SlotRef to = SlotRef.Equip(from.playerType, EquipSlotType.Weapon);
                    DragPayload payload = new DragPayload(from);
                    payload.SetTo(to);
                    return TryTransferBetweenSlots(payload);
                }
                
            case ItemType.Head:
                {
                    SlotRef to = SlotRef.Equip(from.playerType, EquipSlotType.Head);
                    DragPayload payload = new DragPayload(from);
                    payload.SetTo(to);
                    return TryTransferBetweenSlots(payload);
                }
            case ItemType.Body:
                {
                    SlotRef to = SlotRef.Equip(from.playerType, EquipSlotType.Body);
                    DragPayload payload = new DragPayload(from);
                    payload.SetTo(to);
                    return TryTransferBetweenSlots(payload);
                }
            case ItemType.Leg:
                {
                    SlotRef to = SlotRef.Equip(from.playerType, EquipSlotType.Leg);
                    DragPayload payload = new DragPayload(from);
                    payload.SetTo(to);
                    return TryTransferBetweenSlots(payload);
                }
            case ItemType.Shoes:
                {
                    SlotRef to = SlotRef.Equip(from.playerType, EquipSlotType.Shoes);
                    DragPayload payload = new DragPayload(from);
                    payload.SetTo(to);
                    return TryTransferBetweenSlots(payload);
                }
            case ItemType.Bag:
                {
                    SlotRef to = SlotRef.Equip(from.playerType, EquipSlotType.Bag);
                    DragPayload payload = new DragPayload(from);
                    payload.SetTo(to);
                    return TryTransferBetweenSlots(payload);
                }
            case ItemType.Accessory:
                {
                    if (equipment == null) return false;

                    EquipmentSlot acc1 = equipment.GetSlot(EquipSlotType.Accessory1);
                    EquipmentSlot acc2 = equipment.GetSlot(EquipSlotType.Accessory2);

                    EquipSlotType targetSlot;

                    if(acc1 ==  null || acc2 == null) return false;

                    if (acc1.isEmpty)
                        targetSlot = EquipSlotType.Accessory1;
                    else if (acc2.isEmpty)
                        targetSlot = EquipSlotType.Accessory2;
                    else
                        targetSlot = EquipSlotType.Accessory1;

                    SlotRef to = SlotRef.Equip(from.playerType, targetSlot);
                    DragPayload payload = new DragPayload(from);
                    payload.SetTo(to);
                    return TryTransferBetweenSlots(payload);
                }
            case ItemType.Consumable:
                return TryUseConsumableFromInventory(from);

            default:
                return false;
        }
    }
    private static bool TryUseConsumableFromInventory(SlotRef from)
    {
        if(from.slotType != SlotType.Inventory) return false;

        PlayerData playerData = PlayerManager.Instance.GetPlayerData(from.playerType);
        if(playerData == null || playerData.Inventory == null) return false;

        Inventory inventory = playerData.Inventory;
        InventorySlot inventorySlot = inventory.GetSlot(from.index);
        if(inventorySlot == null || inventorySlot.isEmpty) return false;
        if(!inventorySlot.IsStack) return false;

        CommonItemData common = ItemDB.GetCommon(inventorySlot.itemId);
        if(common == null) return false;

        Debug.Log("아이템 사용");
        return inventory.TryRemoveStack(from.index, 1);
    }
    public static int GetEquippedItemId(SlotRef from)
    {
        if (from.slotType != SlotType.Equipment) return 0;

        PlayerData data = PlayerManager.Instance.GetPlayerData(from.playerType);
        if (data == null || data.Equipment == null) return 0;

        EquipmentSlot equipmentSlot = data.Equipment.GetSlot(from.equipSlot);
        if (equipmentSlot == null || equipmentSlot.isEmpty) return 0;

        return equipmentSlot.GetItemId();
    }
    private static bool TryUseConsumableFromQuickSlot(PlayerType playerType, int quickSlotIndex)
    {
        PlayerData playerData = PlayerManager.Instance.GetPlayerData(playerType);
        if(playerData == null || playerData.QuickSlot == null || playerData.Inventory == null) return false;

        QuickSlot quickSlot = playerData.QuickSlot;
        Inventory inventory = playerData.Inventory;

        QuickSlotSlot quickSlotSlot = quickSlot.GetSlot(quickSlotIndex);
        if(quickSlotSlot == null || quickSlotSlot.isEmpty) return false;
        if (!quickSlotSlot.IsStack) return false;

        CommonItemData common = ItemDB.GetCommon(quickSlotSlot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;
        if(itemType != ItemType.Consumable) return false;

        int itemId = quickSlotSlot.itemId;
        int invIndex = -1;

        for(int i = 0; i< inventory.Capacity; i++)
        {
            InventorySlot inventorySlot = inventory.GetSlot(i);
            if(inventorySlot == null || inventorySlot.isEmpty) continue;

            if(inventorySlot.IsStack && inventorySlot.itemId == itemId)
            {
                invIndex = i;
                break;
            }
        }
        if(invIndex < 0) return false;

        //아이템 사용함

        bool success = inventory.TryRemoveStack(invIndex, 1);
        if(!success) return false;

        return true;
    }
}