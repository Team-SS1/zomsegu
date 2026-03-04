using System.Collections.Generic;
[System.Serializable]
public class ClothingShopTypeADropItemList
{
	public int ItemID;
	public string ItemName;
	public int ItemType;
	public int ItemRarity;
	public int MaxDropCount;

	public static Dictionary<int, ClothingShopTypeADropItemList> tableDic = new Dictionary<int, ClothingShopTypeADropItemList>();
}
