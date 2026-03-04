using System.Collections.Generic;
[System.Serializable]
public class CarTypeADropItemList
{
	public int ItemID;
	public string ItemName;
	public int ItemType;
	public int ItemRarity;
	public int MaxDropCount;

	public static Dictionary<int, CarTypeADropItemList> tableDic = new Dictionary<int, CarTypeADropItemList>();
}
