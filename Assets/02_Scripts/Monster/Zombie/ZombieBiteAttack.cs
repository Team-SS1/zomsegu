using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ZombieBiteAttack : IZombieAttack
{
    public AttackType Type => AttackType.Bite;
    public float AttackRange => 0.25f;
    public float AttackSpeed => 1.5f;

    public void Execute(Zombie zombie, IDamageable target)
    {
        int damage = zombie.stat.AttackDamage + 1;
        ArmorType hitPart = ZombieHitBodyPartUtility.GetRandomPart();

        target.TakeDamage(damage, hitPart);
    }
}