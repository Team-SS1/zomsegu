using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.UI.Image;

public class PlayerAttack : MonoBehaviour
{
    public PlayerAim Aim {  get; private set; }
    public bool IsAttacking {  get; private set; }
    public Vector2 AttackDirection { get; private set; }
    public int AttackRange {  get; private set; } = 5;
    public int AttackAngle { get; private set; } = 120;


    private void Awake()
    {
        Aim = GetComponent<PlayerAim>();
    }

    private void Start()
    {
        ///* --- Test Set --- */
        //InputManager.Instance.SetMaps(InputEnum.ActionMaps.Gameplay);
        ///* --- -------- --- */
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Attack, OnAttack);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartCoroutine(Attack());
#if UNITY_EDITOR
            Debug.Log("Attack Input");
#endif

        }
    }

    public IEnumerator Attack()
    {
        if (IsAttacking)
        {
#if UNITY_EDITOR
            Debug.Log("Attack Return");
#endif
            yield break;
        }


        IsAttacking = true;

        AttackDirection = Aim.Mousedir.GetLocalMouseDirection(transform.position);

        yield return new WaitForSeconds(HalfAttackDuration());

        ExcuteAttack(AttackDirection);

        yield return new WaitForSeconds(HalfAttackDuration());

        IsAttacking = false;
    }

    public void ExcuteAttack(Vector2 attackdirection)
    {
        Vector2 attackWorldDirection = transform.TransformDirection(attackdirection);

        Collider2D[] hits = CheckRange(attackWorldDirection);

        foreach(var col in hits)
        {
            if (col.isTrigger)
                return;
            if (col.TryGetComponent<Monster>(out var monster))
            {
#if UNITY_EDITOR
                Debug.Log("Monster Hit");
#endif
                //monster.TakeDamage(damage);
            }
        }
    }

    public Collider2D[] CheckRange(Vector2 forwardDir)
    {
        List<Collider2D> results = new List<Collider2D>();

        Vector2 origin = transform.position;

        Collider2D[] candidates =
            Physics2D.OverlapCircleAll(origin, AttackRange);

        float halfAngle = AttackAngle * 0.5f;

        foreach (Collider2D col in candidates)
        {
            if (col.gameObject == gameObject)
                continue;

            // 플레이어 기준 대상의 가장 가까운 지점
            Vector2 targetPos = col.ClosestPoint(origin);

            // 플레이어 >> 대상 방향 벡터
            Vector2 toTarget = targetPos - origin;

            if (toTarget.sqrMagnitude < 0.0001f)
                continue;

            // 방향 벡터 정규화 (각도 계산용)
            toTarget.Normalize();

            // 전방 벡터와 대상 방향 벡터의 내적 계산 / dot = cos(theta)
            float dot = Vector2.Dot(forwardDir.normalized, toTarget);

            // 내적값을 각도로 변환 (라디안 → 도)
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

            // 대상이 공격 부채꼴 범위 안에 있으면 공격 대상으로 포함
            if (angle <= halfAngle)
            {
                results.Add(col);
            }
        }
        // 최종 공격 대상 목록 반환
        return results.ToArray();
    }

    public float HalfAttackDuration()
    {
        float attackSpeed = 1f;
        float halfAttackSpeed = attackSpeed / 2;
        return halfAttackSpeed;
    }
}
