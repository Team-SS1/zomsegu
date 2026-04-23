using ItemEnum;

public static class SearchDisplayTextUtil // 탐색창에 보이는 텍스트 관련 유틸리티 클래스
{
    public static string GetDisplayName(SearchDisplayEntry entry)
    {
        if (entry == null || entry.itemId == 0)
            return string.Empty;

        string itemName = ItemDB.GetItemName(entry.itemId);

        if(entry.IsStackGroup && entry.TotalAmount > 1)
            return $"{itemName} x{entry.TotalAmount}";

        return itemName;
    }
    public static string GetTypeText(int itemId)
    {
        CommonItemData common = ItemDB.GetCommon(itemId);
        if (common == null) return string.Empty;

        bool isRangedWeapon = ItemDB.IsRangedWeapon(itemId);

        ItemType itemType = (ItemType)common.ItemType;

        return itemType switch
        {
            ItemType.Head => "머리 방어구",
            ItemType.Body => "몸통 방어구",
            ItemType.Leg => "다리 방어구",
            ItemType.Shoes => "신발",
            ItemType.Bag => "가방",
            ItemType.Weapon => isRangedWeapon ? "투척무기" : "근접무기", 
            ItemType.Accessory => "액세서리",
            ItemType.Consumable => "소모품",
            ItemType.Misc => "잡동사니",
            _ => "잡동사니"                  // 기본값을 "잡동사니"로 설정하여 예외 상황에 대비
        };
    }

}
