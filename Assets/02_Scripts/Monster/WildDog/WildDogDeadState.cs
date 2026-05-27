public class WildDogDeadState : IWildDogState
{
    private readonly WildDog dog;

    public WildDogDeadState(WildDog dog)
    {
        this.dog = dog;
    }

    public void Enter()
    {
        dog.StopMove();
    }

    public void Tick()
    {
    }

    public void Exit()
    {
    }
}