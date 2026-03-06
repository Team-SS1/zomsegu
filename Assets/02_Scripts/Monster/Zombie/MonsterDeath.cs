//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//public class MonsterDeath : MonoBehaviour
//{
//    private Monster monster;
//    //private ZombieVisibility visibility;

//    [Header("Death Spawn")]
//    [SerializeField] private GameObject deathSpawnPrefab;
//    [SerializeField] private Vector3 deathSpawnOffset = Vector3.zero;

//    private bool spawnedDeathPrefab = false;

//    private void Awake()
//    {
//        monster = GetComponent<Monster>();
//        //visibility = GetComponent<ZombieVisibility>();
//    }

//    public void ResetDeathState()
//    {
//        spawnedDeathPrefab = false;
//    }

//    public void Die()
//    {
//#if UNITY_EDITOR
//        Debug.Log($"[Die] {name} Die() called");
//#endif
//        //if (monster.IsDead) return;

//        //monster.currentState = MonsterStateType.Dead;

//        if (!spawnedDeathPrefab && deathSpawnPrefab != null)
//        {
//            spawnedDeathPrefab = true;

//            Vector3 spawnPos = transform.position + deathSpawnOffset;
//            Instantiate(deathSpawnPrefab, spawnPos, Quaternion.identity);
//        }

//        //var darkener = GetComponent<OutdoorSpriteDarkener>();
//        //if (darkener != null)
//        //{
//        //    darkener.MarkAsDead();
//        //}

//        //if (visibility == null) visibility = GetComponent<ZombieVisibility>();
//        //visibility?.ForceVisibleNow();

//        StopAllCoroutines();
//        //monster.AI?.StopAllStateRoutines();

//        DisableAllColliders();
//        DisablePhysics();
//    }

//    private void DisableAllColliders()
//    {
//        var cols = GetComponentsInChildren<Collider2D>();
//        foreach (var col in cols)
//            col.enabled = false;
//    }

//    private void DisablePhysics()
//    {
//        var rb = GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.velocity = Vector2.zero;
//            rb.simulated = false;
//        }
//    }
//}