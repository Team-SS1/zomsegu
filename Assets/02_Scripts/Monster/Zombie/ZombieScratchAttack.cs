using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ZombieScratchAttack : IZombieAttack
{
    public AttackType Type => AttackType.Scratch;
    public float AttackRange => 0.5f;
    public float AttackSpeed => 1f;

    public void Execute(Zombie zombie, IDamageable target)
    {
        int damage = zombie.stat.AttackDamage;
        ArmorType hitPart = ZombieHitBodyPartUtility.GetRandomPart();

        target.TakeDamage(damage, hitPart);
    }
}