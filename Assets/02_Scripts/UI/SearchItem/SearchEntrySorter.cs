using System.Collections.Generic;

public static class SearchEntrySorter
{
    public static void Sort(List<SearchDisplayEntry> entries)
    {
        if (entries == null) return;

        entries.Sort(Compare);
    }
    private static int Compare(SearchDisplayEntry a, SearchDisplayEntry b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        
        CommonItemData commonA = ItemDB.GetCommon(a.itemId);
        CommonItemData commonB = ItemDB.GetCommon(b.itemId);

        if(commonA == null && commonB == null) return 0;
        if(commonA == null) return 1;
        if(commonB == null) return -1;

        int rarityCompare = commonA.ItemRarity.CompareTo(commonB.ItemRarity); // 아이템 희귀도 오름차순 정렬 (낮은 희귀도가 먼저 오도록)
        if (rarityCompare != 0)
            return rarityCompare;

        return commonB.ItemID.CompareTo(commonA.ItemID); // 아이템 ID 내림차순 정렬 (높은 ID가 먼저 오도록)
    }
}
