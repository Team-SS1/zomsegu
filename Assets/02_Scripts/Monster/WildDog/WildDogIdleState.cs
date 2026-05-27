using MonsterEnum;
using UnityEngine;

public class WildDogIdleState : IWildDogState
{
    private readonly WildDog dog;

    private float idleTimer;
    private bool actionChosen;
    private WildDogIdleActionType actionType;
    private float actionTimer;
    private Vector2 actionTarget;
    private bool runAction;


    public WildDogIdleState(WildDog dog)
    {
        this.dog = dog;
    }

    public void Enter()
    {
        dog.StopMove();
        idleTimer = Mathf.Max(0f, dog.IdleRecoverUntil - Time.time);
        actionChosen = false;
        actionTimer = 0f;
        actionTarget = Vector2.zero;
        runAction = false;
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

        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;
            dog.StopMove();
            return;
        }

        if (!actionChosen)
        {
            PickAction();
            return;
        }

        switch (actionType)
        {
            case WildDogIdleActionType.WalkRandom:
            case WildDogIdleActionType.RunRandom:
                TickMoveAction();
                break;

            case WildDogIdleActionType.Howling:
                TickHowling();
                break;
        }
    }

    public void Exit()
    {
    }

    private void PickAction()
    {
        actionChosen = true;

        float r = Random.value;
        if (r < 0.3f)
        {
            actionType = WildDogIdleActionType.WalkRandom;
            runAction = false;
        }
        else if (r < 0.8f)
        {
            actionType = WildDogIdleActionType.RunRandom;
            runAction = true;
        }
        else
        {
            actionType = WildDogIdleActionType.Howling;
            actionTimer = 3f;
            dog.StopMove();
            return;
        }

        actionTimer = 10f;
        actionTarget = FindRandomPoint(6f);

        dog.PathAgent?.ResetAll();

        dog.IsRunning = runAction;
        dog.IsWalking = !runAction;
    }

    private void TickMoveAction()
    {
        actionTimer -= Time.deltaTime;

        bool usingPath = dog.PathAgent != null && dog.PathAgent.TickPathToGoal(actionTarget);
        if (!usingPath)
        {
            Vector2 to = actionTarget - dog.GetNavigationOrigin();
            if (to.sqrMagnitude > 0.0001f)
                dog.MoveDirection = to.normalized;
        }

        if (Vector2.Distance(dog.GetNavigationOrigin(), actionTarget) <= 0.4f || actionTimer <= 0f)
            dog.EnterIdle(3f);
    }

    private void TickHowling()
    {
        dog.StopMove();
        actionTimer -= Time.deltaTime;
        if (actionTimer <= 0f)
            dog.EnterIdle(3f);
    }

    private Vector2 FindRandomPoint(float radius)
    {
        Vector2 origin = dog.GetNavigationOrigin();
        return origin + Random.insideUnitCircle * radius;
    }
}