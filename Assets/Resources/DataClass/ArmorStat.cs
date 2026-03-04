using System.Collections.Generic;
[System.Serializable]
public class ArmorStat
{
	public int ItemID;
	public int ArmorType;
	public int Durability;

	public static Dictionary<int, ArmorStat> tableDic = new Dictionary<int, ArmorStat>();
}
