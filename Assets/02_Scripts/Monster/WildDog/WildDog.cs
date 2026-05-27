using MonsterEnum;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(WildDogStateMachine))]
[RequireComponent(typeof(WildDogHealth))]
[RequireComponent(typeof(WildDogMovement))]
[RequireComponent(typeof(WildDogCombat))]
[RequireComponent(typeof(MonsterKnockback))]
public class WildDog : MonoBehaviour, IDamageable, IPoolable
{
    [Header("Setup")]
    [SerializeField] private WildDogType dogType;

    [Header("Target Layers")]
    [SerializeField] private LayerMask aggroCharacterLayers;
    [SerializeField] private LayerMask attackCharacterLayers;
    [SerializeField] private LayerMask fleeAttackerLayers;

    [Header("Sound")]
    [SerializeField] private float soundHearingMultiplier = 2f;

    [Header("Runtime")]
    [SerializeField] private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    public WildDogStats Stat { get; private set; }

    public Vector2 MoveDirection { get; set; }
    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    public bool IsWalking { get; set; }
    public bool IsRunning { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsTakingDamage { get; set; }
    public bool IsStunned { get; set; }

    public bool IsDead => StateMachine.CurrentType == WildDogStateType.Dead;

    public WildDogAggroTargetKind AggroTargetKind { get; private set; } = WildDogAggroTargetKind.None;

    public Vector2 LastKnownTargetPos { get; private set; }
    public Vector2 CurrentSoundPos { get; private set; }
    public Vector2 ReservedSoundPos { get; private set; }
    public bool HasReservedSound { get; private set; }

    public float LastSeenTime { get; private set; }
    public float LastHitTime { get; private set; }

    public float IdleRecoverUntil { get; private set; }

    public Vector2 FleeDestination { get; private set; }
    public float FleeEndTime { get; private set; }

    public WildDogStateMachine StateMachine { get; private set; }
    public WildDogHealth Health { get; private set; }
    public WildDogMovement Movement { get; private set; }
    public WildDogCombat Combat { get; private set; }
    public WildDogPathAgent PathAgent { get; private set; }
    public MonsterKnockback Knockback { get; private set; }
    public WildDogAnimationHandler AnimationHandler { get; private set; }

    private WildDogIdleState idleState;
    private WildDogMoveState moveState;
    private WildDogAggroState aggroState;
    private WildDogFleeState fleeState;
    private WildDogDeadState deadState;

    private float stunTimer;
    private bool bootstrapped;
    private float outOfRangeTimer;

    private void Awake()
    {
        StateMachine = GetComponent<WildDogStateMachine>();
        Health = GetComponent<WildDogHealth>();
        Movement = GetComponent<WildDogMovement>();
        Combat = GetComponent<WildDogCombat>();
        PathAgent = GetComponent<WildDogPathAgent>();
        Knockback = GetComponent<MonsterKnockback>();
        AnimationHandler = GetComponent<WildDogAnimationHandler>();

        Stat = WildDogHardcodedDatabase.Get(dogType);

        idleState = new WildDogIdleState(this);
        moveState = new WildDogMoveState(this);
        aggroState = new WildDogAggroState(this);
        fleeState = new WildDogFleeState(this);
        deadState = new WildDogDeadState(this);
    }

    private void Start()
    {
        BootstrapIfNeeded();
    }

    private void OnEnable()
    {
        if (WorldEventManager.Instance != null)
            WorldEventManager.Instance.OnSoundEvent += OnSoundEventReceived;

        BootstrapIfNeeded();
    }

    private void OnDisable()
    {
        if (WorldEventManager.Instance != null)
            WorldEventManager.Instance.OnSoundEvent -= OnSoundEventReceived;
    }

    private void BootstrapIfNeeded()
    {
        if (bootstrapped)
            return;

        bootstrapped = true;
        OnSpawn();
    }

    public void OnSpawn()
    {
        currentTarget = null;
        AggroTargetKind = WildDogAggroTargetKind.None;
        LastKnownTargetPos = Vector2.zero;
        CurrentSoundPos = Vector2.zero;
        ReservedSoundPos = Vector2.zero;
        HasReservedSound = false;

        MoveDirection = Vector2.zero;
        FacingDirection = Vector2.down;

        IsWalking = false;
        IsRunning = false;
        IsAttacking = false;
        IsTakingDamage = false;
        IsStunned = false;

        stunTimer = 0f;
        LastSeenTime = Time.time;
        LastHitTime = -999f;
        IdleRecoverUntil = Time.time + 3f;
        FleeDestination = Vector2.zero;
        FleeEndTime = 0f;

        Health.Init(Stat.MaxHP);
        PathAgent?.ResetAll();

        StateMachine.Initialize(idleState, WildDogStateType.Idle);

        outOfRangeTimer = 0f;
    }

    public void OnDespawn()
    {
        MoveDirection = Vector2.zero;
        IsWalking = false;
        IsRunning = false;
        IsAttacking = false;
        IsTakingDamage = false;
        IsStunned = false;
        currentTarget = null;
        AggroTargetKind = WildDogAggroTargetKind.None;
        PathAgent?.ResetAll();
        Combat?.ForceCancel();
    }

    private void Update()
    {
        //if (TimeManager.Instance != null && TimeManager.Instance.IsStopped(StopType.Monster))
        //    return;

        if (IsDead)
            return;

        if (MoveDirection.sqrMagnitude > 0.001f)
            FacingDirection = MoveDirection.normalized;

        if (IsStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                IsStunned = false;
        }
    }

    public void EnterIdle(float recoverDelay = 0f)
    {
        ClearAggroTarget();
        StopMove();
        IdleRecoverUntil = Time.time + recoverDelay;
        StateMachine.ChangeState(idleState, WildDogStateType.Idle);
    }

    public void EnterMove()
    {
        StateMachine.ChangeState(moveState, WildDogStateType.Move);
    }

    public void EnterAggroCharacter(Transform target)
    {
#if UNITY_EDITOR
        Debug.Log($"[WildDog] EnterAggroCharacter before reset pathCount={PathAgent?.DebugPathCount}, pathIndex={PathAgent?.DebugPathIndex}");
#endif
        if (IsDead || target == null)
            return;

        currentTarget = target;
        AggroTargetKind = WildDogAggroTargetKind.Character;
        LastKnownTargetPos = GetTargetCenter(target);
        LastSeenTime = Time.time;
        HasReservedSound = false;

        outOfRangeTimer = 0f;
        PathAgent?.ResetAll();

        StateMachine.ChangeState(aggroState, WildDogStateType.Aggro);
    }

    public void EnterAggroSound(Vector2 soundPos)
    {
        if (IsDead)
            return;

        CurrentSoundPos = soundPos;
        AggroTargetKind = WildDogAggroTargetKind.Sound;

        outOfRangeTimer = 0f;
        PathAgent?.ResetAll();

        StateMachine.ChangeState(aggroState, WildDogStateType.Aggro);
    }

    public void EnterFlee(Vector2 attackerPos, float fleeDistance = 15f, float fleeDuration = 3f)
    {
        Vector2 my = GetNavigationOrigin();
        Vector2 away = my - attackerPos;

        if (away.sqrMagnitude < 0.0001f)
            away = Random.insideUnitCircle.normalized;
        else
            away.Normalize();

        FleeDestination = my + away * fleeDistance;
        FleeEndTime = Time.time + fleeDuration;

        currentTarget = null;
        AggroTargetKind = WildDogAggroTargetKind.None;
        HasReservedSound = false;

        PathAgent?.ResetAll();

        StateMachine.ChangeState(fleeState, WildDogStateType.Flee);
    }

    public void EnterDead()
    {
        StopMove();
        StateMachine.ChangeState(deadState, WildDogStateType.Dead);
    }

    public void ClearAggroTarget()
    {
        currentTarget = null;
        AggroTargetKind = WildDogAggroTargetKind.None;
        CurrentSoundPos = Vector2.zero;
        LastKnownTargetPos = Vector2.zero;
        outOfRangeTimer = 0f;
        PathAgent?.ResetAll();
    }

    public void StopMove()
    {
        IsWalking = false;
        IsRunning = false;
        MoveDirection = Vector2.zero;
    }

    public void MarkSeen()
    {
        LastSeenTime = Time.time;
        if (currentTarget != null)
            LastKnownTargetPos = GetTargetCenter(currentTarget);
    }

    public void NotifySuccessfulHit()
    {
        LastHitTime = Time.time;
        MarkSeen();
    }

    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }

