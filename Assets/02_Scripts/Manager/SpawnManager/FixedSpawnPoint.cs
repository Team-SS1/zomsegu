using System.Collections;
using System.Collections.Generic;
using SpawnEnum;
using UnityEngine;

[System.Serializable]
public class FixedSpawnPointEntry
{
    public FixedSpawnPointKey key;
    public Transform point;
}

public class FixedSpawnPoint : MonoBehaviour
{
    [SerializeField] private FixedSpawnPointEntry[] fixedSpawnPoints;

    public Transform GetPoint(FixedSpawnPointKey key)
    {
        foreach (var entry in fixedSpawnPoints)
        {
            if (entry.key == key)
                return entry.point;
        }

        Debug.LogWarning($"Key 값 null: {key}");
        return null;
    }
    private void Awake()
    {
        Transform point = GetPoint(FixedSpawnPointKey.Chapcter1PlayerSpawnPoint);
        SpawnManager.Instance.Spawn(SpawnID.Player, point);
    }
}
