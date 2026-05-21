using UnityEngine;
using EventEnum;

public class WorldDropSpawner : MonoBehaviour // 월드에 아이템 드롭을 생성하는 스크립트
{
    [Header("Prefab")]
    [SerializeField] private WorldLootObject worldLootPrefab;

    [Header("Parent")]
    [SerializeField] private Transform lootRoot; // 드롭된 아이템 오브젝트들이 위치할 부모 트랜스폼

    [Header("Drop Position")] 
    [SerializeField] private Transform dropOrigin; // 드롭이 시작되는 위치 (예: 적이 죽은 위치)
    [SerializeField] private float minDropRadius = 0.4f;
    [SerializeField] private float maxDropRadius = 0.8f;

    private void OnEnable()
    {
        EventManager.Subscribe<Transform>(EventKey.PlayerSpawned, OnPlayerSpawned);
        EventManager.Subscribe<WorldDropRequest>(EventKey.WorldDropRequested, OnWorldDropRequested);
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<Transform>(EventKey.PlayerSpawned, OnPlayerSpawned);
        EventManager.UnSubscribe<WorldDropRequest>(EventKey.WorldDropRequested, OnWorldDropRequested);
    }
    private void OnPlayerSpawned(Transform playerTransform)
    {
        if (playerTransform == null) return;

        SetDropOrigin(playerTransform);
#if UNITY_EDITOR
        Debug.Log("WorldDropSpawner: Player Transform 연결 완료");
#endif
    }
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
#endif
            return false;
        }

        if(worldLootPrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogError("WorldDropSpawner: WorldLootObject prefab is not assigned!");
#endif
            return false;
        }

        if(dropOrigin == null)
        {
#if UNITY_EDITOR
            Debug.LogError("WorldDropSpawner: Drop origin is not assigned!");
#endif
            return false;
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
        if(dropOrigin == null)
            return Vector3.zero;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minDropRadius, maxDropRadius);

        Vector2 dropPosition = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle))*distance;

        Vector3 finalPosition = dropOrigin.position + (Vector3)dropPosition;
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
    private void OnWorldDropRequested(WorldDropRequest request)
    {
        if (request == null) return;
        if (request.success) return;

        bool spawned = TrySpawnDrop(request.lootSource, request.iconItemId, out WorldLootObject spawnedObject);
        request.Complete(spawned, spawnedObject);
    }
}
