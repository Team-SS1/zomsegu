using MonsterEnum;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(WildDog))]
[RequireComponent(typeof(Rigidbody2D))]
public class WildDogMovement : MonoBehaviour
{
    private const float SpeedConvertRatio = 1f / 40f;

    private WildDog dog;
    private Rigidbody2D rb;

    private void Awake()
    {
        dog = GetComponent<WildDog>();
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        //if (TimeManager.Instance != null && TimeManager.Instance.IsStopped(StopType.Monster))
        //{
        //    rb.velocity = Vector2.zero;
        //    return;
        //}

        if (dog.IsDead || dog.IsTakingDamage || dog.IsStunned || dog.IsAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (dog.Knockback != null && dog.Knockback.IsKnockbacking)
            return;

        if (!dog.IsWalking && !dog.IsRunning)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 dir = dog.MoveDirection;
        if (dir.sqrMagnitude < 0.0001f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float moveSpeed = dog.Stat.MoveSpeed * SpeedConvertRatio;
        if (dog.IsRunning)
            moveSpeed *= dog.Stat.RunSpeedMultiplier;

        rb.velocity = dir.normalized * moveSpeed;
    }
}