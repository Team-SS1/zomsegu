using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHitEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem ps;
    private Coroutine returnRoutine;

    private void Awake()
    {
        if (ps == null)
            ps = GetComponentInChildren<ParticleSystem>(true);
    }

    public void Play(Vector2 pos, Vector2 dir)
    {
        if (ps == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[ZombieHitEffect] ParticleSystem is missing on prefab.");
#endif
            return;
        }
        float yOffset = 1f; // 좀비 피격 이펙트 높이 보정
        transform.position = pos + Vector2.up * yOffset;

        // 풀 재사용 시 잔상/상태 꼬임 방지
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        var shape = ps.shape;
        shape.rotation = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        ps.Play();

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnAfterAlive());
    }

    IEnumerator ReturnAfterAlive()
    {
        while (ps != null && ps.IsAlive(true))
            yield return null;
        EffectPool.Instance.Return(this);
    }

    public void OnSpawn() { }
    public void OnDespawn()
    {
        StopAllCoroutines();
        returnRoutine = null;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}