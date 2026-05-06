using System;
using UnityEngine;

[Serializable]
public class LootItem // 실제 존재하는 아이템
{
    [Header("Identity")]
    public string worldLootId;

    [Header("Item")]
    public int itemId;
    public ItemStack instance;

    [Header("Stack")]
    public int amount = 1;

    public bool IsStack => instance == null;
    public bool IsInstance => instance != null;

    public LootItem(int itemId, int amount = 1)
    {
        this.worldLootId = Guid.NewGuid().ToString("N");
        this.itemId = itemId;
        this.instance = null;
        this.amount = Mathf.Max(1, amount);
    }
    public LootItem(ItemStack instance)
        {
            this.worldLootId = Guid.NewGuid().ToString("N");
            this.itemId = instance != null ? instance.itemId : 0;
            this.instance = instance;
            this.amount = 1;
        }
}
