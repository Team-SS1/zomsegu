using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ZombieAggroZone : MonoBehaviour
{
    private Zombie z;
    private CircleCollider2D col;

    private bool radiusSynced;

    private void Awake()
    {
        z = GetComponentInParent<Zombie>();
        col = GetComponent<CircleCollider2D>();

        col.isTrigger = true;
    }

    private void Start()
    {
        TrySyncRadiusFromStat();
    }

    private void Update()
    {
        if (!radiusSynced)
            TrySyncRadiusFromStat();
    }

    private void OnValidate()
    {
        if (col == null) col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private void TrySyncRadiusFromStat()
    {
        if (z == null)
            z = GetComponentInParent<Zombie>();

        if (col == null)
            col = GetComponent<CircleCollider2D>();

        if (z == null || z.stat == null || col == null)
            return;

        col.radius = Mathf.Max(
            z.stat.VisionFrontRadius,
            z.stat.VisionBackRadius,
            z.stat.VisionFOVRadius
        );

        radiusSynced = true;

#if UNITY_EDITOR
        Debug.Log($"[AggroZone] Radius Sync 완료: {col.radius}, Zombie:{z.name}, Stat:{z.stat.Name}");
#endif
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDetect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDetect(other);
    }

    private void TryDetect(Collider2D other)
    {
        if (z == null || z.IsDead)
            return;

        if (!radiusSynced)
            TrySyncRadiusFromStat();

        Transform targetRoot = ResolveTargetRoot(other);
        if (targetRoot == null)
            return;

        if (!z.CanAggroTarget(targetRoot))
        {
//#if UNITY_EDITOR
//            Debug.Log($"[AggroZone] LayerMask 불일치: {targetRoot.name}, Layer:{LayerMask.LayerToName(targetRoot.gameObject.layer)}");
//#endif
//            return;
//        }

//        if (!CanSeeTarget(targetRoot))
//        {
//#if UNITY_EDITOR
//            Debug.Log($"[AggroZone] Trigger 안에는 있지만 시야 조건 실패: {targetRoot.name}");
//#endif
//            return;
        }

//#if UNITY_EDITOR
//        Debug.Log($"[AggroZone] 타겟 감지 성공: {targetRoot.name}");
//#endif

        if (z.Target == targetRoot)
            z.MarkTargetSeen();
        else
            z.OnTargetSeen(targetRoot);
    }

    private Transform ResolveTargetRoot(Collider2D other)
    {
        if (other == null)
            return null;

        Player player = other.GetComponentInParent<Player>();
        if (player != null)
            return player.transform;

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

        if (dist <= z.stat.VisionFOVRadius)
        {
            float angle = Vector2.Angle(forward, dir);
            if (angle <= z.stat.VisionFOVAngle * 0.5f)
                return true;
        }

        if (dot >= 0f && dist <= z.stat.VisionFrontRadius)
            return true;

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

    private void OnDrawGizmosSelected()
    {
        if (col == null)
            col = GetComponent<CircleCollider2D>();

        Gizmos.DrawWireSphere(transform.position, col.radius);
    }
}