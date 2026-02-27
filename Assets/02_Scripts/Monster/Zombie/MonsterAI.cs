//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MonsterAI : MonoBehaviour
//{
//    private Monster monster;

//    private Coroutine stateRoutine;

//    public readonly float idleWaitMin = 3.5f;
//    public readonly float idleWaitMax = 4.5f;

//    private void Awake()
//    {
//        monster = GetComponent<Monster>();
//    }

//    public void EnterIdle()
//    {
//        if (monster.IsInInitialBehavior)
//            return;

//        if (monster.IsEating)
//            return;

//        if (monster.IsWakeUp)
//            return;

//        //SwitchState(MonsterStateType.Idle, IdleLoop());
//    }

//    //public void EnterAggro()
//    //{
//    //    monster.Sfx?.Play(ZombieSoundKind.Aggro);
//    //    SwitchState(MonsterStateType.Aggro, AggroLoop());
//    //}

//    //private void SwitchState(MonsterStateType newState, IEnumerator routine)
//    //{
//    //    if (monster.IsDead) return;

//    //    if (stateRoutine != null)
//    //        StopCoroutine(stateRoutine);

//    //    monster.currentState = newState;
//    //    stateRoutine = StartCoroutine(routine);
//    //}

//    //private IEnumerator IdleLoop()
//    //{
//    //    while (monster.currentState == MonsterStateType.Idle && !monster.IsDead)
//    //    {
//    //        if (monster.IsInInitialBehavior)
//    //        {
//    //            // 초기행동이 Eating/GuardLook이면 AI Idle 로직에서 이동을 멈추는 게 안전
//    //            if (monster.Initial != null)
//    //            {
//    //                var ib = monster.Initial.initialBehavior;
//    //                if (ib == InitialBehaviorType.Eating || ib == InitialBehaviorType.GuardLook)
//    //                {
//    //                    monster.StopMove();
//    //                }
//    //            }

//    //            yield return null;
//    //            continue;
//    //        }

//    //        monster.StopMove();

//    //        yield return new WaitForSeconds(Random.Range(idleWaitMin, idleWaitMax));

//    //        int r = Random.Range(0, 100);

//    //        if (r < 40) yield return Idle_MoveToRandomPoint();
//    //        else if (r < 70) yield return Idle_Stay();
//    //        else if (r < 90) yield return Idle_LookAround();
//    //        else yield return Idle_MoveForward();
//    //    }
//    //}

//    private IEnumerator Idle_MoveToRandomPoint()
//    {
//        //monster.Sfx?.Play(ZombieSoundKind.MoveIdle);

//        Vector3 idleTargetPos = monster.transform.position +
//            (Vector3)Random.insideUnitCircle.normalized * Random.Range(1.5f, 3f);

//        yield return MoveToTargetForTime(idleTargetPos, 2.5f);
//    }

//    private IEnumerator Idle_Stay()
//    {
//        //monster.Sfx?.Play(ZombieSoundKind.IdleWait);

//        monster.StopMove();
//        float t = Random.Range(1.5f, 3f);

//        while (t > 0f)
//        {
//            //if (monster.currentState != MonsterStateType.Idle) yield break;
//            t -= Time.deltaTime;
//            yield return null;
//        }
//    }

//    private IEnumerator Idle_LookAround()
//    {
//        RotateRandomFacing(90f);
//        yield return new WaitForSeconds(Random.Range(1f, 2f));
//    }

//    private IEnumerator Idle_MoveForward()
//    {
//        //monster.Sfx?.Play(ZombieSoundKind.MoveIdle);
//        yield return MoveForwardForTime(3f);
//    }

//    private IEnumerator MoveToTargetForTime(Vector3 targetPos, float duration)
//    {
//        float timer = duration;
//        monster.IsWalking = true;

//        while (timer > 0f)
//        {
//            //if (monster.currentState != MonsterStateType.Idle) yield break;

//            timer -= Time.deltaTime;

//            Vector3 dir = targetPos - monster.transform.position;
//            if (dir.magnitude < 0.1f) break;

//            monster.MoveDirection = dir.normalized;
//            yield return null;
//        }

//        monster.StopMove();
//    }

//    private IEnumerator MoveForwardForTime(float duration)
//    {
//        float timer = duration;
//        monster.IsWalking = true;

//        if (monster.MoveDirection == Vector2.zero)
//            monster.MoveDirection = monster.FacingDirection;

