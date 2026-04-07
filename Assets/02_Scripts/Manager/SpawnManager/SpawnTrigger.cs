using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    private SpawnPoint spawnPoint;

    private void Awake()
    {
        spawnPoint = GetComponent<SpawnPoint>();

        if (!spawnPoint.SpawnData.TriggerSpawn)
            return;

        CircleCollider2D col = GetComponent<CircleCollider2D>();

        if (col == null)
            col = gameObject.AddComponent<CircleCollider2D>();

        col.isTrigger = true;

        if (spawnPoint != null && spawnPoint.SpawnData != null)
        {
            col.radius = spawnPoint.SpawnData.SpawnTriggerRadius;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.GetComponent<Player>())
            return;

        if (spawnPoint == null)
            return;

        SpawnManager.Instance.Spawn(spawnPoint);
        Destroy(this.GetComponent<CircleCollider2D>());
    }

    public void Spawn()
    {
        SpawnManager.Instance.Spawn(spawnPoint);
    }
}