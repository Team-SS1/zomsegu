using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("Data")]
    public int monsterID;
    public MonsterStat stat;

    [Header("Runtime")]
    public MonsterStateType currentState = MonsterStateType.Idle;

    [HideInInspector] public Transform target;

    public ZombieType zombieType;

    public Vector2 MoveDirection { get; set; }
    public Vector2 FacingDirection { get; set; } = Vector2.down;

    public bool IsWalking { get; set; }
    public bool IsRunning { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsTakingDamage { get; set; }
    public bool IsEating { get; set; }

    public bool IsDead => currentState == MonsterStateType.Dead;

    // Attack runtime
    public AttackType CurrentAttackType { get; set; }
    public IMonsterAttack currentAttack { get; set; }

    // Cached refs
    public MonsterAI AI { get; private set; }
    public MonsterCombat Combat { get; private set; }
    public ZombieAnimationHandler Animation { get; private set; }

    public MonsterSfx Sfx { get; private set; }
    public MonsterKnockback Knockback { get; private set; }
    public ZombieVisibility Visibility { get; private set; }
    public MonsterHealth Health { get; private set; }

    public MonsterPerception Perception { get; private set; }
    public MonsterInitialBehavior Initial { get; private set; }
    public MonsterRunner Runner { get; private set; }
    public MonsterFacing8Dir Facing8Dir { get; private set; }
    public MonsterSpatialUtil Spatial { get; private set; }
    public MonsterDamageReceiver Damage { get; private set; }
    public MonsterDeath Death { get; private set; }

    public bool IsKnockbacking => (Knockback != null) && Knockback.IsKnockbacking;

    // Initial behavior flags passthrough
    public bool IsInInitialBehavior => (Initial != null) && Initial.IsInInitialBehavior;
    public bool IsFakeDie => (Initial != null) && Initial.IsFakeDie;
    public bool IsWakeUp => (Initial != null) && Initial.IsWakeUp;

    private void Awake()
    {
        AI = GetComponent<MonsterAI>();
        Combat = GetComponent<MonsterCombat>();
        Animation = GetComponent<ZombieAnimationHandler>();

        Sfx = GetComponent<MonsterSfx>();
        Knockback = GetComponent<MonsterKnockback>();
        Visibility = GetComponent<ZombieVisibility>();
        Health = GetComponent<MonsterHealth>();

        Perception = GetComponent<MonsterPerception>();
        Initial = GetComponent<MonsterInitialBehavior>();
        Runner = GetComponent<MonsterRunner>();
        Facing8Dir = GetComponent<MonsterFacing8Dir>();
        Spatial = GetComponent<MonsterSpatialUtil>();
        Damage = GetComponent<MonsterDamageReceiver>();
        Death = GetComponent<MonsterDeath>();

        InitMonsterStat();
    }

    private void Update()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsStopped(StopType.Monster))
            return;

        if (IsDead)
            return;

        Facing8Dir?.TickUpdateFacingFromMove();
        Runner?.Tick();

        if (IsAttacking)
            return;
    }

    private void InitMonsterStat()
    {
        if (MonsterStat.tableDic != null && MonsterStat.tableDic.TryGetValue(monsterID, out MonsterStat foundStat))
        {
            stat = foundStat;
        }
        else if (stat == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[Monster] ID {monsterID} not found in MonsterStat.tableDic and stat is null.");
#endif
        }

        if (stat != null)
            Health?.Init(stat.MaxHP);
    }

    public void OnSpawn()
    {
        if (AI == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name} : AI is NULL on OnSpawn()");
#endif
            return;
        }

        currentState = MonsterStateType.Idle;

        // Runtime reset
        target = null;
        MoveDirection = Vector2.zero;
        FacingDirection = Vector2.down;

        IsWalking = false;
        IsRunning = false;
        IsAttacking = false;
        IsTakingDamage = false;
        IsEating = false;

        currentAttack = null;
        CurrentAttackType = AttackType.Scratch;

        // Stat re-init
        if (stat != null)
            Health?.Init(stat.MaxHP);

        // Module reset
        Perception?.ResetPerception();
        Initial?.ResetInitial();
        Runner?.ResetRunner();
        Facing8Dir?.ResetFacing();
        Death?.ResetDeathState();

        Initial?.StartInitialLoopIfNeeded();
        AI.EnterIdle();
    }

    public void OnDespawn()
    {
        StopAllCoroutines();
        AI?.StopAllStateRoutines();

        MoveDirection = Vector2.zero;

        IsWalking = false;
        IsRunning = false;
        IsAttacking = false;
        IsTakingDamage = false;
        IsEating = false;

        currentAttack = null;
        CurrentAttackType = AttackType.Scratch;
    }

    public void InitializeAfterSpawn()
    {
        OnSpawn();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void StopMove()
    {
        IsWalking = false;
        IsRunning = false;
        MoveDirection = Vector2.zero;
    }

    public void MarkEverAggroed()
    {
        Initial?.MarkEverAggroed();
    }

    public bool HasValidTarget()
    {
        return target != null && !IsDead;
    }

    public void Die()
    {
        Death?.Die();
    }
}