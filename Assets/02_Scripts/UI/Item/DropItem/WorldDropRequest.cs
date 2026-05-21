using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldDropRequest // 드롭 요청 받고 성공 여부, 생성된 오브젝트 기록용
{
    public LootSource lootSource;
    public int iconItemId;

    public bool success;
    public WorldLootObject spawnedObject;

    public WorldDropRequest(LootSource lootSource, int iconItemId)
    {
        this.lootSource = lootSource;
        this.iconItemId = iconItemId;
        success = false;
        spawnedObject = null;
    }
    public void Complete(bool success, WorldLootObject spawnedObject)
    {
        this.success = success;
        this.spawnedObject = spawnedObject;
    }
}
