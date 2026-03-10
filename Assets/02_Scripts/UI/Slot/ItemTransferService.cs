using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemEnum;
using PlayerEnum;

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
        if(emptySlotIndex < 0) return false;

        return MoveBetweenInventories(fromInv, from.index, toInv, emptySlotIndex);
    }
    public static bool TryDromOutside(SlotRef from) // 슬롯에서 외부로 드롭
    {
        if(from.slotType == SlotType.Inventory)
            return TryInventorytoWorldDrop(from);

        if(from.slotType == SlotType.QuickSlot)
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
        //아직 구현 안함
        return false;
    }
    private static bool TryEquipmentToInventory(SlotRef from, SlotRef to) // 장비에서 인벤으로 이동
    {
        //아직 구현 안함
        return false;
    }
    private static bool TryInventoryToQuickSlot(SlotRef from, SlotRef to) // 인벤에서 퀵슬롯으로 이동
    {
        //아직 구현 안함
        return false;
    }
    private static bool TryDropItemToInventory(SlotRef from, SlotRef to) // 드롭아이템에서 인벤으로 이동
    {
        //아직 구현 안함
        return false;
    }

    //외부로 드래그 관련
    private static bool TryInventorytoWorldDrop(SlotRef from) // 인벤에서 월드 드롭
    {
        //아직 구현 안함
        return false;
    }
    private static bool TryClearQuickSlot(SlotRef from) // 퀵슬롯에서 외부로 드롭 (퀵슬롯 제외)
    {
        //아직 구현 안함
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
    private static bool MoveBetweenInventories(Inventory fromInv, int fromIndex, Inventory toInv, int toIndex) 
    {
        if(fromInv == null || toInv == null) return false;

        InventorySlot fromSlot = fromInv.GetSlot(fromIndex);
        InventorySlot toSlot = toInv.GetSlot(toIndex);

        if(fromSlot == null || fromSlot.isEmpty) return false;
        if(toSlot == null || !toSlot.isEmpty) return false;

        if (fromSlot.IsStack)
        {
            int itemId = fromSlot.itemId;
            int amount = fromSlot.amount;

            if(!toInv.TryAddStack(itemId, amount)) return false;

            return fromInv.TryRemoveStack(fromIndex, amount);
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