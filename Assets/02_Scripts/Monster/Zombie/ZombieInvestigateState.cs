using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ZombieInvestigateState : IZombieState
{
    private readonly Zombie z;

    public ZombieInvestigateState(Zombie zombie) { z = zombie; }

    public void Enter()
    {
        z.IsWalking = true;
        z.IsRunning = false;
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

        // 이동 중 플레이어/몬스터가 들어오면 Aggro로 즉시 전환(요구사항)
        if (z.HasValidTarget())
        {
            z.EnterAggro();
            return;
        }

        if (!z.hasSoundTarget)
        {
            z.EnterIdle();
            return;
        }

        if (z.IsAttacking || z.IsTakingDamage || (z.Knockback != null && z.Knockback.IsKnockbacking))
        {
            z.StopMove();
            return;
        }

        // Stuck/Path (Investigate에서도 사용)
        bool usingPath = z.PathAgent != null && z.PathAgent.TickPathAndStuck_Investigate(z.lastHeardSoundPos);

        if (!usingPath)
        {
            Vector2 dir = z.lastHeardSoundPos - z.GetBodyOrigin();
            if (dir.sqrMagnitude < 0.09f) // 0.3^2
            {
                z.hasSoundTarget = false;
                z.EnterIdle();
                return;
            }
            z.MoveDirection = dir.normalized;
        }
    }

    public void FixedTick() { }
}