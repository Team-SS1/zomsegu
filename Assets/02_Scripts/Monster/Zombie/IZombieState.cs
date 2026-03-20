using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IZombieState
{
    void Enter();
    void Exit();
    void Tick();       // Update
    void FixedTick();  // FixedUpdate (필요 시)
}