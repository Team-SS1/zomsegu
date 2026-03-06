using System.Collections.Generic;
[System.Serializable]
public class ConsumableStat
{
	public int ItemID;
	public int HungerRecover;
	public int ThirstRecover;
	public float AtkBuffAdd;
	public float AtkSpdBuffAdd;
	public float SpdBuffAdd;
	public float MaxStaminaBuffAdd;
	public int Duration;
	public int ConsumeTime;
	public int ConsumeType;

	public static Dictionary<int, ConsumableStat> tableDic = new Dictionary<int, ConsumableStat>();
}
