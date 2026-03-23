using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ZombieHealth))]
[RequireComponent(typeof(ZombieStateMachine))]
public class Zombie : MonoBehaviour, IDamageable, IPoolable
{
    [Header("Data")]
    public int zombieID;
    public MonsterStat stat;
    public ZombieType zombieType;

    [Header("Runtime")]
    [SerializeField] private Transform target;
    public Transform Target => target;

    public Vector2 MoveDirection { get; set; }
    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    public bool IsWalking { get; set; }
    public bool IsRunning { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsTakingDamage { get; set; }
    public bool IsEating { get; set; }
    public bool IsFakeDie { get; private set; }
    public bool IsWakeUp { get; private set; }

    public bool IsDead => StateMachine.CurrentType == ZombieStateType.Dead;

    public AttackType CurrentAttackType { get; set; }
    public IZombieAttack CurrentAttack { get; set; }

    [Header("Sound Investigate")]
    public Vector2 lastHeardSoundPos;
    public GameObject lastSoundSource;
    public bool hasSoundTarget;

    [Header("Aggro Timers")]
    public float loseAggroOutOfRangeGraceSec = 5f;

    private float _outOfRangeTimer;
    private float _chaseTimer;
    private float _lastSeenTime;
    [SerializeField] private float seenMemorySeconds = 0.2f;

    // ===== 여기 중요 =====
    [Header("References")]
    [SerializeField] private MonsterKnockback knockback;

    public MonsterKnockback Knockback => knockback;
    // ====================

    public ZombieStateMachine StateMachine { get; private set; }
    public ZombieCombat Combat { get; private set; }
    public ZombieMovement Movement { get; private set; }
    public ZombiePathAgent PathAgent { get; private set; }

    private ZombieIdleState _idle;
    private ZombieAggroState _aggro;
    private ZombieInvestigateState _investigate;
    private ZombieDeadState _dead;

    private float _runTimer;
    private float _runCooldownTimer;

    private bool _bootstrapped;

    private void Awake()
    {
        StateMachine = GetComponent<ZombieStateMachine>();
        Combat = GetComponent<ZombieCombat>();
        Movement = GetComponent<ZombieMovement>();
        PathAgent = GetComponent<ZombiePathAgent>();

        // ===== 여기 중요 =====
        if (knockback == null) knockback = GetComponent<MonsterKnockback>();
        // ====================

        InitStat();

        _idle = new ZombieIdleState(this);
        _aggro = new ZombieAggroState(this);
        _investigate = new ZombieInvestigateState(this);
        _dead = new ZombieDeadState(this);
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

    private void BootstrapIfNeeded()
    {
        if (_bootstrapped)
            return;

        _bootstrapped = true;
        OnSpawn();
    }

    private void OnDisable()
    {
        if (WorldEventManager.Instance != null)
            WorldEventManager.Instance.OnSoundEvent -= OnSoundEventReceived;
    }

    // 스폰 시 초기화
    public void OnSpawn()
    {
        target = null;
        MoveDirection = Vector2.zero;
        FacingDirection = Vector2.down;

        IsWalking = false;
        IsRunning = false;
        IsAttacking = false;
        IsTakingDamage = false;
        IsEating = false;
        IsFakeDie = false;
        IsWakeUp = false;

        hasSoundTarget = false;
        lastSoundSource = null;
        lastHeardSoundPos = Vector2.zero;

        _outOfRangeTimer = 0f;
        _chaseTimer = 0f;
        _lastSeenTime = Time.time;

        _runTimer = stat != null ? stat.RunSec : 0f;
        _runCooldownTimer = 0f;

        GetComponent<ZombieHealth>().Init(stat.MaxHP);

        PathAgent?.ResetAll();

        StateMachine.Initialize(_idle, ZombieStateType.Idle);
    }

    // 디스폰 시 초기화
    public void OnDespawn()
    {
        MoveDirection = Vector2.zero;
        IsWalking = false;
        IsRunning = false;
        IsAttacking = false;
        IsTakingDamage = false;

        PathAgent?.ResetAll();
        Combat?.ForceCancel();
    }

    private void Update()
    {
        if (IsDead)
            return;

        if (MoveDirection.sqrMagnitude > 0.001f)
            FacingDirection = MoveDirection.normalized;

        UpdateRunTimer();
    }

    private void InitStat()
    {
        if (!MonsterStat.tableDic.TryGetValue(zombieID, out stat))
        {
#if UNITY_EDITOR
            Debug.LogError($"[Zombie] ID {zombieID} not found in MonsterStat.tableDic");
#endif
            enabled = false;
            return;
        }
    }

    public void MarkTargetSeen()
    {
        _lastSeenTime = Time.time;
    }


    public void OnTargetSeen(Transform seenTarget)
    {
        if (IsDead || seenTarget == null) return;

        if (target != null && target != seenTarget)
        {
            float cur = ((Vector2)target.position - (Vector2)transform.position).sqrMagnitude;
            float nxt = ((Vector2)seenTarget.position - (Vector2)transform.position).sqrMagnitude;

            if (nxt + 0.01f < cur)
                target = seenTarget;
        }
        else
        {
            target = seenTarget;
        }

        _outOfRangeTimer = 0f;
        _lastSeenTime = Time.time;

        if (StateMachine.CurrentType != ZombieStateType.Aggro)
            EnterAggro();
    }

    public void ClearTarget()
    {
        target = null;
        _outOfRangeTimer = 0f;
        _chaseTimer = 0f;
    }

    public bool HasValidTarget()
    {
        return target != null && !IsDead;
    }

    public void EnterIdle()
    {
        ClearTarget();
        StopMove();
        PathAgent?.ResetAll();
        StateMachine.ChangeState(_idle, ZombieStateType.Idle);
    }

    public void EnterAggro()
    {
        if (IsDead) return;
        _chaseTimer = 0f;
        _outOfRangeTimer = 0f;
        PathAgent?.ResetAll();
        StateMachine.ChangeState(_aggro, ZombieStateType.Aggro);
    }

    public void EnterInvestigate(Vector2 pos)
    {
        if (IsDead) return;
        lastHeardSoundPos = pos;
        hasSoundTarget = true;
        PathAgent?.ResetAll();
        StateMachine.ChangeState(_investigate, ZombieStateType.Investigate);
    }

    public void EnterDead()
    {
        StopMove();
        PathAgent?.ResetAll();
        StateMachine.ChangeState(_dead, ZombieStateType.Dead);
    }

    private void OnSoundEventReceived(SoundEvent soundEvent)
    {
        if (IsDead) return;
        if (IsEating) return;
        if (soundEvent.source == null || soundEvent.source == gameObject) return;
        if (soundEvent.radius <= 0f) return;

        if (HasValidTarget())
            return;

        float dist = Vector2.Distance(transform.position, soundEvent.position);
        if (dist > soundEvent.radius)
            return;

        lastHeardSoundPos = soundEvent.position;
        lastSoundSource = soundEvent.source;
        hasSoundTarget = true;

        EnterInvestigate(lastHeardSoundPos);
    }

    public void StopMove()
    {
        IsWalking = false;
        IsRunning = false;
        MoveDirection = Vector2.zero;
    }

    private void UpdateRunTimer()
    {
        if (IsRunning)
        {
            _runTimer -= Time.deltaTime;
            if (_runTimer <= 0f)
            {
                IsRunning = false;
                _runCooldownTimer = 5f;
            }
        }
        else
        {
            if (_runCooldownTimer > 0f)
            {
                _runCooldownTimer -= Time.deltaTime;
                if (_runCooldownTimer <= 0f && stat.RunSec > 0f)
                {
                    _runTimer = stat.RunSec;
                }
            }
        }
    }

    public bool CanRunNow()
    {
        if (stat == null || stat.RunSec <= 0f) return false;
        if (_runCooldownTimer > 0f) return false;
        if (_runTimer <= 0f) return false;
        return true;
    }

    public bool ShouldLoseAggro()
    {
        if (!HasValidTarget())
            return true;

        _chaseTimer += Time.deltaTime;

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > stat.LoseAggroDistance)
        {
            _outOfRangeTimer += Time.deltaTime;
            if (_outOfRangeTimer >= loseAggroOutOfRangeGraceSec)
                return true;
        }
        else
        {
            _outOfRangeTimer = 0f;
        }

        bool recentlySeen = (Time.time - _lastSeenTime) <= seenMemorySeconds;

        if (_chaseTimer >= stat.LoseAggroTimeSec && !recentlySeen)
            return true;

        return false;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, ArmorType.Body);
    }

