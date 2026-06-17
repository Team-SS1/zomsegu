using MonsterEnum;
using UnityEngine;

[RequireComponent(typeof(VehicleController2D))]
public class VehicleDamage : MonoBehaviour, IDamageable
{
    [Header("Durability")]
    [SerializeField] private float durability = 100f;

    [Header("Collision Layers")]
    [SerializeField] private LayerMask wallAndCarLayers;
    [SerializeField] private LayerMask monsterLayers;

    public bool IsDead => durability <= 0f;
    public bool IsBroken => durability <= 0f;
    public float Durability => durability;

    private VehicleController2D controller;

    private void Awake()
    {
        controller = GetComponent<VehicleController2D>();
        durability = Mathf.Clamp(durability, 0f, 100f);
    }

    public void TakeDamage(float damage, ArmorType hitPart)
    {
        ApplyDamage(damage);
    }

    public void TakeDamage(float damage, ArmorType hitPart, AttackType attackType)
    {
        ApplyDamage(damage);
    }

    private void ApplyDamage(float amount)
    {
        durability = Mathf.Clamp(durability - amount, 0f, 100f);

        if (durability <= 0f)
        {
            controller.SetEngine(false);
            // TODO: 주차 스프라이트 교체
            // TODO: 탑승자 강제 하차
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float speed = Mathf.Abs(controller.CurrentSpeedKmh);

        if (IsInLayerMask(collision.gameObject, wallAndCarLayers))
        {
            HandleWallOrCarCollision(speed);
            return;
        }

        if (IsInLayerMask(collision.gameObject, monsterLayers))
        {
            HandleMonsterCollision(speed);
            return;
        }
    }

    private void HandleWallOrCarCollision(float speed)
    {
        if (speed < 10f)
        {
            ApplyDamage(3f);
        }
        else if (speed < 50f)
        {
            ApplyDamage(15f);
        }
        else if (speed < 100f)
        {
            ApplyDamage(50f);
            // TODO: 탑승자 부상 1스택
        }
        else
        {
            ApplyDamage(100f);
            // TODO: 탑승자 부상 2스택 / 기존 부상 시 사망
        }

        controller.ForceStop();
    }

    private void HandleMonsterCollision(float speed)
    {
        if (speed < 10f)
        {
            controller.ForceStop();
        }
        else if (speed < 50f)
        {
            ApplyDamage(2f);
            controller.ReduceSpeed(speed * 0.5f);
        }
        else if (speed < 100f)
        {
            ApplyDamage(5f);
            controller.ReduceSpeed(10f);
            TryApplyBrokenGlassEffect();
        }
        else
        {
            ApplyDamage(10f);
            controller.ReduceSpeed(20f);
            TryApplyBrokenGlassEffect();
        }
    }

    private void TryApplyBrokenGlassEffect()
    {
        if (Random.value < 0.5f)
        {
            // TODO: 시야방해효과 적용
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
}