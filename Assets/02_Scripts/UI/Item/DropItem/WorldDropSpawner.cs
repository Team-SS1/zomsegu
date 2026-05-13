using UnityEngine;

public class WorldDropSpawner : MonoBehaviour // 월드에 아이템 드롭을 생성하는 스크립트
{
    [Header("Prefab")]
    [SerializeField] private WorldLootObject worldLootPrefab;

    [Header("Parent")]
    [SerializeField] private Transform lootRoot; // 드롭된 아이템 오브젝트들이 위치할 부모 트랜스폼

    [Header("Drop Position")] 
    [SerializeField] private Transform dropOrigin; // 드롭이 시작되는 위치 (예: 적이 죽은 위치)
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private float randomRadius = 0.25f;

    [Header("Direction")]
    [SerializeField] private Vector2 defaultDropDirection = Vector2.down;

    public bool CanSpawn() // 드롭을 생성할 수 있는지 여부
    {
        return worldLootPrefab != null && dropOrigin != null;
    }
    public void SetDropOrigin(Transform origin)
    {
        dropOrigin = origin;
    }
    public bool TrySpawnDrop(LootSource lootSource, int iconItemId, out WorldLootObject spawnedObject)
    {
        spawnedObject = null;
        
        if(lootSource == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("WorldDropSpawner: LootSource is null. Cannot spawn drop.");
            return false;
#endif
        }

        if(worldLootPrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogError("WorldDropSpawner: WorldLootObject prefab is not assigned!");
            return false;
#endif
        }

        if(dropOrigin == null)
        {
#if UNITY_EDITOR
            Debug.LogError("WorldDropSpawner: Drop origin is not assigned!");
            return false;
#endif
        }

        Vector3 spawnPosition = CalculateDropPosition();
        Sprite icon = LoadIcon(iconItemId);

        Transform parent = lootRoot != null ? lootRoot : null;

        spawnedObject = Instantiate(
            worldLootPrefab,
            spawnPosition,
            Quaternion.identity,
            parent);

        spawnedObject.Init(lootSource, icon);
        return true;
    }

    private Vector3 CalculateDropPosition()
    {
        Vector2 direction = defaultDropDirection;

        if (direction.sqrMagnitude <= 0.001f) // 별 차이 없으면 그냥 그대로
            direction = Vector2.down;

        direction.Normalize();

        Vector3 basePosition = dropOrigin.position + (Vector3)(direction * dropDistance);
        Vector2 randomOffset = Random.insideUnitCircle * randomRadius;

        Vector3 finalPosition = basePosition + (Vector3)randomOffset;
        finalPosition.z = 0f;

        return finalPosition;
    } 
    private Sprite LoadIcon(int itemId)
    {
        if(itemId == 0)
            return null;

        string iconPath = ItemDB.GetIconPath(itemId);

        if(string.IsNullOrEmpty(iconPath))
            return null;

        return Resources.Load<Sprite>(iconPath);
    }
}
