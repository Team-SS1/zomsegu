using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ZombieDeadState : IZombieState
{
    private readonly Zombie z;
    public ZombieDeadState(Zombie zombie) { z = zombie; }

    public void Enter()
    {
        z.StopMove();
    }

    public void Exit() { }
    public void Tick() { }
    public void FixedTick() { }
}