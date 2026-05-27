using MonsterEnum;
using UnityEngine;

[DisallowMultipleComponent]
public class WildDogStateMachine : MonoBehaviour
{
    private IWildDogState current;
    public WildDogStateType CurrentType { get; private set; }

    public void Initialize(IWildDogState state, WildDogStateType type)
    {
        current = state;
        CurrentType = type;
        current?.Enter();
    }

    public void ChangeState(IWildDogState next, WildDogStateType type)
    {
        if (current == next && CurrentType == type)
            return;

        current?.Exit();
        current = next;
        CurrentType = type;
        current?.Enter();
    }

    private void Update()
    {
        current?.Tick();
    }
}