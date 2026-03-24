using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ZombieIdleState : IZombieState
{
    private readonly Zombie z;

    private float waitTimer;
    private float actionTimer;
    private int actionType; // 0~3
    private Vector2 actionDir;
    private Vector2 actionTarget;
    private Vector2 lookDir;

    public ZombieIdleState(Zombie zombie) { z = zombie; }

    public void Enter()
    {
        z.StopMove();
        waitTimer = Random.Range(3.5f, 4.5f);
        actionTimer = 0f;
        actionType = -1;
    }

    public void Exit()
    {
        z.StopMove();
    }

    public void Tick()
    {
        if (z.IsDead) return;
        if (z.IsAttacking || z.IsTakingDamage) { z.StopMove(); return; }

        // 타겟 생기면 Aggro
        if (z.HasValidTarget())
        {
            z.EnterAggro();
            return;
        }

        // 사운드 타겟 있으면 조사
        if (z.hasSoundTarget)
        {
            z.EnterInvestigate(z.lastHeardSoundPos);
            return;
        }

        // 대기
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            z.StopMove();
            return;
        }

        // 새 액션 선정
        if (actionType < 0)
        {
            int r = Random.Range(0, 100);
            if (r < 40) actionType = 0;        // random move
            else if (r < 70) actionType = 1;   // stay
            else if (r < 90) actionType = 2;   // look
            else actionType = 3;               // forward

            actionTimer = Random.Range(1.5f, 3f);

            if (actionType == 0)
            {
                Vector2 rnd = Random.insideUnitCircle.normalized;
                actionTarget = (Vector2)z.transform.position + rnd * Random.Range(1.5f, 3f);
            }
            else if (actionType == 2)
            {
                float ang = Random.Range(-90f, 90f);
                lookDir = (Vector2)(Quaternion.Euler(0f, 0f, ang) * z.FacingDirection);
                if (lookDir.sqrMagnitude > 0.001f)
                    lookDir.Normalize();
            }
            else if (actionType == 3)
            {
                actionDir = z.FacingDirection.sqrMagnitude > 0.001f ? z.FacingDirection : Vector2.down;
            }
        }

        // 액션 수행
        switch (actionType)
        {
            case 0: // MoveToRandomPoint
                z.IsWalking = true; z.IsRunning = false;

                Vector2 to = actionTarget - (Vector2)z.transform.position;
                if (to.sqrMagnitude < 0.01f)
                {
                    FinishAction();
                    return;
                }
                z.MoveDirection = to.normalized;
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f) FinishAction();
                break;

            case 1: // Stay
                z.StopMove();
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f) FinishAction();
                break;

            case 2: // LookAround
                z.IsWalking = false;
                z.IsRunning = false;
                z.MoveDirection = lookDir;

                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f) FinishAction();
                break;

            case 3: // MoveForward
                z.IsWalking = true; z.IsRunning = false;
                z.MoveDirection = actionDir;
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0f) FinishAction();
                break;
        }
    }

    public void FixedTick() { }

    private void FinishAction()
    {
        z.StopMove();
        actionType = -1;
        waitTimer = Random.Range(3.5f, 4.5f);
    }
}