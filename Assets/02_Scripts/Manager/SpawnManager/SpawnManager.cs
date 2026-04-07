using System.Collections;
using System.Collections.Generic;
using SpawnEnum;
using UnityEngine;

public class SpawnManager : GlobalSingleton<SpawnManager>
{
    [SerializeField] private SpawnPrefabDatabase database;

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Random ID, Position Spawn * Random Count
    /// </summary>
    /// <param name="point"></param>
    public void Spawn(SpawnPoint point)
    {
        SpawnData data = point.SpawnData;

        if (data == null)
        {
            Debug.LogWarning("SpawnData가 없습니다.");
            return;
        }
        for (int i = 0; i < data.SpawnCount; i++)
        {
            int randomIndex = Random.Range(0, data.SpawnId.Length);
            SpawnID randomID = data.SpawnId[randomIndex];

            GameObject prefab = database.GetPrefab(randomID);

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab 없음: {randomID}");
                return;
            }

            float radius = data.SpawnRadius; // SpawnData에 있어야 함
            Vector2 offset = Random.insideUnitCircle * radius;

            Vector3 spawnPos = point.transform.position + new Vector3(offset.x, offset.y, 0f);

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
    /// <summary>
    /// Fixed Spawn / Player, Partner, NPC
    /// </summary>
    /// <param name="spawnId"></param>
    /// <param name="spawnPoint"></param>
    /// <returns></returns>
    public GameObject Spawn(SpawnID spawnId, Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnPoint가 없습니다.");
            return null;
        }

        GameObject prefab = database.GetPrefab(spawnId);

        if (prefab == null)
        {
            Debug.LogWarning($"Prefab 없음: {spawnId}");
            return null;
        }

        return Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
