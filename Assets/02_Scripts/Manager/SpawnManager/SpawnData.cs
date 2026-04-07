using System.Collections;
using System.Collections.Generic;
using SpawnEnum;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnData", menuName = "SO/Spawn/SpawnData")]
public class SpawnData : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private SpawnType spawnType;
    [SerializeField] private SpawnID[] spawnId;
    [SerializeField] private int spawnCount = 1;
    [SerializeField] private bool triggerSpawn = true;

    [Header("Spawn Range")]
    [SerializeField] private bool spawnRandom = false;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private float spawnTriggerRadius = 1.5f;

    [Header("Respawn")]
    [SerializeField] private bool respawn = false;
    [SerializeField] private float respawnDelay = 0f;

    public SpawnType SpawnType => spawnType;
    public SpawnID[] SpawnId => spawnId;
    public bool TriggerSpawn => triggerSpawn;
    public int SpawnCount => spawnCount;
    public bool SpawnRandom => spawnRandom;
    public float SpawnRadius => spawnRadius;
    public float SpawnTriggerRadius => spawnTriggerRadius;
    public bool Respawn => respawn;
    public float RespawnDelay => respawnDelay;
}
