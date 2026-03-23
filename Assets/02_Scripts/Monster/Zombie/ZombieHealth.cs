using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    private Zombie z;
    private float hp;

    public float CurrentHP => hp;
    public bool IsDead => hp <= 0f;

    private void Awake()
    {
        z = GetComponent<Zombie>();
    }

    public void Init(float maxHP)
    {
        hp = maxHP;
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead) return;

        hp -= amount;
        if (hp <= 0f)
            z.Die();
    }
}