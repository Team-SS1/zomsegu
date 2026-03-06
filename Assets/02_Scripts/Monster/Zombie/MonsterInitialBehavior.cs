//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//public class MonsterInitialBehavior : MonoBehaviour
//{
//    private Monster monster;

//    [Header("첫 배치 전용 행동")]
//    //public InitialBehaviorType initialBehavior = InitialBehaviorType.DefaultIdle;

//    [Header("Patrol")]
//    //public PatrolType patrolType = PatrolType.None;
//    public float patrolSpeed = 1.0f;
//    public float patrolDistance = 2.0f;

//    //[Header("8방향 설정")]
//    //public EightDir guardFacing = EightDir.Down;

//    public bool IsInInitialBehavior { get; private set; }

//    private bool hasEverAggroed = false;
//    private Coroutine initialRoutine;

//    private Vector3 patrolStart;
//    private Vector3 patrolTarget;

//    private float eatingSoundTimer;
//    private const float EatingSoundInterval = 13f;

//    public bool IsFakeDie { get; private set; }
//    public bool IsWakeUp { get; private set; }

//    private void Awake()
//    {
//        monster = GetComponent<Monster>();
//    }

//    public void ResetInitial()
//    {
//        hasEverAggroed = false;

//        IsInInitialBehavior = false;
//        IsFakeDie = false;
//        IsWakeUp = false;

//        patrolStart = Vector3.zero;
//        patrolTarget = Vector3.zero;

//        if (initialRoutine != null)
//        {
//            StopCoroutine(initialRoutine);
//            initialRoutine = null;
//        }
//    }

//    public void MarkEverAggroed()
//    {
//        hasEverAggroed = true;
//    }

//    public void StartInitialLoopIfNeeded()
//    {
//        if (initialRoutine != null)
//        {
//            StopCoroutine(initialRoutine);
//            initialRoutine = null;
//        }

//        // DefaultIdle면 루프 안 돌려도 됨
//        //if (initialBehavior == InitialBehaviorType.DefaultIdle)
//        //{
//        //    IsInInitialBehavior = false;
//        //    return;
//        //}

//        //// Patrol인데 타입 None이면 의미 없음
//        //if (initialBehavior == InitialBehaviorType.Patrol && patrolType == PatrolType.None)
//        //{
//        //    IsInInitialBehavior = false;
//        //    return;
//        //}

//        initialRoutine = StartCoroutine(ExecuteInitialBehaviorLoop());
//    }

//    //private IEnumerator ExecuteInitialBehaviorLoop()
//    //{
//    //    IsInInitialBehavior = true;

//    //    while (monster.currentState == MonsterStateType.Idle && !hasEverAggroed && !monster.IsDead)
//    //    {
//    //        switch (initialBehavior)
//    //        {
//    //            case InitialBehaviorType.Patrol:
//    //                yield return Initial_Patrol();
//    //                break;

//    //            case InitialBehaviorType.GuardLook:
//    //                yield return Initial_GuardLook();
//    //                break;

//    //            case InitialBehaviorType.Eating:
//    //                yield return Initial_Eating();
//    //                break;

//    //            case InitialBehaviorType.FakeDie:
//    //                yield return Initial_FakeDie();
//    //                break;

//    //            default:
//    //                yield return null;
//    //                break;
//    //        }

//    //        yield return null;
//    //    }

//    //    IsInInitialBehavior = false;
//    //    initialRoutine = null;
//    //}

//    //private IEnumerator Initial_Patrol()
//    //{
//    //    if (patrolType == PatrolType.None)
//    //    {
//    //        monster.StopMove();
//    //        yield break;
//    //    }

//    //    if (patrolStart == Vector3.zero)
//    //        patrolStart = transform.position;

//    //    if (patrolTarget == Vector3.zero)
//    //    {
//    //        if (patrolType == PatrolType.Vertical)
//    //            patrolTarget = patrolStart + new Vector3(0, patrolDistance, 0);
//    //        else
//    //            patrolTarget = patrolStart + new Vector3(patrolDistance, 0, 0);
//    //    }

//    //    Vector2 toTarget = (Vector2)(patrolTarget - transform.position);

