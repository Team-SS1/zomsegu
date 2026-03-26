using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class MonsterKnockback : MonoBehaviour
{
    private Monster monster;
    private Zombie zombie;

    [Header("Knockback")]
    public float knockbackForce = 3.5f;
    public float knockbackDuration = 0.15f;

    public bool IsKnockbacking { get; private set; }

    private Rigidbody2D rb;
    private Coroutine knockbackRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 둘 다 받아두기
        monster = GetComponent<Monster>();
        zombie = GetComponent<Zombie>();
    }

    public void Apply(Vector2 dir)
    {
        if (rb == null)
            return;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        if (EffectPool.Instance != null)
        {
            Vector2 hitPos = GetHitEffectOrigin();
            EffectPool.Instance.SpawnHit(hitPos, dir);
        }

        float resist = GetStaggerResistance01();

        // 저항이 높을수록 짧고 약하게
        float finalForce = knockbackForce * (1f - resist);
        float finalDuration = knockbackDuration * (1f - resist);

        // 너무 0에 가까워지지 않게
        finalForce = Mathf.Max(0.05f, finalForce);
        finalDuration = Mathf.Max(0.02f, finalDuration);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(dir.normalized, finalForce, finalDuration));
    }

    public void StopImmediate()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        IsKnockbacking = false;

        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    private IEnumerator KnockbackRoutine(Vector2 dir, float force, float duration)
    {
        IsKnockbacking = true;

        float timer = duration;
        rb.velocity = Vector2.zero;

        while (timer > 0f)
        {
            rb.velocity = dir * force;
            timer -= Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        IsKnockbacking = false;
        knockbackRoutine = null;
    }

    private float GetStaggerResistance01()
    {
        if (zombie != null && zombie.stat != null)
            return Mathf.Clamp01(zombie.stat.StaggerResistance);

        return 0f;
    }

    private Vector2 GetHitEffectOrigin()
    {
        if (zombie != null)
            return zombie.GetBodyOrigin();

        return transform.position;
    }
}