using System.Collections.Generic;
using PlayerEnum;

public static class SearchLootService // 탐색창 아이템 줍는 메서드
{
    public static bool TryPickupEntry(PlayerType playerType, SearchDisplayEntry entry)
    {
        if (entry == null || entry.source == null || entry.itemId == 0) return false;

        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null || playerData.Inventory == null) return false;

        Inventory inventory = playerData.Inventory;

        if (entry.IsStackGroup) //스택되어 있는 그룹일 때
            return TryPickupStackEntry(inventory, entry);

        return TryPickupInstanceEntry(inventory, entry); //낱개형 아이템
    }
    private static bool TryPickupStackEntry(Inventory inventory, SearchDisplayEntry entry) //스택 아이템 줍기
    {
        int totalAmount = entry.TotalAmount;
        if (totalAmount <= 0)
            return false;

        if (!inventory.TryAddStack(entry.itemId, totalAmount)) // 이거 한번에 다 넣는거여서 나중에 숫자 조정해서 획득할거면 수정해라
            return false;

        RemoveLootItemsFromSource(entry.source, entry.lootItems);
        return true;
    }
    private static bool TryPickupInstanceEntry(Inventory inventory, SearchDisplayEntry entry) // 낱개 아이템 줍기
    {
        if (entry.lootItems == null || entry.lootItems.Count == 0)
            return false;

        LootItem lootItem = entry.lootItems[0];
        if (lootItem == null || lootItem.instance == null)
            return false;

        if(!inventory.TryAddInstance(lootItem.itemId, lootItem.instance))
            return false;

        entry.source.RemoveItem(lootItem);
        return true;
    }
    private static void RemoveLootItemsFromSource(LootSource source, List<LootItem> lootItems)
    {
        if(source == null || lootItems == null) return;

        for(int i = lootItems.Count - 1; i >= 0; i--)
        {
            LootItem lootItem = lootItems[i];
            if (lootItem == null)
                continue;

            source.RemoveItem(lootItem);
        }
    }
}
