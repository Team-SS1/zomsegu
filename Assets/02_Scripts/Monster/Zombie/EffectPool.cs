using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance;

    [SerializeField] ZombieHitEffect hitPrefab;
    ObjectPool<ZombieHitEffect> pool;

    private void Awake()
    {
        Instance = this;
        pool = new ObjectPool<ZombieHitEffect>(hitPrefab, 30, transform);
    }

    public void SpawnHit(Vector2 pos, Vector2 dir)
    {
        pool.Get().Play(pos, dir);
    }

    public void Return(ZombieHitEffect fx)
    {
        pool.Return(fx);
    }
}