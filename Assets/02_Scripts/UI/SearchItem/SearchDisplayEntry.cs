using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SearchDisplayEntry // 탐색창에 보이는 한줄
{
    [Header("Base")]
    public int itemId;
    public ItemStack representativeInstance;

    [Header("Merged Items")]
    public List<LootItem> lootItems = new List<LootItem>();

    public bool IsStackGroup => representativeInstance == null;
    public bool IsInstanceEntry => representativeInstance != null;

    public int TotalAmount
    {
        get
        {
            if (lootItems == null || lootItems.Count == 0)
                return 0;

            int total = 0;

            for(int i =0; i < lootItems.Count; i++)
            {
                LootItem lootItem = lootItems[i];
                if(lootItem == null) continue;

                total += Mathf.Max(1, lootItem.amount);
            }

            return total;
        }
    }

    public void AddUnit(LootItem lootItem)
    {
        if (lootItem == null) return;

        if(itemId == 0)
            itemId = lootItem.itemId;

        if(lootItem.IsInstance && representativeInstance == null)
            representativeInstance = lootItem.instance;

        lootItems.Add(lootItem);
    }
}
