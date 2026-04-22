using ItemEnum;
using UnityEngine;

public static class DropTransferService
{
    internal static bool TryDropOutside(SlotRef from, int amount) // 인벤 아이템 밖으로 버리기
    {
        if (from.slotType == SlotType.Inventory)
            return TryInventoryToWorldDrop(from, amount);

        if (from.slotType == SlotType.QuickSlot)
            return QuickSlotService.TryClearQuickSlot(from);

        return false;
    }
    internal static bool TryDropItemToInventory(SlotRef from, SlotRef to) // 드롭 -> 인벤
    {
        //아직 구현 안함
        Debug.Log("드롭 -> 인벤");
        return false;
    }
    private static bool TryInventoryToWorldDrop(SlotRef from, int amount)
    {
        PlayerData data = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (data == null || data.Inventory == null) return false;

        Inventory inventory = data.Inventory;
        InventorySlot slot = inventory.GetSlot(from.index);

        if (slot == null || slot.isEmpty) return false;

        if (slot.IsStack)
        {
            int removeAmount = amount <= 0 ? slot.amount : amount;
            removeAmount = Mathf.Clamp(removeAmount, 1, slot.amount);

            int itemId = slot.itemId;

            bool removed = inventory.TryRemoveStack(from.index, removeAmount);
            if (!removed) return false;

            Debug.Log($"스택 아이템 {itemId} 버리기 {removeAmount}개"); //아직 구현 다 안끝남

            return true;
        }

        if (slot.IsInstance && slot.instance != null)
        {
            string guid = slot.instance.guid;
            int itemId = slot.instance.itemId;

            bool removed = inventory.TryRemoveInstance(guid, out ItemStack removedItem);
            if (!removed) return false;

            QuickSlotService.ValidateQuickSlots(from.playerType);

            Debug.Log($"인스턴스 아이템 {itemId} 버리기");
            return true;
        }

        return false;
    }
}
