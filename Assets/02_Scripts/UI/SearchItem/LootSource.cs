using System.Collections.Generic;
using UnityEngine;
using ItemEnum;
using System;

[Serializable]
public class LootSource // 아이템이 생성되는 출처
{
    [Header("Identity")]
    public string sourceId;

    [Header("Display")]
    public string displayName;
    public LootSourceType sourceType;

    [Header("Items")]
    public List<LootItem> lootItems = new List<LootItem>();

    [Header("Display Rule")]
    public bool mergeStackableForDisplay = true; // 플레이어 드롭 아이템은 낱개로 보이지만, 다른 아이템들은 합쳐져서 보이는 경우가 있음. 이 옵션은 그런 경우에 사용됨.

    [Header("Investigation")]
    public bool requiredInvestigation; //조사 필요 여부
    public float baseInvestigationTime = 0f; // 조사에 필요한 기본 시간 (초 단위)

    public LootSource(string displayName, LootSourceType sourceType)
    {
        this.sourceId = Guid.NewGuid().ToString("N");
        this.displayName = displayName;
        this.sourceType = sourceType;
    }
    public void AddItem(LootItem lootItem)
    {
        if (lootItem == null) return;
        lootItems.Add(lootItem);
    }
    public bool RemoveItem(LootItem lootItem)
    {
        if (lootItem == null) return false;
        return lootItems.Remove(lootItem);
    }
}
