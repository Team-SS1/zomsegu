using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IZombieAttack
{
    AttackType Type { get; }
    float AttackRange { get; }
    float AttackSpeed { get; } // 필요하면 애니 속도 제어에 사용

    void Execute(Zombie zombie, IDamageable target);
}