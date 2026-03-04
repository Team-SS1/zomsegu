using System.Collections.Generic;
[System.Serializable]
public class ZombieTypeADropItemList
{
	public int ItemID;
	public string ItemName;
	public int ItemType;
	public int ItemRarity;
	public int MaxDropCount;

	public static Dictionary<int, ZombieTypeADropItemList> tableDic = new Dictionary<int, ZombieTypeADropItemList>();
}
