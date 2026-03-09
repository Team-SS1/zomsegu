using UnityEngine;

public class MonsterDeath : MonoBehaviour
{
    private Monster monster;

    [Header("Death Spawn")]
    [SerializeField] private GameObject deathSpawnPrefab;
    [SerializeField] private Vector3 deathSpawnOffset = Vector3.zero;

    private bool spawnedDeathPrefab = false;

    private void Awake()
    {
        monster = GetComponent<Monster>();
    }

    public void ResetDeathState()
    {
        spawnedDeathPrefab = false;
    }

    public void Die()
    {
        if (monster.IsDead)
            return;

        monster.currentState = MonsterStateType.Dead;
        monster.IsAttacking = false;
        monster.IsTakingDamage = false;
        monster.IsWalking = false;
        monster.IsRunning = false;
        monster.MoveDirection = Vector2.zero;

        if (!spawnedDeathPrefab && deathSpawnPrefab != null)
        {
            spawnedDeathPrefab = true;
            Vector3 spawnPos = transform.position + deathSpawnOffset;
            Instantiate(deathSpawnPrefab, spawnPos, Quaternion.identity);
        }

        monster.Visibility?.ForceVisibleNow();

        StopAllCoroutines();
        monster.AI?.StopAllStateRoutines();

        DisableAllColliders();
        DisablePhysics();
    }

    private void DisableAllColliders()
    {
        var cols = GetComponentsInChildren<Collider2D>();
        foreach (var col in cols)
            col.enabled = false;
    }

    private void DisablePhysics()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
    }
}