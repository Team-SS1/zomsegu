using System;
using System.Collections;
using UnityEngine;

public enum MonsterStateType
{
    Idle,
    Aggro,
    Investigate,
    Dead
}

public enum ZombieType
{
    Normal,
    Young,
    Middle,
    Old,
    Special
}

public enum AttackType
{
    Scratch,
    Bite,
    Heavy
}

public enum ZombieSoundKind
{
    Aggro,
    MoveIdle,
    IdleWait,
    Eating,
    TakeDamage,
    Attack
}

public enum InitialBehaviorType
{
    DefaultIdle,
    Patrol,
    GuardLook,
    Eating,
    FakeDie
}

public enum PatrolType
{
    None,
    Horizontal,
    Vertical
}

public enum EightDir
{
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public enum ArmorType
{
    Body,
    Head,
    Arm,
    Leg
}

public enum StopType
{
    Monster
}

public interface IMonsterAttack
{
    void Begin(Monster monster);
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Monster))]
public class MonsterCombat : MonoBehaviour
{
    private Monster monster;
    private Coroutine attackRoutine;

    [Header("Temp Attack Range")]
    [SerializeField] private float defaultScratchRange = 1.0f;
    [SerializeField] private float defaultBiteRange = 0.9f;
    [SerializeField] private float defaultHeavyRange = 1.2f;

    private void Awake()
    {
        monster = GetComponent<Monster>();
    }

    public AttackType SelectAttackTypeByStat()
    {
        if (monster.stat == null)
            return AttackType.Scratch;

        int scratch = monster.stat.ScratchChance;
        int bite = monster.stat.BiteChance;

        int total = scratch + bite;
        if (total <= 0)
            return AttackType.Scratch;

        int roll = UnityEngine.Random.Range(0, total);

        if (roll < scratch)
            return AttackType.Scratch;

        return AttackType.Bite;
    }

    public float GetAttackRange(AttackType type)
    {
        switch (type)
        {
            case AttackType.Bite:
                return defaultBiteRange;
            case AttackType.Heavy:
                return defaultHeavyRange;
            default:
                return defaultScratchRange;
        }
    }

    public void StartAttack(AttackType type)
    {
        if (monster.IsDead || monster.IsAttacking)
            return;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(CoAttack(type));
    }

    private IEnumerator CoAttack(AttackType type)
    {
        monster.IsAttacking = true;
        monster.CurrentAttackType = type;
        monster.StopMove();
        monster.Sfx?.Play(ZombieSoundKind.Attack);

        float attackDelay = 0.35f;
        if (monster.stat != null && monster.stat.AttackSpeed > 0f)
            attackDelay = Mathf.Max(0.1f, 1f / monster.stat.AttackSpeed);

        yield return new WaitForSeconds(attackDelay);

        monster.IsAttacking = false;
        attackRoutine = null;
    }
}

public class ZombieAnimationHandler : MonoBehaviour
{
    public void ResetTakeDamage()
    {
    }
}

[DisallowMultipleComponent]
public class MonsterSfx : MonoBehaviour
{
    public void Play(ZombieSoundKind kind)
    {
#if UNITY_EDITOR
        // Debug.Log($"[MonsterSfx] {name} -> {kind}");
#endif
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Monster))]
public class MonsterKnockback : MonoBehaviour
{
    [SerializeField] private float knockbackDistance = 0.35f;
    [SerializeField] private float knockbackDuration = 0.08f;

    private Monster monster;
    private Rigidbody2D rb;
    private Coroutine knockRoutine;

    public bool IsKnockbacking { get; private set; }

    private void Awake()
    {
        monster = GetComponent<Monster>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Apply(Vector2 dir)
    {
        if (knockRoutine != null)
            StopCoroutine(knockRoutine);

        knockRoutine = StartCoroutine(CoKnockback(dir.normalized));
    }

    public void StopImmediate()
    {
        if (knockRoutine != null)
        {
            StopCoroutine(knockRoutine);
            knockRoutine = null;
        }

        IsKnockbacking = false;

        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    private IEnumerator CoKnockback(Vector2 dir)
    {
        IsKnockbacking = true;

        Vector2 start = transform.position;
        Vector2 end = start + dir * knockbackDistance;
        float t = 0f;

        while (t < knockbackDuration)
        {
            t += Time.deltaTime;
            float lerp = knockbackDuration <= 0f ? 1f : t / knockbackDuration;
            Vector2 next = Vector2.Lerp(start, end, lerp);

            if (rb != null)
                rb.MovePosition(next);
            else
                transform.position = next;

            yield return null;
        }

        IsKnockbacking = false;
        knockRoutine = null;
    }
}

public class ZombieVisibility : MonoBehaviour
{
    public void ForceVisibleNow()
    {
    }
}

public class Dotween_ZombieDamageFlash : MonoBehaviour
{
    public void Play()
    {
    }

    public void ResetImmediately()
    {
    }
}

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public bool IsStopped(StopType stopType)
    {
        return false;
    }
}

[Serializable]
public struct SoundEvent
{
    public Vector2 position;
    public float radius;
    public GameObject source;

    public SoundEvent(Vector2 position, float radius, GameObject source)
    {
        this.position = position;
        this.radius = radius;
        this.source = source;
    }
}

public class WorldEventManager : MonoBehaviour
{
    public static WorldEventManager Instance { get; private set; }

    public event Action<SoundEvent> OnSoundEvent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public void RaiseSoundEvent(SoundEvent soundEvent)
    {
        OnSoundEvent?.Invoke(soundEvent);
    }
}