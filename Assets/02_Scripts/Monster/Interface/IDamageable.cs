using MonsterEnum;

public interface IDamageable
{
    void TakeDamage(float damage, ArmorType hitPart);
    bool IsDead { get; }
}