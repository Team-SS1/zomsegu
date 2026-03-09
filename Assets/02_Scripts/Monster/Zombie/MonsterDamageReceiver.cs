using UnityEngine;

public class MonsterDamageReceiver : MonoBehaviour
{
    private Monster monster;
    private MonsterHealth health;
    private MonsterSpatialUtil spatial;
    private Dotween_ZombieDamageFlash damageFlash;

    private void Awake()
    {
        monster = GetComponent<Monster>();
        health = GetComponent<MonsterHealth>();
        spatial = GetComponent<MonsterSpatialUtil>();
        damageFlash = GetComponent<Dotween_ZombieDamageFlash>();
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, ArmorType.Body);
    }

    public void TakeDamage(float damage, ArmorType hitPart)
    {
        if (monster.IsDead)
            return;

        if (monster.IsTakingDamage)
        {
            monster.Animation?.ResetTakeDamage();
            monster.Knockback?.StopImmediate();
        }
        else
        {
            BeginTakeDamage();
        }

        Vector2 hitDir = spatial != null
            ? spatial.GetHitDirFromAttacker(monster.target)
            : -monster.FacingDirection;

        Vector2 knockDir;
        if (monster.target != null)
            knockDir = ((Vector2)transform.position - (Vector2)monster.target.position).normalized;
        else
            knockDir = hitDir.sqrMagnitude > 0.001f ? hitDir.normalized : -monster.FacingDirection;

        monster.Knockback?.Apply(knockDir);

        float defense = monster.stat != null ? monster.stat.Defense : 0f;
        float finalDamage = Mathf.Max(0f, damage - defense);
        health.ApplyDamage(finalDamage);
    }

    public void TakeDamage(float damage, ArmorType hitPart, Vector2 hitDir)
    {
        if (monster.IsDead)
            return;

        if (monster.IsTakingDamage)
        {
            monster.Animation?.ResetTakeDamage();
            monster.Knockback?.StopImmediate();
        }
        else
        {
            BeginTakeDamage();
        }

        Vector2 knockDir = hitDir.sqrMagnitude > 0.0001f ? hitDir.normalized : -monster.FacingDirection;
        monster.Knockback?.Apply(knockDir);

        float defense = monster.stat != null ? monster.stat.Defense : 0f;
        float finalDamage = Mathf.Max(0f, damage - defense);
        health.ApplyDamage(finalDamage);
    }

    private void BeginTakeDamage()
    {
        monster.Sfx?.Play(ZombieSoundKind.TakeDamage);

        monster.IsTakingDamage = true;
        monster.StopMove();
        monster.IsAttacking = false;

        if (damageFlash != null)
            damageFlash.Play();

        if (monster.target != null && monster.currentState != MonsterStateType.Aggro && !monster.IsWakeUp)
        {
            monster.MarkEverAggroed();
            monster.AI?.OnTargetDetected();
        }
    }

    public void EndTakeDamage()
    {
        monster.IsTakingDamage = false;

        if (damageFlash != null)
            damageFlash.ResetImmediately();

        if (monster.IsDead)
            return;

        monster.StopMove();

        if (monster.target != null)
            monster.AI?.OnTargetDetected();
        else
            monster.AI?.EnterIdle();
    }
}