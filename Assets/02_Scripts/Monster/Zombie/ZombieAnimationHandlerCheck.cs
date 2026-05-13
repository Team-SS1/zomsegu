using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Zombie))]
public class ZombieAnimationHandlerCheck : MonoBehaviour
{
    private Animator animator;
    private Zombie zombie;

    private Vector2 lastDir = Vector2.down;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        zombie = GetComponent<Zombie>();
    }

    private void Update()
    {
        if (zombie == null || animator == null)
            return;

        UpdateDirection();
        UpdateAnimatorState();
        UpdateAnimatorSpeed();
    }

    private void UpdateDirection()
    {
        Vector2 dir;

        if (zombie.IsAttacking || zombie.IsTakingDamage)
        {
            // 공격/피격 중에는 바라보는 방향 유지
            dir = zombie.FacingDirection;
        }
        else
        {
            // 이동 중이면 MoveDirection, 아니면 FacingDirection
            dir = zombie.MoveDirection.sqrMagnitude > 0.001f
                ? zombie.MoveDirection
                : zombie.FacingDirection;
        }

        if (dir.sqrMagnitude > 0.001f)
            lastDir = dir.normalized;

        animator.SetFloat("MoveX", lastDir.x);
        animator.SetFloat("MoveY", lastDir.y);
    }

    private void UpdateAnimatorState()
    {
        bool isDie = zombie.IsDead;
        bool isTakeDamage = !isDie && zombie.IsTakingDamage;
        bool isAttack = !isDie && !isTakeDamage && zombie.IsAttacking;

        bool isRun = !isDie && !isTakeDamage && !isAttack && zombie.IsRunning;
        bool isWalk = !isDie && !isTakeDamage && !isAttack && !isRun && zombie.IsWalking;

        bool isScratch = isAttack && zombie.CurrentAttackType == AttackType.Scratch;
        bool isBite = isAttack && zombie.CurrentAttackType == AttackType.Bite;

        bool isEat = !isDie && !isTakeDamage && !isAttack && zombie.IsEating;
        bool isWakeUp = !isDie && zombie.IsWakeUp;
        bool isFakeDie = !isDie && !isWakeUp && zombie.IsFakeDie;

        animator.SetBool("IsDie", isDie);
        animator.SetBool("IsTakeDamage", isTakeDamage);

        animator.SetBool("IsScratch", isScratch);
        animator.SetBool("IsBite", isBite);

        animator.SetBool("IsRun", isRun);
        animator.SetBool("IsWalk", isWalk);

        animator.SetBool("IsEat", isEat);
        animator.SetBool("IsFakeDie", isFakeDie);
        animator.SetBool("IsWakeUp", isWakeUp);
    }

    private void UpdateAnimatorSpeed()
    {
        if (zombie == null || zombie.stat == null)
        {
            animator.speed = 1f;
            return;
        }

        if (zombie.IsAttacking)
            animator.speed = 1f / Mathf.Max(0.01f, zombie.stat.AttackSpeed);
        else
            animator.speed = 1f;
    }

    // ===== Animation Event =====

    // 피격 애니 끝
    public void OnTakeDamageEnd()
    {
        zombie.EndTakeDamage();
    }

    // 공격 애니의 타격 프레임
    public void OnAttackHit()
    {
        Debug.Log("[ZombieAnimationHandler] OnAttackHit Event");
        zombie.Combat.OnAttackHit();
    }

    // 공격 애니 끝
    public void OnAttackEnd()
    {
        Debug.Log("[ZombieAnimationHandler] OnAttackEnd Event");
        zombie.Combat.EndAttack();
    }

    // 피격 애니 재트리거가 필요할 때
    public void ResetTakeDamage()
    {
        animator.ResetTrigger("TakeDamage");
        animator.SetTrigger("TakeDamage");
    }
}