using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ItemStack
{
    public string guid;
    public int itemId;

    //내구도 (장비 / 근접 무기)
    public int durability;
    public int maxDurability;

    public ItemStack(int itemId, int durability = 0, int maxDurability = 0)
    {
        this.guid = ItemGuid.NewGuid();
        this.itemId = itemId;
        this.durability = durability;
        this.maxDurability = maxDurability;
    }

    public bool HasDurability => maxDurability > 0;
}
