using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ZombieAggroState : IZombieState
{
    private readonly Zombie z;

    public ZombieAggroState(Zombie zombie) { z = zombie; }

    public void Enter()
    {
        // 타겟이 없다면 Idle
        if (!z.HasValidTarget())
        {
            z.EnterIdle();
            return;
        }

        z.PathAgent?.ResetAll();
    }

    public void Exit()
    {
        z.StopMove();
        z.PathAgent?.ResetAll();
    }

    public void Tick()
    {
        if (z.IsDead) return;

        if (!z.HasValidTarget() && !z.CanAttackTarget(z.Target))
        {
            z.EnterIdle();
            return;
        }

        if (z.IsAttacking || z.IsTakingDamage || (z.Knockback != null && z.Knockback.IsKnockbacking))
        {
            z.StopMove();
            // 피격/넉백 중에는 PathMode를 지우지 않는다.
            // 넉백 끝난 뒤 PathAgent가 이어서 길찾기를 수행해야 함.
            return;
        }

        if (z.ShouldLoseAggro())
        {
            z.EnterIdle();
            return;
        }

        AttackType type = z.Combat.SelectAttackTypeByStat();
        float range = z.Combat.GetAttackRange(type);

        Vector2 targetPos = z.GetTargetBodyOrigin();
        Vector2 myPos = z.GetNavigationOrigin();
        Vector2 toTarget = targetPos - myPos;

        // 1. 공격 가능하면 공격 우선
        if (z.IsInAttackRange2D(targetPos, range))
        {
            z.PathAgent?.ResetAll();
            z.StopMove();
            z.Combat.StartAttack(type);
            return;
        }

        // 2. 달리기/걷기
        if (z.CanRunNow())
        {
            z.IsRunning = true;
            z.IsWalking = false;
        }
        else
        {
            z.IsRunning = false;
            z.IsWalking = true;
        }

        // 3. Path / Stuck 처리
        bool usingPath = z.PathAgent != null && z.PathAgent.TickPathAndStuck_Aggro();

        // 중요:
        // path 모드면 절대 직선 추격으로 덮어쓰지 않는다.
        if (usingPath)
            return;

        // 4. path가 아닐 때만 직선 추격
        if (toTarget.sqrMagnitude > 0.0001f)
            z.MoveDirection = toTarget.normalized;
    }

    public void FixedTick() { }
}