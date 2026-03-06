using System.Collections.Generic;
[System.Serializable]
public class DropItemRarityRule
{
	public int RarityID;
	public int Common;
	public int Rare;
	public int Epic;
	public int Unique;

	public static Dictionary<int, DropItemRarityRule> tableDic = new Dictionary<int, DropItemRarityRule>();
}
