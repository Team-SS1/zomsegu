using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Zombie))]
public class ZombieCombat : MonoBehaviour
{
    private Zombie z;

    private IZombieAttack scratchAttack;
    private IZombieAttack biteAttack;

    private bool hasHitThisAttack;

    // 공격 종료 안전장치
    private float attackTimeoutTimer;
    private const float AttackEndSafetyBuffer = 0.25f;

    private float nextAttackAllowedTime;
    [SerializeField] private float reattackBlockDuration = 0.02f; // 1프레임 정도

    private void Awake()
    {
        z = GetComponent<Zombie>();

        scratchAttack = new ZombieScratchAttack();
        biteAttack = new ZombieBiteAttack();
    }

    private void Update()
    {
        if (!z.IsAttacking)
            return;

        attackTimeoutTimer -= Time.deltaTime;
        if (attackTimeoutTimer <= 0f)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[ZombieCombat] Attack timeout fallback fired: {name}");
#endif
            EndAttack();
        }
    }

    public void StartAttack(AttackType type)
    {
        if (z.IsDead || z.IsAttacking)
            return;

        if (Time.time < nextAttackAllowedTime)
            return;

        z.IsAttacking = true;
        hasHitThisAttack = false;

        z.CurrentAttackType = type;
        z.CurrentAttack = GetAttack(type);

        attackTimeoutTimer = z.CurrentAttack.AttackSpeed + AttackEndSafetyBuffer;

#if UNITY_EDITOR
        Debug.Log($"[ZombieCombat] StartAttack: {type}, timeout={attackTimeoutTimer:F2}");
#endif

    }

    public void OnAttackHit()
    {
#if UNITY_EDITOR
        Debug.Log("[ZombieCombat] OnAttackHit()");
#endif

        if (hasHitThisAttack || z.IsDead) return;
        if (!z.HasValidTarget()) return;
        if (z.CurrentAttack == null) return;

        Vector2 targetPos = z.GetTargetBodyOrigin();
        float range = z.CurrentAttack.AttackRange;

        if (!z.IsInAttackRange2D(targetPos, range))
        {
#if UNITY_EDITOR
            Debug.Log("[ZombieCombat] AttackHit canceled: out of range");
#endif
            return;
        }

        IDamageable damageable = z.Target.GetComponent<IDamageable>();
        if (damageable == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[ZombieCombat] Target has no IDamageable");
#endif
            return;
        }

        hasHitThisAttack = true;

        z.CurrentAttack.Execute(z, damageable);
    }

    public void EndAttack()
    {
#if UNITY_EDITOR
        Debug.Log("[ZombieCombat] EndAttack()");
#endif

        z.IsAttacking = false;
        z.CurrentAttack = null;
        attackTimeoutTimer = 0f;
        hasHitThisAttack = false;

        // Animator가 공격 bool false 상태를 최소 1프레임은 보게 함
        nextAttackAllowedTime = Time.time + reattackBlockDuration;

        // 공격 종료 후 Aggro 상태라면 길찾기 다시 시작 + 지속시간 초기화
        if (!z.IsDead &&
            z.PathAgent != null &&
            z.StateMachine != null &&
            z.StateMachine.CurrentType == ZombieStateType.Aggro &&
            z.HasValidTarget())
        {
            z.PathAgent.ForcePathModeAfterAttack();
        }
    }

    public void ForceCancel()
    {
        z.IsAttacking = false;
        z.CurrentAttack = null;
        attackTimeoutTimer = 0f;
        hasHitThisAttack = false;
        nextAttackAllowedTime = Time.time + reattackBlockDuration;
    }

    private IZombieAttack GetAttack(AttackType type)
        => type == AttackType.Bite ? biteAttack : scratchAttack;

    public float GetAttackRange(AttackType type) => GetAttack(type).AttackRange;

    public AttackType SelectAttackTypeByStat()
    {
        int scratch = z.stat.ScratchChance;
        int bite = z.stat.BiteChance;
        int total = scratch + bite;
        if (total <= 0) return AttackType.Scratch;

        int r = Random.Range(0, total);
        return (r < scratch) ? AttackType.Scratch : AttackType.Bite;
    }
}