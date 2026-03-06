using System.Collections.Generic;
[System.Serializable]
public class WeaponStat
{
	public int ItemID;
	public int WeaponCategory;
	public int WeaponDamageType;
	public int WeaponSprite;
	public float Attack;
	public float AttackSpeed;
	public float AttackRange;
	public int AttackAngle;
	public float CritChance;
	public int Durability;
	public float KnockbackForce;
	public float HitStun;
	public bool MultipleHit;
	public int MonsterHitNoiseRadius;
	public int EnvironmentHitNoiseRadius;
	public int ObjectHitNoiseRadius;
	public int AttackNoiseRadius;
	public float ProjectileSpeed;

	public static Dictionary<int, WeaponStat> tableDic = new Dictionary<int, WeaponStat>();
}
