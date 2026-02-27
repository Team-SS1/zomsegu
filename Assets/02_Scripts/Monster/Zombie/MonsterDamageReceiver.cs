//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//public class MonsterDamageReceiver : MonoBehaviour//, IDamageable
//{
//    private Monster monster;
//    private MonsterHealth health;
//    private MonsterSpatialUtil spatial;

//    //private Dotween_ZombieDamageFlash damageFlash;

//    private void Awake()
//    {
//        monster = GetComponent<Monster>();
//        health = GetComponent<MonsterHealth>();
//        spatial = GetComponent<MonsterSpatialUtil>();
//        //damageFlash = GetComponent<Dotween_ZombieDamageFlash>();
//    }

//    /*
//    public void TakeDamage(float damage)
//    {
//        TakeDamage(damage, ArmorType.Body);
//    }

//    public void TakeDamage(float damage, ArmorType hitPart)
//    {
//        if (monster.IsDead)
//            return;

//        // 연속 피격이면 애니 리셋 + 넉백 즉시 중단
//        if (monster.IsTakingDamage)
//        {
//            monster.Animation?.ResetTakeDamage();
//            monster.Knockback?.StopImmediate();
//        }
//        else
//        {
//            BeginTakeDamage();
//        }

//        Vector2 hitDir = spatial != null ? spatial.GetHitDirFromAttacker(monster.target) : (-monster.FacingDirection);

//        Vector2 knockDir;
//        if (monster.target != null)
//            knockDir = ((Vector2)transform.position - (Vector2)monster.target.position).normalized;
//        else
//            knockDir = hitDir.sqrMagnitude > 0.001f ? hitDir.normalized : -monster.FacingDirection;

//        monster.Knockback?.Apply(knockDir);

//        float finalDamage = Mathf.Max(0f, damage - monster.stat.Defense);
//        health.ApplyDamage(finalDamage);
//    }

//    public void TakeDamage(float damage, ArmorType hitPart, Vector2 hitDir)
//    {
//        if (monster.IsDead)
//            return;

//        if (monster.IsTakingDamage)
//        {
//            monster.Animation?.ResetTakeDamage();
//            monster.Knockback?.StopImmediate();
//        }
//        else
//        {
//            BeginTakeDamage();
//        }

//        Vector2 knockDir = hitDir.sqrMagnitude > 0.0001f ? hitDir.normalized : -monster.FacingDirection;
//        monster.Knockback?.Apply(knockDir);

//        float finalDamage = Mathf.Max(0f, damage - monster.stat.Defense);
//        health.ApplyDamage(finalDamage);
//    }

//    private void BeginTakeDamage()
//    {
//        monster.Sfx?.Play(ZombieSoundKind.TakeDamage);

//        monster.IsTakingDamage = true;

//        monster.StopMove();
//        monster.IsAttacking = false;

//        if (damageFlash != null)
//            damageFlash.Play();

//        // 피격으로 어그로 전환
//        if (monster.target != null && monster.currentState != MonsterStateType.Aggro && !monster.IsWakeUp)
//        {
//            monster.MarkEverAggroed();
//            monster.AI?.OnTargetDetected();
//        }
//    }

//    public void EndTakeDamage()
//    {
//        monster.IsTakingDamage = false;

//        if (damageFlash != null)
//            damageFlash.ResetImmediately();

//        if (monster.IsDead)
//            return;

//        monster.StopMove();

//        if (monster.target != null)
//            monster.AI?.OnTargetDetected();
//        else
//            monster.AI?.EnterIdle();
//    }
//    */
//}