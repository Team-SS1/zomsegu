using System.Collections.Generic;
[System.Serializable]
public class MonsterStat
{
	public int MonsterID;
	public int MonsterCategory;
	public int ZombieCategory;
	public int ZombieType;
	public int BanditCategory;
	public int AnimalCategory;
	public string Name;
	public string Description;
	public int ScratchChance;
	public int BiteChance;
	public float MaxHP;
	public float MoveSpeed;
	public float RunSpeed;
	public int RunSec;
	public float StaggerResistance;
	public int AttackDamage;
	public float AttackSpeed;
	public float Defense;
	public int DropGroupID;
	public float VisionFrontRadius;
	public float VisionBackRadius;
	public float VisionFOVRadius;
	public int VisionFOVAngle;
	public int LoseAggroDistance;
	public int LoseAggroTimeSec;

	public static Dictionary<int, MonsterStat> tableDic = new Dictionary<int, MonsterStat>();
}
