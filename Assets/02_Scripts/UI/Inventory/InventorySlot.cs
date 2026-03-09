using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public int itemId; // 공통 아이템 ID
    public int amount; // 스택형
    public ItemStack instance; // 인스턴스형 (장비, 근접 무기)

    public bool isEmpty => itemId == 0;
    public bool IsStack => itemId != 0 && instance == null;
    public bool IsInstance => itemId != 0 && instance != null;

    public void Clear()
    {
        itemId = 0;
        amount = 0;
        instance = null;
    }
}
