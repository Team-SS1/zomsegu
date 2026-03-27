using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ZombieAggroZoneCheck : MonoBehaviour
{
    private Zombie z;
    private CircleCollider2D col;

    private void Awake()
    {
        z = GetComponentInParent<Zombie>();
        col = GetComponent<CircleCollider2D>();

        col.isTrigger = true;
        SyncRadiusFromStat();
    }

    private void Start()
    {
        SyncRadiusFromStat();
    }

    private void OnValidate()
    {
        if (col == null) col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private void SyncRadiusFromStat()
    {
        if (z == null || z.stat == null || col == null)
            return;

        col.radius = Mathf.Max(
            z.stat.VisionFrontRadius,
            z.stat.VisionBackRadius,
            z.stat.VisionFOVRadius
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (z == null || z.IsDead)
            return;

        Transform targetRoot = ResolveTargetRoot(other);
        if (targetRoot == null)
            return;

        if (!z.CanAggroTarget(targetRoot))
            return;

        if (CanSeeTarget(targetRoot))
            z.OnTargetSeen(targetRoot);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (z == null || z.IsDead)
            return;

        Transform targetRoot = ResolveTargetRoot(other);
        if (targetRoot == null)
            return;

        if (!z.CanAggroTarget(targetRoot))
            return;

        if (!CanSeeTarget(targetRoot))
            return;

        if (z.Target == targetRoot)
            z.MarkTargetSeen();
        else
            z.OnTargetSeen(targetRoot);
    }

    private Transform ResolveTargetRoot(Collider2D other)
    {
        if (other == null)
            return null;

        // 플레이어 우선
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
            return player.transform;

        //Zombie otherZombie = other.GetComponentInParent<Zombie>();
        //if (otherZombie != null && otherZombie != z)
        //    return otherZombie.transform;

        return null;
    }

    private bool CanSeeTarget(Transform targetRoot)
    {
        if (z == null || z.stat == null || targetRoot == null)
            return false;

        Vector2 origin = z.GetNavigationOrigin();
        Vector2 targetPos = GetTargetCenter(targetRoot);

        Vector2 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;
        if (dist <= 0.001f)
            return true;

        Vector2 forward = z.FacingDirection.sqrMagnitude > 0.001f
            ? z.FacingDirection.normalized
            : Vector2.down;

        Vector2 dir = toTarget / dist;
        float dot = Vector2.Dot(forward, dir);

        // 정면 부채꼴
        if (dist <= z.stat.VisionFOVRadius)
        {
            float angle = Vector2.Angle(forward, dir);
            if (angle <= z.stat.VisionFOVAngle * 0.5f)
                return true;
        }

        // 전면 180도
        if (dot >= 0f && dist <= z.stat.VisionFrontRadius)
            return true;

        // 후면 180도
        if (dot < 0f && dist <= z.stat.VisionBackRadius)
            return true;

        return false;
    }

    private Vector2 GetTargetCenter(Transform t)
    {
        Collider2D hitCol = t.GetComponent<Collider2D>();
        if (hitCol != null)
            return hitCol.bounds.center;

        SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            return sr.bounds.center;

        return t.position;
    }
}