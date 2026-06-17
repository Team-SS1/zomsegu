using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerAnimationHandler : MonoBehaviour
{
    // ------------------------- Field --------------------------
    private PlayerHub hub;
    private Animator[] animators;

    private static readonly int DeltaX = Animator.StringToHash("DeltaX");
    private static readonly int DeltaY = Animator.StringToHash("DeltaY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");
    private static readonly int DieTrigger = Animator.StringToHash("DieTrigger");
    private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");

    private bool isMoving;
    private bool isRunning;

    private void Awake()
    {
        hub = GetComponentInParent<PlayerHub>();
        animators = GetComponentsInChildren<Animator>();
    }
    // ------------------------- Method -------------------------
    private void Update()
    {
        if (hub.Attack == null || animators == null)
            return;

        UpdateDirection();

        UpdateMoveState();
    }
    private void UpdateDirection() // Direction
    {
        Vector2 dir;
        if (hub.Attack.IsAttacking)
        {
            dir = hub.Attack.AttackDirection; // Direction Fix
        }
        else
        {
            dir = hub.Aim.Mousedir.GetLocalMouseDirection(transform.position);
        }
        foreach (var animator in animators)
        {
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                continue;

            animator.SetFloat(DeltaX, dir.x);
            animator.SetFloat(DeltaY, dir.y);
        }
    }
    private void UpdateMoveState() // Move Animation Start
    {
        if (hub.Attack.IsAttacking)
            return;

        Vector2 move = hub.Controller.InputVec;

        isMoving = move.sqrMagnitude > 0.01f;
        isRunning = isRunning = isMoving && hub.Controller.IsRunning;

        foreach (var animator in animators)
        {
            if (animator == null /*|| !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null*/)
                continue;

            animator.SetBool(IsMoving, isMoving);
            animator.SetBool(IsRunning, isRunning);
        }
    }
    public void PlayAttack() // Attack Animation Start
    {
        var animator = hub.SpriteController.CurrentAnimator;

        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
            return;

        float attackSpeed = hub.Player.CurrentAttackSpeed;

        animator.SetFloat(AttackSpeed, attackSpeed);
        animator.SetTrigger(AttackTrigger);
    }
    public void PlayDie()
    {
        foreach (var animator in animators)
        {
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                continue;

            animator.SetTrigger(DieTrigger);
        }
    }
}

