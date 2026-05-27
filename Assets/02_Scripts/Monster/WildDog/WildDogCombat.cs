using UnityEngine;
using MonsterEnum;

[DisallowMultipleComponent]
[RequireComponent(typeof(WildDog))]
public class WildDogCombat : MonoBehaviour
{
    private WildDog dog;

    private bool hasHitThisAttack;
    private float attackTimeoutTimer;
    private float nextAttackAllowedTime;

    [SerializeField] private float reattackBlockDuration = 0.02f;
    [SerializeField] private float attackEndSafetyBuffer = 0.25f;
    [SerializeField] private float legShoeBreakChance = 0.10f;

    private void Awake()
    {
        dog = GetComponent<WildDog>();
    }

    private void Update()
    {
        if (!dog.IsAttacking)
            return;

        attackTimeoutTimer -= Time.deltaTime;
        if (attackTimeoutTimer <= 0f)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[WildDogCombat] Attack timeout fallback fired: {name}");
#endif
            EndAttack();
        }
    }

    public bool CanStartAttackNow()
    {
        if (dog.IsDead || dog.IsAttacking || dog.IsTakingDamage || dog.IsStunned)
            return false;

        if (Time.time < nextAttackAllowedTime)
            return false;

        if (!dog.HasValidCharacterTarget())
            return false;

        if (!dog.CanAttackTarget(dog.CurrentTarget))
            return false;

        return true;
    }

    public void StartAttack()
    {
        if (!CanStartAttackNow())
            return;

        dog.IsAttacking = true;
        hasHitThisAttack = false;

        attackTimeoutTimer = dog.Stat.AttackDuration + attackEndSafetyBuffer;

#if UNITY_EDITOR
        Debug.Log($"[WildDogCombat] StartAttack timeout={attackTimeoutTimer:F2}");
#endif
    }

    public void OnAttackHit()
    {
#if UNITY_EDITOR
        Debug.Log("[WildDogCombat] OnAttackHit()");
#endif

        if (hasHitThisAttack || dog.IsDead)
            return;

        if (!dog.HasValidCharacterTarget())
            return;

        Transform target = dog.CurrentTarget;
        if (!dog.CanAttackTarget(target))
            return;

        if (!dog.IsInAttackRange(target))
            return;

        hasHitThisAttack = true;
        dog.NotifySuccessfulHit();

        IDamageable damageable = target.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        ArmorType part = WildDogHitPartUtility.GetRandomArmorPart();

        //PlayerTakeDamage playerTakeDamage = target.GetComponentInParent<PlayerTakeDamage>();
        //if (playerTakeDamage != null)
        //{
        //    playerTakeDamage.SetIncomingAttackType(AttackType.Bite);

        //    if (part == ArmorType.Leg && Random.value < legShoeBreakChance)
        //    {
        //        // TODO: 신발 파괴 API 연결
        //    }
        //}
        //
        //damageable.TakeDamage(dog.Stat.AttackDamage, part);
    }

    public void EndAttack()
    {
#if UNITY_EDITOR
        Debug.Log("[WildDogCombat] EndAttack()");
#endif

        dog.IsAttacking = false;
        hasHitThisAttack = false;
        attackTimeoutTimer = 0f;
        nextAttackAllowedTime = Time.time + reattackBlockDuration;

        if (!dog.IsDead &&
            dog.PathAgent != null &&
            dog.StateMachine.CurrentType == WildDogStateType.Aggro &&
            dog.HasValidCharacterTarget())
        {
            dog.PathAgent.ForcePathModeAfterAttack();
        }
    }

    public void ForceCancel()
    {
        dog.IsAttacking = false;
        hasHitThisAttack = false;
        attackTimeoutTimer = 0f;
        nextAttackAllowedTime = Time.time + reattackBlockDuration;
    }
}