    public bool HasValidCharacterTarget()
    {
        return currentTarget != null && AggroTargetKind == WildDogAggroTargetKind.Character && !IsDead;
    }

    public bool CanAggroTarget(Transform t)
    {
        if (t == null) return false;
        return IsInLayerMask(t.gameObject, aggroCharacterLayers);
    }

    public bool CanAttackTarget(Transform t)
    {
        if (t == null) return false;
        return IsInLayerMask(t.gameObject, attackCharacterLayers);
    }

    public bool ShouldFleeFrom(Transform t)
    {
        if (t == null) return false;
        return IsInLayerMask(t.gameObject, fleeAttackerLayers);
    }

    public Vector2 GetNavigationOrigin()
    {
        // 1순위: CapsuleCollider2D
        if (TryGetComponent<CapsuleCollider2D>(out var capsule) && capsule.enabled)
            return capsule.bounds.center;

        // 2순위: BoxCollider2D
        if (TryGetComponent<BoxCollider2D>(out var box) && box.enabled)
            return box.bounds.center;

        // 3순위: CircleCollider2D
        if (TryGetComponent<CircleCollider2D>(out var circle) && circle.enabled)
            return circle.bounds.center;

        // 4순위: 기존 body origin
        return GetBodyOrigin();
    }

    public Vector2 GetBodyOrigin()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
            return transform.position;

