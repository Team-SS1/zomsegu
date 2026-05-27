using UnityEngine;

public class WildDogFleeState : IWildDogState
{
    private readonly WildDog dog;

    public WildDogFleeState(WildDog dog)
    {
        this.dog = dog;
    }

    public void Enter()
    {
        dog.IsRunning = true;
        dog.IsWalking = false;
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

        dog.IsRunning = true;
        dog.IsWalking = false;

        bool usingPath = dog.PathAgent != null && dog.PathAgent.TickPathToGoal(dog.FleeDestination);
        if (!usingPath)
        {
            Vector2 to = dog.FleeDestination - dog.GetNavigationOrigin();
            if (to.sqrMagnitude > 0.0001f)
                dog.MoveDirection = to.normalized;
        }

        if (Time.time >= dog.FleeEndTime || Vector2.Distance(dog.GetNavigationOrigin(), dog.FleeDestination) <= 0.5f)
            dog.EnterIdle(3f);
    }

    public void Exit()
    {
    }
}