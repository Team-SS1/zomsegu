using System.Collections.Generic;
[System.Serializable]
public class CommonItemData
{
	public int ItemID;
	public string ItemName;
	public int ItemType;
	public string Icon;
	public string Description;
	public int ItemRarity;
	public bool IsStackable;
	public bool IsSellable;
	public bool IsDroppable;
	public int PlayerAudioType;
	public float ItemVolume;
	public float ItemWeight;

	public static Dictionary<int, CommonItemData> tableDic = new Dictionary<int, CommonItemData>();
}