//        while (timer > 0f)
//        {
//            if (monster.currentState != MonsterStateType.Idle) yield break;
//            timer -= Time.deltaTime;
//            yield return null;
//        }

//        monster.StopMove();
//    }

//    private void RotateRandomFacing(float angleRange)
//    {
//        float angle = Random.Range(-angleRange, angleRange);
//        monster.MoveDirection = (Vector2)(Quaternion.Euler(0, 0, angle) * monster.FacingDirection);
//    }

//    private IEnumerator AggroLoop()
//    {
//        while (monster.currentState == MonsterStateType.Aggro && !monster.IsDead)
//        {
//            if (!monster.HasValidTarget())
//            {
//                EnterIdle();
//                yield break;
//            }

//            // Runner 모듈로 달리기 가능 여부 판단
//            bool canRun = monster.Runner != null && monster.Runner.CanRunNow();
//            if (canRun)
//            {
//                monster.IsRunning = true;
//                monster.IsWalking = false;
//            }
//            else
//            {
//                monster.IsRunning = false;
//                monster.IsWalking = true;
//            }

//            // Spatial 모듈에서 좌표/원점 계산
//            if (monster.Spatial == null)
//            {
//                // Spatial이 없으면 최소 동작만
//                monster.MoveDirection = (monster.target.position - monster.transform.position).normalized;
//                yield return null;
//                continue;
//            }

//            Vector2 targetDir = monster.Spatial.GetTargetBodyOrigin() - monster.Spatial.GetBodyOrigin();

//            // Facing8Dir 모듈로 8방향 스냅
//            if (monster.Facing8Dir != null)
//            {
//                // 기본은 SnapTo8Dir. 원하면 아래처럼 히스테리시스로 교체 가능.
//                if (monster.Facing8Dir.UseAngleHysteresis8Dir)
//                    monster.MoveDirection = monster.Facing8Dir.SnapTo8DirHysteresisByAngleTable(targetDir);
//                else
//                    monster.MoveDirection = monster.Facing8Dir.SnapTo8DirStable(targetDir);
//            }
//            else
//            {
//                monster.MoveDirection = targetDir.normalized;
//            }

//            AttackType type = monster.Combat.SelectAttackTypeByStat();
//            float range = monster.Combat.GetAttackRange(type);

//            if (monster.Spatial.IsInAttackRange2D(monster.Spatial.GetTargetBodyOrigin(), range))
//            {
//                monster.Combat.StartAttack(type);

//                while (monster.IsAttacking)
//                    yield return null;
//            }

//            yield return null;
//        }
//    }

//    public void OnSoundDetected()
//    {
//        if (monster.currentState == MonsterStateType.Aggro)
//            return;

//        SwitchState(MonsterStateType.Investigate, InvestigateLoop());
//    }

//    private IEnumerator InvestigateLoop()
//    {
//        monster.IsWalking = true;
//        monster.IsRunning = false;

//        while (monster.currentState == MonsterStateType.Investigate && !monster.IsDead)
//        {
//            // 이동 중 타겟(플레이어) 발견 시 어그로 전환
//            if (monster.HasValidTarget())
//            {
//                EnterAggro();
//                yield break;
//            }

//            if (monster.Perception == null || monster.Spatial == null)
//            {
//                EnterIdle();
//                yield break;
//            }

//            Vector2 dir = monster.Perception.lastHeardSoundPos - monster.Spatial.GetBodyOrigin();

//            // 소리 위치 도착
//            if (dir.magnitude < 0.3f)
//            {
//                monster.Perception.hasSoundTarget = false;
//                EnterIdle();
//                yield break;
//            }

//            monster.MoveDirection = dir.normalized;
//            yield return null;
//        }
//    }

//    public void OnTargetDetected()
//    {
//        if (monster.IsDead) return;

//        if (monster.IsWakeUp)
//            return;

//        if (monster.currentState != MonsterStateType.Aggro)
//            EnterAggro();
//    }

//    public void OnTargetLost()
//    {
//        if (monster.currentState != MonsterStateType.Idle)
//        {
//            EnterIdle();
//            monster.Perception?.ClearTargetAndReturnIdle();
//        }
//    }

//    public void StopAllStateRoutines()
//    {
//        if (stateRoutine != null)
//        {
//            StopCoroutine(stateRoutine);
//            stateRoutine = null;
//        }
//    }
//}