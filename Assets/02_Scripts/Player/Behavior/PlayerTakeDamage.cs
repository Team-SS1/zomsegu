using System.Collections;
using System.Collections.Generic;
using MonsterEnum;
using UnityEngine;

public class PlayerTakeDamage : MonoBehaviour, IDamageable
{
    public bool IsDead => throw new System.NotImplementedException();

    public void TakeDamage(float damage, ArmorType hitPart)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float damage, ArmorType hitPart, AttackType attackType)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