//    //    if (toTarget.magnitude > 0.1f)
//    //    {
//    //        Vector2 dir = toTarget.normalized;

//    //        monster.MoveDirection = dir;
//    //        monster.FacingDirection = dir;
//    //        monster.IsWalking = true;
//    //        monster.IsRunning = false;
//    //    }
//    //    else
//    //    {
//    //        monster.StopMove();

//    //        if (patrolType == PatrolType.Vertical)
//    //        {
//    //            patrolTarget = (patrolTarget.y > patrolStart.y) ? patrolStart : patrolStart + new Vector3(0, patrolDistance, 0);
//    //        }
//    //        else if (patrolType == PatrolType.Horizontal)
//    //        {
//    //            patrolTarget = (patrolTarget.x > patrolStart.x) ? patrolStart : patrolStart + new Vector3(patrolDistance, 0, 0);
//    //        }
//    //    }

//    //    yield return null;
//    //}

//    //private IEnumerator Initial_GuardLook()
//    //{
//    //    monster.StopMove();
//    //    monster.FacingDirection = MonsterSpatialUtil.EightDirToVector2(guardFacing);
//    //    yield return null;
//    //}

//    //private IEnumerator Initial_Eating()
//    //{
//    //    monster.StopMove();
//    //    monster.FacingDirection = MonsterSpatialUtil.EightDirToVector2(guardFacing);

//    //    monster.IsEating = true;
//    //    eatingSoundTimer = 0f;

//    //    while (monster.currentState == MonsterStateType.Idle && !hasEverAggroed && !monster.IsDead)
//    //    {
//    //        eatingSoundTimer -= Time.deltaTime;

//    //        if (eatingSoundTimer <= 0f)
//    //        {
//    //            monster.Sfx?.Play(ZombieSoundKind.Eating);
//    //            eatingSoundTimer = EatingSoundInterval;
//    //        }

//    //        yield return null;
//    //    }

//    //    monster.IsEating = false;
//    //}

//    //private IEnumerator Initial_FakeDie()
//    //{
//    //    monster.StopMove();
//    //    monster.IsEating = false;

//    //    IsFakeDie = true;
//    //    IsWakeUp = false;

//    //    while (monster.currentState == MonsterStateType.Idle && !hasEverAggroed && !monster.IsDead)
//    //    {
//    //        monster.StopMove();
//    //        yield return null;
//    //    }

//    //    IsFakeDie = false;
//    //}

//    //public void SetInitialBehavior(InitialBehaviorType behavior)
//    //{
//    //    initialBehavior = behavior;
//    //}

//    //public void SetPatrolPoints(Transform[] points)
//    //{
//    //    if (points == null || points.Length < 2)
//    //    {
//    //        patrolType = PatrolType.None;
//    //        return;
//    //    }

//    //    Vector3 start = points[0].position;
//    //    Vector3 end = points[1].position;

//    //    Vector3 diff = end - start;

//    //    patrolDistance = diff.magnitude;

//    //    if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
//    //        patrolType = PatrolType.Horizontal;
//    //    else
//    //        patrolType = PatrolType.Vertical;
//    //}

//    //public bool IsFakeDieActiveAndCanWakeUp()
//    //{
//    //    return initialBehavior == InitialBehaviorType.FakeDie && IsFakeDie && !IsWakeUp;
//    //}

//    public void StartWakeUp()
//    {
//        if (IsWakeUp) return;

//        // 초기행동 루프 강제 종료
//        if (initialRoutine != null)
//        {
//            StopCoroutine(initialRoutine);
//            initialRoutine = null;
//        }

//        IsInInitialBehavior = false;

//        monster.StopMove();
//        monster.IsAttacking = false;
//        monster.IsTakingDamage = false;
//        monster.IsEating = false;

//        IsFakeDie = false;
//        IsWakeUp = true;
//    }

//    //public void OnWakeUpEnd()
//    //{
//    //    if (monster.IsDead) return;
//    //    if (!IsWakeUp) return;

//    //    IsWakeUp = false;

//    //    // 타겟이 있으면 바로 Aggro
//    //    if (monster.target != null && monster.currentState != MonsterStateType.Aggro)
//    //    {
//    //        monster.AI?.OnTargetDetected();
//    //    }
//    //}
//}