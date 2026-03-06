//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;

//public class Monster : MonoBehaviour//, IPoolable
//{
//    [Header("Data")]
//    public int monsterID;
//    //public MonsterStat stat;

//    [Header("Runtime")]
//    //public MonsterStateType currentState = MonsterStateType.Idle;

//    [HideInInspector] public Transform target;

//    //public ZombieType zombieType;

//    public Vector2 MoveDirection { get; set; }
//    public Vector2 FacingDirection { get; set; } = Vector2.down;

//    public bool IsWalking { get; set; }
//    public bool IsRunning { get; set; }
//    public bool IsAttacking { get; set; }
//    public bool IsTakingDamage { get; set; }
//    public bool IsEating { get; set; }

//    //public bool IsDead => currentState == MonsterStateType.Dead;

//    // Attack runtime
//    //public AttackType CurrentAttackType { get; set; }
//    //public IMonsterAttack currentAttack { get; set; }

//    // Cached refs
//    //public MonsterAI AI { get; private set; }
//    //public MonsterCombat Combat { get; private set; }
//    //public ZombieAnimationHandler Animation { get; private set; }

//    //public MonsterSfx Sfx { get; private set; }
//    //public MonsterKnockback Knockback { get; private set; }
//    //public ZombieVisibility Visibility { get; private set; }
//    public MonsterHealth Health { get; private set; }

//    //public MonsterPerception Perception { get; private set; }
//    public MonsterInitialBehavior Initial { get; private set; }
//    public MonsterRunner Runner { get; private set; }
//    public MonsterFacing8Dir Facing8Dir { get; private set; }
//    public MonsterSpatialUtil Spatial { get; private set; }
//    public MonsterDamageReceiver Damage { get; private set; }
//    public MonsterDeath Death { get; private set; }

//    //public bool IsKnockbacking => (Knockback != null) && Knockback.IsKnockbacking;

//    // Initial behavior flags passthrough
//    public bool IsInInitialBehavior => (Initial != null) && Initial.IsInInitialBehavior;
//    public bool IsFakeDie => (Initial != null) && Initial.IsFakeDie;
//    public bool IsWakeUp => (Initial != null) && Initial.IsWakeUp;

//    private void Awake()
//    {
//        // Core refs
//        //AI = GetComponent<MonsterAI>();
//        //Combat = GetComponent<MonsterCombat>();
//        //Animation = GetComponent<ZombieAnimationHandler>();

//        //Sfx = GetComponent<MonsterSfx>();
//        //Knockback = GetComponent<MonsterKnockback>();
//        //Visibility = GetComponent<ZombieVisibility>();
//        Health = GetComponent<MonsterHealth>();

//        // Split modules
//        //Perception = GetComponent<MonsterPerception>();
//        Initial = GetComponent<MonsterInitialBehavior>();
//        Runner = GetComponent<MonsterRunner>();
//        Facing8Dir = GetComponent<MonsterFacing8Dir>();
//        Spatial = GetComponent<MonsterSpatialUtil>();
//        Damage = GetComponent<MonsterDamageReceiver>();
//        Death = GetComponent<MonsterDeath>();

//        //InitMonsterStat();
//    }

//    private void Update()
//    {
//        //if (TimeManager.Instance.IsStopped(StopType.Monster))
//        //    return;

//        //if (IsDead)
//        //    return;

//        // Facing은 모듈이 담당
//        Facing8Dir?.TickUpdateFacingFromMove();

//        // 달리기 타이머도 모듈이 담당
//        Runner?.Tick();

//        if (IsAttacking)
//            return;
//    }

////    private void InitMonsterStat()
////    {
////        if (!MonsterStat.tableDic.TryGetValue(monsterID, out stat))
////        {
////#if UNITY_EDITOR
////            Debug.LogError($"[Monster] ID {monsterID} not found in MonsterStat.tableDic");
////#endif
////            enabled = false;
////            return;
////        }

////        Health?.Init(stat.MaxHP);
////    }

//    public void OnSpawn()
//    {
////        if (AI == null)
////        {
////#if UNITY_EDITOR
////            Debug.LogError($"{name} : AI is NULL on OnSpawn()");
////#endif
////            return;
////        }

////        currentState = MonsterStateType.Idle;

//        // Runtime reset
//        target = null;
//        MoveDirection = Vector2.zero;
//        FacingDirection = Vector2.down;

//        IsWalking = false;
//        IsRunning = false;
//        IsAttacking = false;
//        IsTakingDamage = false;
//        IsEating = false;

//        //currentAttack = null;
//        //CurrentAttackType = AttackType.Scratch;

//        //// Stat re-init
//        //Health?.Init(stat.MaxHP);

//        //// Module reset
//        //Perception?.ResetPerception();
//        Initial?.ResetInitial();
//        Runner?.ResetRunner();
//        Facing8Dir?.ResetFacing();
//        Death?.ResetDeathState();

//        // 초기행동 루프 시작 (필요할 때만)
//        Initial?.StartInitialLoopIfNeeded();

//        // 기본 시작 상태
//        //AI.EnterIdle();
//    }

//    public void OnDespawn()
//    {
//        StopAllCoroutines();
//        //AI?.StopAllStateRoutines();

//        MoveDirection = Vector2.zero;

//        IsWalking = false;
//        IsRunning = false;
//        IsAttacking = false;
//        IsTakingDamage = false;
//        IsEating = false;

//        //currentAttack = null;
//        //CurrentAttackType = AttackType.Scratch;
//    }

//    public void InitializeAfterSpawn()
//    {
//        OnSpawn();
//    }

//    public void SetTarget(Transform newTarget)
//    {
//        target = newTarget;
//    }

//    public void StopMove()
//    {
//        IsWalking = false;
//        IsRunning = false;
//        MoveDirection = Vector2.zero;
//    }

//    /// <summary>
//    /// "한 번이라도 어그로를 탔다" 플래그는 InitialBehavior 모듈이 관리
//    /// </summary>
//    public void MarkEverAggroed()
//    {
//        Initial?.MarkEverAggroed();
//    }

//    //public bool HasValidTarget()
//    //{
//    //    return target != null && !IsDead;
//    //}

//    /// <summary>
//    /// 사망 처리는 Death 모듈로 위임
//    /// </summary>
//    public void Die()
//    {
//        Death?.Die();
//    }
//}