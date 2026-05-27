using MonsterEnum;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WildDogAggroSensor : MonoBehaviour
{
    private WildDog dog;
    private CircleCollider2D col;

    private void Awake()
    {
        dog = GetComponentInParent<WildDog>();
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        SyncRadius();
    }

    private void Start()
    {
        SyncRadius();
    }

    private void SyncRadius()
    {
        if (dog == null || dog.Stat == null || col == null)
            return;

        col.radius = Mathf.Max(dog.Stat.VisionFrontRadius360, dog.Stat.VisionFrontFOVRadius);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Handle(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Handle(other);
    }

    private void Handle(Collider2D other)
    {
        if (dog == null || dog.IsDead)
            return;

        Transform root = ResolveTargetRoot(other);
        if (root == null || root == dog.transform)
            return;

        if (dog.ShouldFleeFrom(root))
        {
            dog.EnterFlee(root.position);
            return;
        }

        if (!dog.CanAggroTarget(root))
            return;

        if (!CanSee(root))
            return;

        if (dog.CurrentTarget == root)
            dog.MarkSeen();
        else if (dog.StateMachine.CurrentType != WildDogStateType.Aggro ||
                 dog.AggroTargetKind != WildDogAggroTargetKind.Character)
            dog.EnterAggroCharacter(root);
    }

    private Transform ResolveTargetRoot(Collider2D other)
    {
        if (other == null)
            return null;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
            return rb.transform;

        return other.transform.root;
    }

    private bool CanSee(Transform target)
    {
        Vector2 origin = dog.GetNavigationOrigin();
        Vector2 targetPos = dog.GetTargetCenter(target);

        Vector2 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;
        if (dist <= 0.001f)
            return true;

        Vector2 forward = dog.FacingDirection.sqrMagnitude > 0.001f
            ? dog.FacingDirection.normalized
            : Vector2.down;

        Vector2 dir = toTarget / dist;

        if (dist <= dog.Stat.VisionFrontRadius360)
            return true;

        if (dist <= dog.Stat.VisionFrontFOVRadius)
        {
            float angle = Vector2.Angle(forward, dir);
            if (angle <= dog.Stat.VisionFrontFOVAngle * 0.5f)
                return true;
        }

        return false;
    }
}