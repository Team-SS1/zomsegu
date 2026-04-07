using System.Collections.Generic;
using SpawnEnum;
using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public SpawnID id;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "SpawnPrefabDatabase", menuName = "SO/Spawn/SpawnPrefabDatabase")]
public class SpawnPrefabDatabase : ScriptableObject
{
    [SerializeField] private List<SpawnEntry> entries = new();

    public GameObject GetPrefab(SpawnID id)
    {
        foreach (var entry in entries)
        {
            if (entry.id == id)
                return entry.prefab;
        }

        Debug.LogWarning($"[SpawnPrefabDatabase] Prefab not found: {id}");
        return null;
    }
}