//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//public class MonsterSpatialUtil : MonoBehaviour
//{
//    private Monster monster;

//    private void Awake()
//    {
//        monster = GetComponent<Monster>();
//    }

//    public Vector2 GetBodyOrigin()
//    {
//        var sr = GetComponentInChildren<SpriteRenderer>();
//        if (sr == null) return transform.position;

//        float bodyRatio = 0.2f;
//        float height = sr.bounds.size.y;

//        return (Vector2)transform.position + Vector2.up * height * bodyRatio;
//    }

//    public Vector2 GetTargetBodyOrigin()
//    {
//        if (monster.target == null) return Vector2.zero;

//        Collider2D col = monster.target.GetComponent<Collider2D>();
//        if (col != null)
//            return col.bounds.center;

//        var sr = monster.target.GetComponentInChildren<SpriteRenderer>();
//        if (sr != null)
//        {
//            float height = sr.bounds.size.y;
//            return (Vector2)monster.target.position + Vector2.up * height * 0.5f;
//        }

//        return (Vector2)monster.target.position;
//    }

//    public bool IsInAttackRange2D(Vector2 targetPos, float range)
//    {
//        Vector2 my = GetBodyOrigin();

//        float dx = Mathf.Abs(my.x - targetPos.x);
//        float dy = Mathf.Abs(my.y - targetPos.y);

//        float xAllowance = range;
//        float yAllowance = range;

//        if (monster.FacingDirection.y > -0.5f) // 아래를 보고 있을 때
//            yAllowance *= 1.8f;

//        return dx <= xAllowance && dy <= yAllowance;
//    }

//    public Vector2 GetLightDetectOrigin()
//    {
//        if (TryGetComponent<BoxCollider2D>(out var box))
//            return box.bounds.center;

//        if (TryGetComponent<CapsuleCollider2D>(out var cap))
//            return cap.bounds.center;

//        if (TryGetComponent<SpriteRenderer>(out var sr))
//            return sr.bounds.center;

//        return transform.position;
//    }

//    public Vector2 GetHitDirFromAttacker(Transform attacker)
//    {
//        Vector2 myPos = GetBodyOrigin();
//        Vector2 attackerPos = attacker != null ? (Vector2)attacker.position : myPos;

//        Vector2 dir = (myPos - attackerPos);
//        if (dir.sqrMagnitude < 0.0001f)
//            dir = -monster.FacingDirection;

//        return dir.normalized;
//    }

//    //public static Vector2 EightDirToVector2(EightDir dir)
//    //{
//    //    switch (dir)
//    //    {
//    //        case EightDir.Up: return Vector2.up;
//    //        case EightDir.Down: return Vector2.down;
//    //        case EightDir.Left: return Vector2.left;
//    //        case EightDir.Right: return Vector2.right;
//    //        case EightDir.UpLeft: return (Vector2.up + Vector2.left).normalized;
//    //        case EightDir.UpRight: return (Vector2.up + Vector2.right).normalized;
//    //        case EightDir.DownLeft: return (Vector2.down + Vector2.left).normalized;
//    //        case EightDir.DownRight: return (Vector2.down + Vector2.right).normalized;
//    //        default: return Vector2.down;
//    //    }
//    //}
//}