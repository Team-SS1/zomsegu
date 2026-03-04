using System.Collections.Generic;
[System.Serializable]
public class PharmacyTypeADropItemList
{
	public int ItemID;
	public string ItemName;
	public int ItemType;
	public int ItemRarity;
	public int MaxDropCount;

	public static Dictionary<int, PharmacyTypeADropItemList> tableDic = new Dictionary<int, PharmacyTypeADropItemList>();
}
