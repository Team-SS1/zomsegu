using ItemEnum;
using PlayerEnum;

public static class QuickSlotService
{
    internal static bool TryInventoryToQuickSlot(SlotRef from, SlotRef to) // 인벤 -> 퀴슬롯
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
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
        }
        else if (inventorySlot.IsInstance && inventorySlot.instance != null)
        {
            existingIndex = quickSlot.FindSlotByGuid(inventorySlot.instance.guid);
        }

        if (existingIndex >= 0 && existingIndex != to.index)
        {
            quickSlot.ClearSlot(existingIndex);
        }

        bool success = false;

        if (inventorySlot.IsStack)
        {
            success = quickSlot.BindStack(to.index, inventorySlot.itemId);
        }
        else if (inventorySlot.IsInstance && inventorySlot.instance != null)
        {
            success = quickSlot.BindInstance(to.index, inventorySlot.instance);
        }

        if (!success) return false;

        TryAutoEquipIfSelectedQuickSlot(from.playerType, to.index);
        return true;
    }
    internal static bool TryQuickSlotToQuickSlot(SlotRef from, SlotRef to) // 퀵슬롯 -> 퀵슬롯
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (playerData == null || playerData.QuickSlot == null) return false;

        return playerData.QuickSlot.Swap(from.index, to.index);
    }
    internal static bool TryClearQuickSlot(SlotRef from) // 퀵슬롯 등록 해제
    {
        if (from.slotType != SlotType.QuickSlot) return false;

        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (playerData == null || playerData.QuickSlot == null) return false;

        return playerData.QuickSlot.ClearSlot(from.index);
    }
    internal static bool TrySelectQuickSlot(PlayerType playerType, int quickSlotIndex) // 퀵슬롯 선택
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null || playerData.QuickSlot == null) return false;

        QuickSlot quickSlot = playerData.QuickSlot;
        if (quickSlotIndex < 0 || quickSlotIndex >= quickSlot.Capacity) return false;

        QuickSlotSlot slot = quickSlot.GetSlot(quickSlotIndex);
        if (slot == null) return false;

        quickSlot.SetSelectedIndex(quickSlotIndex);

        if (slot.isEmpty) return true;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        if (itemType == ItemType.Consumable)
            return ItemUseService.TryUseConsumableFromQuickSlot(playerType, quickSlotIndex);
        else if (itemType == ItemType.Weapon)
        {
            TryAutoEquipIfSelectedQuickSlot(playerType, quickSlotIndex);
            return true;
        }
        return true;
    }
    internal static void ValidateQuickSlots(PlayerType playerType) // 퀵슬롯에 등록된 아이템이 아직 유효한지
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null) return;

        QuickSlot quickSlot = playerData.QuickSlot;
        if (quickSlot == null) return;

        for (int i = 0; i < quickSlot.Capacity; i++)
        {
            QuickSlotSlot quickSlotSlot = quickSlot.GetSlot(i);
            if (quickSlotSlot == null || quickSlotSlot.isEmpty) continue;

            if (quickSlotSlot.IsInstance)
            {
                ItemStack instance = ItemTransferCommon.FindInstancePlayer(playerData, quickSlotSlot.guid);

                if (instance == null)
                {
                    quickSlot.ClearSlot(i);
                    continue;
                }
                if (instance.HasDurability && instance.durability <= 0)
                {
                    quickSlot.ClearSlot(i);
                    continue;
                }
            }
        }
    }
    //무기 아이템 장착시 자동으로 퀵슬롯 첫번째 칸에 등록
    internal static void AutoBindEquippedItemToFirstQuickSlot(PlayerType playerType, int itemId, ItemStack instance)
    {
        if (itemId == 0) return;

        CommonItemData common = ItemDB.GetCommon(itemId);
        if (common == null) return;

        ItemType itemType = (ItemType)common.ItemType;

        if (itemType != ItemType.Weapon) return;

        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null) return;

        QuickSlot quickSlot = playerData.QuickSlot;
        if (quickSlot == null) return;

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
    //무기 아이템이 등록된 퀵슬롯 번호 클릭시 자동 장착
    internal static void TryAutoEquipIfSelectedQuickSlot(PlayerType playerType, int quickSlotIndex)
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null) return;

        QuickSlot quickSlot = playerData.QuickSlot;

        if (quickSlot == null) return;

        QuickSlotSlot slot = quickSlot.GetSlot(quickSlotIndex);
        if (slot == null || slot.isEmpty) return;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if (common == null) return;

        ItemType itemType = (ItemType)common.ItemType;
        if (itemType != ItemType.Weapon) return;

        EquipmentTransferService.TryEquipWeaponFromQuickSlot(playerType, slot);
    }
    private static bool CanRegisterQuickSlot(InventorySlot slot) // 등록 가능한 아이템인지 확인
    {
        if (slot == null || slot.isEmpty) return false;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        if (itemType == ItemType.Consumable || itemType == ItemType.Weapon) return true;

        return false;
    }
}
