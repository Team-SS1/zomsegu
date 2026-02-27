//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//[DisallowMultipleComponent]
//[RequireComponent(typeof(Monster))]
//public class MonsterRunner : MonoBehaviour
//{
//    private Monster monster;

//    private float runTimer;
//    private float runCooldownTimer;

//    private void Awake()
//    {
//        monster = GetComponent<Monster>();
//    }

//    public void ResetRunner()
//    {
//        if (monster.stat == null)
//        {
//            runTimer = 0f;
//            runCooldownTimer = 0f;
//            return;
//        }

//        runTimer = monster.stat.RunSec;
//        runCooldownTimer = 0f;
//    }

//    public void Tick()
//    {
//        if (monster.IsDead)
//            return;

//        if (monster.IsRunning)
//        {
//            runTimer -= Time.deltaTime;

//            if (runTimer <= 0f)
//            {
//                monster.IsRunning = false;
//                runCooldownTimer = 5f;
//            }
//        }
//        else
//        {
//            if (runCooldownTimer > 0f)
//            {
//                runCooldownTimer -= Time.deltaTime;
//                if (runCooldownTimer <= 0f && monster.stat.RunSec > 0f)
//                {
//                    runTimer = monster.stat.RunSec;
//                }
//            }
//        }
//    }

//    public bool CanRunNow()
//    {
//        if (monster.stat == null || monster.stat.RunSec <= 0f)
//            return false;

//        if (runCooldownTimer > 0f)
//            return false;

//        if (runTimer <= 0f)
//            return false;

//        return true;
//    }
//}