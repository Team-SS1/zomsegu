using ItemEnum;
using UnityEngine;

public static class DropTransferService
{
    internal static bool TryDropOutside(SlotRef from, int amount, WorldDropSpawner worldDropSpawner) // 인벤 아이템 밖으로 버리기
    {
        if (from.slotType == SlotType.Inventory)
            return TryInventoryToWorldDrop(from, amount, worldDropSpawner);

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
    private static bool TryInventoryToWorldDrop(SlotRef from, int amount, WorldDropSpawner worldDropSpawner)
    {
        if(worldDropSpawner == null)
        {
#if UNITY_EDITOR
            Debug.LogError("DropTransferService : WorldDropSpawner 참조 없음");
#endif
            return false;
        }
        if (!worldDropSpawner.CanSpawn())
        {
#if UNITY_EDITOR
            Debug.LogError("DropTransferService : WorldDropSpawner가 드롭 생성 가능한 상태가 아닙니다.");
#endif
            return false;
        }
        PlayerData data = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (data == null || data.Inventory == null) return false;

        Inventory inventory = data.Inventory;
        InventorySlot slot = inventory.GetSlot(from.index);

        if (slot == null || slot.isEmpty) return false;

        if (slot.IsStack)
        {
            return TryDropStackItem(from, inventory, slot, amount, worldDropSpawner);
        }

        if (slot.IsInstance && slot.instance != null)
        {
            return TryDropInstanceItem(from, inventory, slot, worldDropSpawner);
        }

        return false;
    }
    private static bool TryDropStackItem(SlotRef from, Inventory inventory, InventorySlot slot, int amount, WorldDropSpawner worldDropSpawner)
    {
        int removeAmount = amount <= 0 ? slot.amount : amount;
        removeAmount = Mathf.Clamp(removeAmount, 1, slot.amount);

        int itemId = slot.itemId;

        LootSource lootSource = CreateDropLootSource(itemId);
        lootSource.AddItem(new LootItem(itemId, removeAmount));

        bool removed = inventory.TryRemoveStack(from.index, removeAmount);
        if (!removed) return false;

        bool spawned = worldDropSpawner.TrySpawnDrop(lootSource, itemId, out WorldLootObject spawnedObject);

        if (!spawned)
        {
#if UNITY_EDITOR
            Debug.LogError($"DropTransferService : 월드 드롭 생성 실패, itemId = {itemId}, amount = {removeAmount}");
#endif
            inventory.TryAddStack(itemId, removeAmount);
            return false;
        }
        QuickSlotService.ValidateQuickSlots(from.playerType);

        return true;
    }
    private static bool TryDropInstanceItem(SlotRef from, Inventory inventory, InventorySlot inventorySlot, WorldDropSpawner worldDropSpawner)
    {
        string guid = inventorySlot.instance.guid;
        int itemId = inventorySlot.instance.itemId;

        bool removed = inventory.TryRemoveInstance(guid, out ItemStack removedItem);
        if(!removed || removedItem == null) return false;

        LootSource lootSource = CreateDropLootSource(itemId);
        lootSource.AddItem(new LootItem(removedItem));

        bool spawned = worldDropSpawner.TrySpawnDrop(lootSource, itemId, out WorldLootObject spawnedObject);
        if (!spawned)
        {
#if UNITY_EDITOR
            Debug.LogError($"DropTransferService : 월드 드롭 생성 실패 itemId = {itemId}, guid = {guid}");
#endif
            inventory.TryAddInstance(itemId, removedItem);
            return false;
        }

        QuickSlotService.ValidateQuickSlots(from.playerType);

        return true;
    }
    private static LootSource CreateDropLootSource(int itemId)
    {
        string itemName = ItemDB.GetItemName(itemId);

        if (string.IsNullOrEmpty(itemName))
            itemName = $"Item {itemId}";

        LootSource lootSource = new LootSource(itemName, LootSourceType.GroundScan);
        lootSource.mergeStackableForDisplay = true;
        lootSource.requiredInvestigation = false;
        lootSource.baseInvestigationTime = 0f;

        return lootSource;
    }
}
