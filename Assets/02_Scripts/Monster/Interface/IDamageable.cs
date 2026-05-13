using MonsterEnum;

public interface IDamageable
{
    void TakeDamage(float damage, ArmorType hitPart);

    void TakeDamage(float damage, ArmorType hitPart, AttackType attackType);
    bool IsDead { get; }
}