    public void TakeDamage(float damage, ArmorType hitPart)
    {
        if (IsDead) return;

        if (!IsTakingDamage)
        {
            IsTakingDamage = true;
            StopMove();
            IsAttacking = false;
            Combat?.ForceCancel();
        }

        Vector2 knockDir = HasValidTarget()
            ? ((Vector2)transform.position - (Vector2)target.position).normalized
            : -FacingDirection;

        // 바라보는 방향은 유지하고, 넉백만 적용
        Knockback?.Apply(knockDir);

        float finalDamage = Mathf.Max(0f, damage - stat.Defense);
        GetComponent<ZombieHealth>().ApplyDamage(finalDamage);

        if (!IsDead && PathAgent != null)
        {
            bool wasAggroLike =
                StateMachine.CurrentType == ZombieStateType.Aggro ||
                IsAttacking;

            if (wasAggroLike)
                PathAgent.ForcePathModeAfterHit();
        }

        if (HasValidTarget() && StateMachine.CurrentType != ZombieStateType.Aggro && !IsWakeUp)
            EnterAggro();
    }

    public void TakeDamage(float damage, ArmorType hitPart, Vector2 hitDir)
    {
        if (IsDead)
            return;

        bool wasAggroLike = StateMachine.CurrentType == ZombieStateType.Aggro || IsAttacking;

        if (!IsTakingDamage)
        {
            IsTakingDamage = true;
            StopMove();
            IsAttacking = false;
            Combat?.ForceCancel();
        }

        Vector2 knockDir = hitDir.sqrMagnitude > 0.0001f
            ? hitDir.normalized
            : -FacingDirection;

        // 바라보는 방향은 유지, 넉백만 적용
        Knockback?.Apply(knockDir);

        float finalDamage = Mathf.Max(0f, damage - stat.Defense);
        GetComponent<ZombieHealth>().ApplyDamage(finalDamage);

        // Aggro/공격 중 맞았으면 path mode 강제 실행 + 지속시간 초기화
        if (!IsDead && PathAgent != null && wasAggroLike)
        {
            PathAgent.ForcePathModeAfterHit();
        }

        if (HasValidTarget() && StateMachine.CurrentType != ZombieStateType.Aggro && !IsWakeUp)
            EnterAggro();
    }

