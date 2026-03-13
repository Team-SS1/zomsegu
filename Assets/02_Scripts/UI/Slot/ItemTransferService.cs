using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemEnum;
using PlayerEnum;
using UnityEngine.Scripting.APIUpdating;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public static class ItemTransferService
{
    public static bool TryTransferBetweenSlots(DragPayload payload) // 슬롯 간 이동
    {
        if (payload == null || !payload.hasTo) return false;

        SlotRef from = payload.from;
        SlotRef to = payload.to;

        if(from.playerType != to.playerType) return false; // 플레이어 간 이동은 원래도 안되지만 일단 여기서도 막아둠

        if (from.slotType == SlotType.Inventory && to.slotType == SlotType.Inventory) // 인벤 -> 인벤 이동
            return TryInventoryToInventory(from, to);

        if(from.slotType == SlotType.Inventory && to.slotType == SlotType.Equipment) // 인벤 -> 장비 이동
            return TryInventoryToEquipment(from, to);

        if(from.slotType == SlotType.Equipment && to.slotType == SlotType.Inventory) // 장비 -> 인벤 이동
            return TryEquipmentToInventory(from, to);

        if(from.slotType == SlotType.Inventory && to.slotType == SlotType.QuickSlot) // 인벤 -> 퀵슬롯 이동
            return TryInventoryToQuickSlot(from, to);

        if(from.slotType == SlotType.DropItem && to.slotType == SlotType.Inventory) // 드롭아이템 -> 인벤 이동
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

        return MoveBetweenInventories(fromInv, from.index, toInv, emptySlotIndex, amount);
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
    private static bool TryInventoryToEquipment(SlotRef from, SlotRef to) // 인벤에서 장비로 이동
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(from.playerType);
        if(data == null) return false;

        Inventory inventory = data.Inventory;
        Equipment equipment = data.Equipment;

        if(inventory == null || equipment == null) return false;

        InventorySlot fromSlot = inventory.GetSlot(from.index);
        if(fromSlot == null || fromSlot.isEmpty) return false;
        
        int itemId = fromSlot.itemId;
        if(!ItemDB.CanEquipToSlot(itemId, to.equipSlot)) return false;

        EquipmentSlot equipSlot = equipment.GetSlot(to.equipSlot);
        if(equipSlot == null) return false;

        if (fromSlot.IsStack && ItemDB.IsRangedWeapon(itemId)) //인벤 아이템 : 원거리 아이템
        {
            if (equipSlot.isEmpty)
            {
                return equipment.EquipRangedItem(to.equipSlot, itemId);
            }

            if (equipSlot.HasRangedWeapon)
            {
                return equipment.SwapRangedItem(to.equipSlot, itemId, out _, out _);
            }
            
        }

        if(!fromSlot.IsInstance || fromSlot.instance == null) return false; // 여기서부턴 인스턴스형 아이템 장착 

        ItemStack instance = fromSlot.instance;
        string guid = instance.guid;

        if (equipSlot.isEmpty)
        {
            if (!inventory.TryRemoveInstance(guid, out ItemStack removed)) return false;
            if(!equipment.EquipInstance(to.equipSlot, removed))
            {
                inventory.TryAddInstance(removed.itemId, removed);
                return false;
            }
            return true;
        }

        if (equipSlot.HasRangedWeapon) //기존 장착 아이템이 원거리면 인벤쪽 인스턴스형 아이템 제거
        {
            if (!equipment.SwapInstance(to.equipSlot, instance, out _, out _)) return false;
            return inventory.TryRemoveInstance(guid, out _);
        }
        if (equipSlot.HasInstance) // 기존 장착 아이템이 인스턴스형이면 스왑
        {
            ItemStack equippedItem = equipSlot.equippedItem;
            if(!equipment.SwapInstance(to.equipSlot, instance, out ItemStack oldStack, out _)) return false;
            if(!inventory.TryRemoveInstance(guid, out _)) return false;

            return inventory.TryAddInstance(oldStack.itemId, oldStack);
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

        if (equipSlot.HasRangedWeapon) //아이템이 원거리 아이템이면 그냥 등록 해제
        {
            return equipment.UnEquip(from.equipSlot, out _, out _);
        }
        if(!equipSlot.HasInstance || equipSlot.equippedItem == null) return false;

        ItemStack equippedItem = equipSlot.equippedItem;

        if (inventorySlot.isEmpty) //장비 -> 인벤 그냥 옮기기
        {
            if(!equipment.UnEquip(from.equipSlot, out  ItemStack removed, out _))return false;

            if(!inventory.TryAddInstance(equippedItem.itemId, equippedItem))
            {
                equipment.EquipInstance(from.equipSlot, removed);
                return false;
            }
            return true;
        }

        if(!inventorySlot.IsInstance || inventorySlot.instance == null)return false;
        if(!ItemDB.CanEquipToSlot(inventorySlot.itemId, from.equipSlot)) return false;

        ItemStack invItem = inventorySlot.instance;
        string invGuid = invItem.guid;

        if (!equipment.SwapInstance(from.equipSlot, invItem, out ItemStack oldInstance, out _)) return false;
        if(!inventory.TryRemoveInstance(invGuid, out _)) return false;

        return inventory.TryAddInstance(oldInstance.itemId, oldInstance);
    }
    private static bool TryInventoryToQuickSlot(SlotRef from, SlotRef to) // 인벤에서 퀵슬롯으로 이동
    {
        //아직 구현 안함
        Debug.Log("인벤 -> 퀵슬롯");
        return false;
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

            Debug.Log($"인스턴스 아이템 {itemId} 버리기");
            return true;
        }
        
        return false;
    }
    private static bool TryClearQuickSlot(SlotRef from) // 퀵슬롯에서 외부로 드롭 (퀵슬롯 제외)
    {
        //아직 구현 안함
        Debug.Log("퀵슬롯 등록 해제");
        return false;
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

            return fromInv.TryRemoveInstance(guid, out _);
        }

        return false;
    }
}