        float h = sr.bounds.size.y;
        return (Vector2)transform.position + Vector2.up * h * 0.25f;
    }

    public Vector2 GetTargetCenter(Transform t)
    {
        if (t == null)
            return Vector2.zero;

        Collider2D col = t.GetComponentInParent<Collider2D>();
        if (col != null)
            return col.bounds.center;

        SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            return sr.bounds.center;

        return t.position;
    }

    public float GetDistanceToTargetSurface(Transform t)
    {
        if (t == null)
            return float.MaxValue;

        Vector2 my = GetNavigationOrigin();

        Collider2D targetCol = t.GetComponentInParent<Collider2D>();
        if (targetCol != null)
        {
            Vector2 closest = targetCol.ClosestPoint(my);
            return Vector2.Distance(my, closest);
        }

        return Vector2.Distance(my, GetTargetCenter(t));
    }

    // 들개 공격 적중 범위
    public bool IsInAttackRange(Transform t)
    {
        return GetDistanceToTargetSurface(t) <= Stat.BiteRange;
    }

    // 들개 공격 시작 범위 (공격 애니메이션이 시작되는 범위, 실제 공격 판정 범위와는 다를 수 있음)
    public bool IsInAttackStartRange(Transform t)
    {
        if (t == null)
            return false;

        return GetDistanceToTargetSurface(t) <= Stat.AttackStartRange;
    }

    public bool ShouldLoseCharacterAggro()
    {
        if (!HasValidCharacterTarget())
            return true;

        float dist = Vector2.Distance(GetNavigationOrigin(), GetTargetCenter(currentTarget));

        if (dist > Stat.AggroLoseDistance)
        {
            outOfRangeTimer += Time.deltaTime;
            if (outOfRangeTimer >= 5f)
                return true;
        }
        else
        {
            outOfRangeTimer = 0f;
        }

        bool recentlySeen = (Time.time - LastSeenTime) <= Stat.AggroLoseTime;
        return !recentlySeen;
    }

    private void OnSoundEventReceived(SoundEvent soundEvent)
    {
        if (IsDead || IsTakingDamage || IsAttacking)
            return;

        if (AggroTargetKind == WildDogAggroTargetKind.Character)
            return;

        float effectiveRadius = soundEvent.radius * soundHearingMultiplier;
        float dist = Vector2.Distance(GetNavigationOrigin(), soundEvent.position);
        if (dist > effectiveRadius)
            return;

        if (AggroTargetKind == WildDogAggroTargetKind.Sound)
        {
            ReservedSoundPos = CurrentSoundPos;
            HasReservedSound = true;
        }

        EnterAggroSound(soundEvent.position);
    }

    public void RememberReservedSound(Vector2 pos)
    {
        ReservedSoundPos = pos;
        HasReservedSound = true;
    }

    public bool ConsumeReservedSound(out Vector2 pos)
    {
        if (!HasReservedSound)
        {
            pos = Vector2.zero;
            return false;
        }

        pos = ReservedSoundPos;
        HasReservedSound = false;
        ReservedSoundPos = Vector2.zero;
        return true;
    }

    public void ApplyStun(float rawStunSeconds)
    {
        float finalStun = Mathf.Max(0f, rawStunSeconds - Stat.StaggerResistance);
        if (finalStun <= 0f)
            return;

        IsStunned = true;
        stunTimer = Mathf.Max(stunTimer, finalStun);
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, ArmorType.Body);
    }

    public void TakeDamage(float damage, ArmorType hitPart)
    {
        TakeDamage(damage, hitPart, Vector2.zero, 0f, null);
    }

    public void TakeDamage(float damage, ArmorType hitPart, AttackType attackType)
    {
        TakeDamage(damage, hitPart);
    }

    public void TakeDamage(float damage, ArmorType hitPart, Vector2 hitDir)
    {
        TakeDamage(damage, hitPart, hitDir, 0f, null);
    }

    public void TakeDamage(float damage, ArmorType hitPart, Vector2 hitDir, float extraStunSeconds)
    {
        TakeDamage(damage, hitPart, hitDir, extraStunSeconds, null);
    }

    public void TakeDamage(float damage, ArmorType hitPart, Vector2 hitDir, float extraStunSeconds, Transform attacker)
    {
        if (IsDead)
            return;

        bool wasAggro = StateMachine.CurrentType == WildDogStateType.Aggro || IsAttacking;

        IsTakingDamage = true;
        IsAttacking = false;
        StopMove();
        Combat?.ForceCancel();

        Vector2 knockDir = hitDir.sqrMagnitude > 0.0001f ? hitDir.normalized : -FacingDirection;
        Knockback?.Apply(knockDir);

        float finalDamage = Mathf.Max(0f, damage - Stat.Defense);
        Health.ApplyDamage(finalDamage);

        ApplyStun(extraStunSeconds);

        if (IsDead)
            return;

        if (attacker != null && ShouldFleeFrom(attacker))
        {
            EnterFlee(attacker.position);
            return;
        }

        if (attacker != null && CanAggroTarget(attacker))
        {
            EnterAggroCharacter(attacker);
            PathAgent?.ForcePathModeAfterHit();
            return;
        }

        if (PathAgent != null && wasAggro)
            PathAgent.ForcePathModeAfterHit();
    }

    public void EndTakeDamage()
    {
        IsTakingDamage = false;

        if (IsDead)
            return;

        StopMove();

        if (StateMachine.CurrentType == WildDogStateType.Aggro ||
            StateMachine.CurrentType == WildDogStateType.Flee)
            return;

        EnterIdle(3f);
    }

    public void Die()
    {
        if (IsDead)
            return;

        EnterDead();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = false;
    }
}