    public void EndTakeDamage()
    {
        IsTakingDamage = false;

        if (IsDead)
            return;

        StopMove();

        // 이미 Aggro 상태면 다시 EnterAggro 하지 않는다.
        // 그래야 피격 시 강제로 켠 PathMode가 Reset되지 않음.
        if (StateMachine.CurrentType == ZombieStateType.Aggro)
            return;

        if (HasValidTarget())
            EnterAggro();
        else
            EnterIdle();
    }

    public void Die()
    {
        if (IsDead) return;
        EnterDead();

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        var cols = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = false;
    }

    public Vector2 GetBodyOrigin()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return transform.position;

        float bodyRatio = 0.2f;
        float height = sr.bounds.size.y;
        return (Vector2)transform.position + Vector2.up * height * bodyRatio;
    }

    public Vector2 GetTargetBodyOrigin()
    {
        if (target == null) return Vector2.zero;

        Collider2D col = target.GetComponent<Collider2D>();
        if (col != null)
            return col.bounds.center;

        var sr = target.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            return sr.bounds.center;

        return (Vector2)target.position;
    }

    public bool IsInAttackRange2D(Vector2 targetPos, float range)
    {
        Vector2 my = GetBodyOrigin();
        float dx = Mathf.Abs(my.x - targetPos.x);
        float dy = Mathf.Abs(my.y - targetPos.y);

        float xAllowance = range;
        float yAllowance = range;
        if (FacingDirection.y > -0.5f)
            yAllowance *= 1.8f;

        return dx <= xAllowance && dy <= yAllowance;
    }

    public void ForceFaceDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
            return;

        FacingDirection = dir.normalized;
        MoveDirection = FacingDirection; // 애니메이션 방향 즉시 갱신용
    }

    public Vector2 GetNavigationOrigin()
    {
        // 1순위: BoxCollider2D
        if (TryGetComponent<BoxCollider2D>(out var box) && box.enabled)
            return box.bounds.center;

        // 2순위: CapsuleCollider2D
        if (TryGetComponent<CapsuleCollider2D>(out var capsule) && capsule.enabled)
            return capsule.bounds.center;

        // 3순위: CircleCollider2D
        if (TryGetComponent<CircleCollider2D>(out var circle) && circle.enabled)
            return circle.bounds.center;

        // 4순위: 기존 body origin
        return GetBodyOrigin();
    }
}