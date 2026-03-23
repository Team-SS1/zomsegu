using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ZombieStateMachine : MonoBehaviour
{
    private IZombieState _current;
    private ZombieStateType _currentType;

    public ZombieStateType CurrentType => _currentType;

    public void Initialize(IZombieState startState, ZombieStateType type)
    {
        _current = startState;
        _currentType = type;
        _current?.Enter();
    }

    public void ChangeState(IZombieState next, ZombieStateType type)
    {
        if (_currentType == type && _current == next)
            return;

        _current?.Exit();
        _current = next;
        _currentType = type;
        _current?.Enter();
    }

    private void Update()
    {
        _current?.Tick();
    }

    private void FixedUpdate()
    {
        _current?.FixedTick();
    }
}