using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterEnum;

public class MonsterPool : MonoBehaviour
{
    public static MonsterPool Instance;

    [SerializeField] private List<MonsterPoolEntry> entries;

    private Dictionary<ZombieType, ObjectPool<Zombie>> pools;

    private void Awake()
    {
#if UNITY_EDITOR
        Debug.Log("MonsterPool Awake");
#endif
        Instance = this;
        pools = new Dictionary<ZombieType, ObjectPool<Zombie>>();

        foreach (var e in entries)
        {
            if (e.prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"받은 프리팹이 null: {e.type}");
#endif
                continue;
            }

            if (pools.ContainsKey(e.type))
            {
#if UNITY_EDITOR
                Debug.LogError($"중복 타입: {e.type}");
#endif
                continue;
            }

            var pool = new ObjectPool<Zombie>(e.prefab, e.initialSize, transform);
            pools.Add(e.type, pool);
        }
    }

    public Zombie Spawn(ZombieType type, Vector3 pos)
    {
        if (!pools.TryGetValue(type, out var pool))
        {
#if UNITY_EDITOR
            Debug.LogError($"No pool for monster type: {type}");
#endif
            return null;
        }

        var zombie = pool.Get();
        zombie.transform.position = pos;
        return zombie;
    }

    public void Return(Zombie zombie)
    {
        if (zombie == null)
            return;

#if UNITY_EDITOR
        Debug.Log($"[Pool] Return {zombie.name}");
#endif

        if (!pools.TryGetValue(zombie.zombieType, out var pool))
        {
#if UNITY_EDITOR
            Debug.LogError($"No pool found for zombie type: {zombie.zombieType}");
#endif
            return;
        }

        pool.Return(zombie);
    }
}