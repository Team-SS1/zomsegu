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

        knockbackRoutine = StartCoroutine(KnockbackRoutine(dir.normalized));
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

    private IEnumerator KnockbackRoutine(Vector2 dir)
    {
        IsKnockbacking = true;

        float timer = knockbackDuration;

        rb.velocity = Vector2.zero;

        while (timer > 0f)
        {
            rb.velocity = dir * knockbackForce;
            timer -= Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        IsKnockbacking = false;
        knockbackRoutine = null;
    }

    private Vector2 GetHitEffectOrigin()
    {
        if (zombie != null)
            return zombie.GetBodyOrigin();

        return transform.position;
    }
}