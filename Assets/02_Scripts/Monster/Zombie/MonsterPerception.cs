//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//[DisallowMultipleComponent]
//[RequireComponent(typeof(Monster))]
//public class ZombiePerception : MonoBehaviour
//{
//    private Monster monster;

//    [Header("Target Tracking")]
//    private float timeSinceSeen;

//    [Header("Sound Tracking")]
//    public Vector2 lastHeardSoundPos;
//    public GameObject lastSoundSource;
//    public bool hasSoundTarget;

//    private void Awake()
//    {
//        monster = GetComponent<Monster>();
//    }

//    //private void OnEnable()
//    //{
//    //    if (WorldEventManager.Instance != null)
//    //        WorldEventManager.Instance.OnSoundEvent += OnSoundEventReceived;
//    //}

//    //private void OnDisable()
//    //{
//    //    if (WorldEventManager.Instance != null)
//    //        WorldEventManager.Instance.OnSoundEvent -= OnSoundEventReceived;
//    //}

//    //public void ResetPerception()
//    //{
//    //    timeSinceSeen = 0f;

//    //    lastHeardSoundPos = Vector2.zero;
//    //    lastSoundSource = null;
//    //    hasSoundTarget = false;
//    //}

//    ///// <summary>
//    ///// MonsterVision 등에서 "타겟 봄"을 알려줄 때 호출
//    ///// </summary>
//    //public void OnTargetSeen(Transform seenTarget)
//    //{
//    //    if (monster.IsDead) return;

//    //    monster.SetTarget(seenTarget);
//    //    timeSinceSeen = 0f;

//    //    monster.MarkEverAggroed();

//    //    // FakeDie였으면 일단 깨우기부터
//    //    if (monster.Initial != null && monster.Initial.IsFakeDieActiveAndCanWakeUp())
//    //    {
//    //        monster.Initial.StartWakeUp();
//    //        return;
//    //    }

//    //    if (monster.currentState != MonsterStateType.Aggro)
//    //        monster.AI?.OnTargetDetected();
//    //}

//    ///// <summary>
//    ///// MonsterVision 등에서 "타겟 놓침"을 알려줄 때 호출
//    ///// (현재 구현은 "Aggro 상태에서만" LoseAggroTime/LoseAggroDistance 적용)
//    ///// </summary>
//    //public void OnTargetNotSeen()
//    //{
//    //    if (monster.IsDead)
//    //        return;

//    //    if (monster.currentState != MonsterStateType.Aggro)
//    //        return;

//    //    timeSinceSeen += Time.deltaTime;

//    //    if (monster.target != null)
//    //    {
//    //        float dist = Vector2.Distance(transform.position, monster.target.position);
//    //        if (dist > monster.stat.LoseAggroDistance)
//    //        {
//    //            monster.AI?.OnTargetLost();
//    //            return;
//    //        }
//    //    }

//    //    if (timeSinceSeen >= monster.stat.LoseAggroTimeSec)
//    //    {
//    //        monster.AI?.OnTargetLost();
//    //    }
//    //}

//    public void ClearTargetAndReturnIdle()
//    {
//        monster.SetTarget(null);
//        timeSinceSeen = 0f;
//        monster.StopMove();
//    }

////    private void OnSoundEventReceived(SoundEvent soundEvent)
////    {
////        if (monster.IsDead)
////            return;

////        if (monster.IsEating)
////            return;

////        if (soundEvent.source == null || soundEvent.source == gameObject)
////            return;

////        if (soundEvent.radius <= 0f)
////            return;

////        // 이미 어그로면 무시
////        if (monster.currentState == MonsterStateType.Aggro)
////            return;

////        if (monster.IsWakeUp || monster.IsFakeDie)
////            return;

////        float dist = Vector2.Distance(transform.position, soundEvent.position);
////        if (dist > soundEvent.radius)
////            return;

////#if UNITY_EDITOR
////        Debug.Log($"[SoundDetect] Zombie:{name} dist:{dist:F2} / radius:{soundEvent.radius} source:{soundEvent.source.name}");
////#endif

////        lastHeardSoundPos = soundEvent.position;
////        lastSoundSource = soundEvent.source;
////        hasSoundTarget = true;

////        monster.AI?.OnSoundDetected();
////    }
//}