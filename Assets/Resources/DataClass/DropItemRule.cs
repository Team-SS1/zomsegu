using System.Collections.Generic;
[System.Serializable]
public class DropItemRule
{
	public int SlotID;
	public int Empty;
	public int Weapon;
	public int HeadArmor;
	public int BodyArmor;
	public int LegArmor;
	public int Accessory;
	public int Shoes;
	public int Bag;
	public int Consumable;
	public int Misc;

	public static Dictionary<int, DropItemRule> tableDic = new Dictionary<int, DropItemRule>();
}
