using System.Collections.Generic;
[System.Serializable]
public class ConvenienceStoreTypeADropItemList
{
	public int ItemID;
	public string ItemName;
	public int ItemType;
	public int ItemRarity;
	public int MaxDropCount;

	public static Dictionary<int, ConvenienceStoreTypeADropItemList> tableDic = new Dictionary<int, ConvenienceStoreTypeADropItemList>();
}
