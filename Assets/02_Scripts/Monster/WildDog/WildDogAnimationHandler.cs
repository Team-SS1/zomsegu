using MonsterEnum;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(WildDog))]
public class WildDogAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private WildDog dog;
    private Vector2 lastDir = Vector2.down;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        dog = GetComponent<WildDog>();
    }

    private void Update()
    {
        UpdateDirection();
        UpdateAnimatorState();
        UpdateAnimatorSpeed();
    }

    private void UpdateDirection()
    {
        Vector2 dir = dog.MoveDirection.sqrMagnitude > 0.001f ? dog.MoveDirection : dog.FacingDirection;
        if (dir.sqrMagnitude > 0.001f)
            lastDir = dir.normalized;

        animator.SetFloat("MoveX", lastDir.x);
        animator.SetFloat("MoveY", lastDir.y);
    }

    private void UpdateAnimatorState()
    {
        animator.SetBool("IsDie", dog.IsDead);
        animator.SetBool("IsTakeDamage", dog.IsTakingDamage);
        animator.SetBool("IsWalk", dog.IsWalking);
        animator.SetBool("IsRun", dog.IsRunning);
        animator.SetBool("IsBite", dog.IsAttacking);
        //animator.SetBool("IsHowling", dog.StateMachine.CurrentType == WildDogStateType.Idle && !dog.IsWalking && !dog.IsRunning && !dog.IsAttacking);
    }

    private void UpdateAnimatorSpeed()
    {
        if (dog.IsAttacking)
            animator.speed = 1f / Mathf.Max(0.01f, dog.Stat.AttackDuration);
        else
            animator.speed = 1f;
    }

    public void OnAttackHit()
    {
        dog.Combat.OnAttackHit();
    }

    public void OnAttackEnd()
    {
        dog.Combat.EndAttack();
    }

    public void OnTakeDamageEnd()
    {
        dog.EndTakeDamage();
    }
}