using ItemEnum;
using PlayerEnum;
using UnityEngine;

public static class InventoryTransferService
{
    internal static bool TryInventoryToInventory(SlotRef from, SlotRef to) // 인벤 -> 인벤
    {
        PlayerData data = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (data == null || data.Inventory == null) return false;

        return data.Inventory.Move(from.index, to.index);
    }
    internal static bool TryGiveToOtherPlayer(SlotRef from, PlayerType targetPlayer, int amount)
    {
        if (from.slotType != SlotType.Inventory) return false;
        if (from.playerType == targetPlayer) return false;

        PlayerData fromData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        PlayerData toData = PlayerDataManager.Instance.GetPlayerData(targetPlayer);

        if (fromData == null || toData == null) return false;

        Inventory fromInv = fromData.Inventory;
        Inventory toInv = toData.Inventory;

        if (fromInv == null || toInv == null) return false;

        InventorySlot fromSlot = fromInv.GetSlot(from.index);
        if (fromSlot == null || fromSlot.isEmpty) return false;

        int emptySlotIndex = ItemTransferCommon.FindEmptySlot(toInv);
        if (emptySlotIndex < 0) return false;

        bool success = MoveBetweenInventories(fromInv, from.index, toInv, emptySlotIndex, amount);
        if (success)
        {
            QuickSlotService.ValidateQuickSlots(from.playerType);
            QuickSlotService.ValidateQuickSlots(targetPlayer);
        }
        return success;
    } 
    internal static bool MoveBetweenInventories(Inventory fromInv, int fromIndex, Inventory toInv, int toIndex, int amount)
    {
        if (fromInv == null || toInv == null) return false;

        InventorySlot fromSlot = fromInv.GetSlot(fromIndex);
        InventorySlot toSlot = toInv.GetSlot(toIndex);

        if (fromSlot == null || fromSlot.isEmpty) return false;
        if (toSlot == null || !toSlot.isEmpty) return false;

        if (fromSlot.IsStack)
        {
            int itemId = fromSlot.itemId;
            int moveAmount = amount <= 0 ? fromSlot.amount : amount;
            moveAmount = Mathf.Clamp(moveAmount, 1, fromSlot.amount);

            if (!toInv.TryAddStack(itemId, moveAmount)) return false;

            bool removed = fromInv.TryRemoveStack(fromIndex, moveAmount);
            if (!removed)
            {
                toInv.TryRemoveStack(toIndex, moveAmount); 
                return false;
            }
            return true;
        }
        if (fromSlot.IsInstance)
        {
            ItemStack instance = fromSlot.instance;
            string guid = instance.guid;

            if (!toInv.TryAddInstance(instance.itemId, instance)) return false;

            bool removed = fromInv.TryRemoveInstance(guid, out _);
            if (!removed)
            {
                toInv.TryRemoveInstance(guid, out _);
                return false;
            }
            return true;
        }

        return false;
    }
    
}
