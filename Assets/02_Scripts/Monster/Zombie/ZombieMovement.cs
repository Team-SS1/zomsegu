using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Zombie))]
[RequireComponent(typeof(Rigidbody2D))]
public class ZombieMovement : MonoBehaviour
{
    private Zombie z;
    private Rigidbody2D rb;

    private const float SpeedConvertRatio = 1f / 40f;

    private void Awake()
    {
        z = GetComponent<Zombie>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        //if (TimeManager.Instance.IsStopped(StopType.Monster))
        //{
        //    rb.velocity = Vector2.zero;
        //    return;
        //}

        if (z.Knockback != null && z.Knockback.IsKnockbacking)
            return;

        if (z.IsAttacking || z.IsTakingDamage || z.IsDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (!z.IsWalking && !z.IsRunning)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 dir = z.MoveDirection;
        if (dir.sqrMagnitude < 0.0001f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float baseMoveSpeed = z.stat.MoveSpeed;
        float runMul = z.IsRunning ? z.stat.RunSpeed : 1f;
        float final = baseMoveSpeed * SpeedConvertRatio * runMul;

        rb.velocity = dir.normalized * final;
    }
}