using MonsterEnum;
using UnityEngine;

public class WildDogAggroState : IWildDogState
{
    private readonly WildDog dog;

    private float aggroStartDelay;

    public WildDogAggroState(WildDog dog)
    {
        this.dog = dog;
    }

    public void Enter()
    {
        aggroStartDelay = 0f;
        dog.StopMove();
    }

    public void Tick()
    {
        if (dog.IsDead)
            return;

        if (dog.IsTakingDamage || dog.IsStunned || (dog.Knockback != null && dog.Knockback.IsKnockbacking))
        {
            dog.StopMove();
            return;
        }

        if (aggroStartDelay > 0f)
        {
            aggroStartDelay -= Time.deltaTime;
            dog.StopMove();
            return;
        }

        switch (dog.AggroTargetKind)
        {
            case WildDogAggroTargetKind.Character:
                TickCharacterAggro();
                break;

            case WildDogAggroTargetKind.Sound:
                TickSoundAggro();
                break;

            default:
                dog.EnterIdle(3f);
                break;
        }
    }

    public void Exit()
    {
    }

    private void TickCharacterAggro()
    {
#if UNITY_EDITOR
        Debug.Log($"[WildDogAggro] Target:{dog.CurrentTarget?.name}, Layer:{(dog.CurrentTarget != null ? LayerMask.LayerToName(dog.CurrentTarget.gameObject.layer) : "null")}, CanAggro:{(dog.CurrentTarget != null && dog.CanAggroTarget(dog.CurrentTarget))}");
#endif
#if UNITY_EDITOR
        float dist = dog.CurrentTarget != null
            ? Vector2.Distance(dog.GetNavigationOrigin(), dog.GetTargetCenter(dog.CurrentTarget))
            : -1f;

        Debug.Log($"[WildDogAggro] dist:{dist:F2}, loseDist:{dog.Stat.AggroLoseDistance:F2}, lastSeenAgo:{Time.time - dog.LastSeenTime:F2}");
#endif

        if (!dog.HasValidCharacterTarget())
        {
#if UNITY_EDITOR
            Debug.Log("[WildDogAggro] -> Idle : HasValidCharacterTarget false");
#endif
            dog.EnterIdle(3f);
            return;
        }

        if (!dog.CanAggroTarget(dog.CurrentTarget))
        {
#if UNITY_EDITOR
            Debug.Log("[WildDogAggro] -> Idle : CanAggroTarget false");
#endif
            dog.EnterIdle(3f);
            return;
        }

        if (dog.ShouldLoseCharacterAggro())
        {
#if UNITY_EDITOR
            Debug.Log("[WildDogAggro] -> Idle : ShouldLoseCharacterAggro true");
#endif
            dog.EnterIdle(3f);
            return;
        }
        if (!dog.HasValidCharacterTarget())
        {
            dog.EnterIdle(3f);
            return;
        }

        if (!dog.CanAggroTarget(dog.CurrentTarget))
        {
            dog.EnterIdle(3f);
            return;
        }

        if (dog.ShouldLoseCharacterAggro())
        {
            dog.EnterIdle(3f);
            return;
        }

        Transform target = dog.CurrentTarget;
        Vector2 targetPos = dog.GetTargetCenter(target);
        Vector2 myPos = dog.GetNavigationOrigin();
        Vector2 toTarget = targetPos - myPos;

        dog.MarkSeen();

        // 1. 공격 시작 범위 안이면 공격 시작
        if (dog.Combat.CanStartAttackNow() && dog.IsInAttackStartRange(target))
        {
            dog.StopMove();
            dog.Combat.StartAttack();
            return;
        }

        // 2. 아니면 추격
        dog.IsRunning = true;
        dog.IsWalking = false;

        float directChaseDistance = 2f;
        float surfaceDist = dog.GetDistanceToTargetSurface(target);

        bool usingPath = false;

        if (surfaceDist > directChaseDistance)
            usingPath = dog.PathAgent != null && dog.PathAgent.TickPathToGoal(targetPos);

        if (!usingPath && toTarget.sqrMagnitude > 0.0001f)
            dog.MoveDirection = toTarget.normalized;
    }

    private void TickSoundAggro()
    {
        dog.IsRunning = false;
        dog.IsWalking = true;

        bool usingPath = dog.PathAgent != null && dog.PathAgent.TickPathToGoal(dog.CurrentSoundPos);
        if (!usingPath)
        {
            Vector2 to = dog.CurrentSoundPos - dog.GetNavigationOrigin();
            if (to.sqrMagnitude > 0.0001f)
                dog.MoveDirection = to.normalized;
        }

        if (Vector2.Distance(dog.GetNavigationOrigin(), dog.CurrentSoundPos) <= 1f)
        {
            if (dog.ConsumeReservedSound(out Vector2 reserved))
            {
                dog.EnterAggroSound(reserved);
                return;
            }

            dog.EnterIdle(3f);
        }
    }
}