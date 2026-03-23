using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ZombieAggroZoneCheck : MonoBehaviour
{
    public float radius = 3f;

    private Zombie z;
    private CircleCollider2D col;

    private void Awake()
    {
        z = GetComponentInParent<Zombie>();
        col = GetComponent<CircleCollider2D>();

        col.isTrigger = true;
        col.radius = radius;
    }

    private void OnValidate()
    {
        if (col == null) col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (z == null || z.IsDead)
            return;

        Transform targetRoot = ResolveTargetRoot(other);
        if (targetRoot == null)
            return;

        z.OnTargetSeen(targetRoot);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (z == null || z.IsDead)
            return;

        Transform targetRoot = ResolveTargetRoot(other);
        if (targetRoot == null)
            return;

        // 같은 타겟이면 "계속 보고 있음" 갱신
        if (z.Target == targetRoot)
        {
            z.MarkTargetSeen();
            return;
        }

        // 더 가까운 새 타겟이면 갱신 가능
        z.OnTargetSeen(targetRoot);
    }

    private Transform ResolveTargetRoot(Collider2D other)
    {
        if (other == null)
            return null;

        // 플레이어 루트 찾기
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
            return player.transform;

        // 필요 시 좀비도 감지 대상으로 포함
        Zombie otherZombie = other.GetComponentInParent<Zombie>();
        if (otherZombie != null && otherZombie != z)
            return otherZombie.transform;

        return null;
    }
}