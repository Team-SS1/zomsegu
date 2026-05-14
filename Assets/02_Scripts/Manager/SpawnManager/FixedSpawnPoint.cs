using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using SpawnEnum;
using UnityEngine;
using EventEnum;

[System.Serializable]
public class FixedSpawnPointEntry
{
    public FixedSpawnPointKey key;
    public Transform point;
}

public class FixedSpawnPoint : MonoBehaviour
{
    [SerializeField] private FixedSpawnPointEntry[] fixedSpawnPoints;
    public CinemachineVirtualCamera cinemachine;

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
    private void Start()
    {
        Transform point = GetPoint(FixedSpawnPointKey.Chapcter1PlayerSpawnPoint);
        GameObject player = SpawnManager.Instance.Spawn(SpawnID.Player, point);
        cinemachine.Follow = player.transform;

        EventManager.TriggerEvent<Transform>(EventKey.PlayerSpawned, player.transform);
    }
}
