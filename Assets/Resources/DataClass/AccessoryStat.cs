using System.Collections.Generic;
[System.Serializable]
public class AccessoryStat
{
	public int ItemID;
	public bool CanEquipMultiple;
	public float AtkBuffAdd;
	public float AtkSpdBuffAdd;
	public float SpdBuffAdd;
	public float MaxStaminaBuffAdd;
	public int VisionRadiusLimitedAdd;
	public int VisionRadiusMinAdd;
	public int VisionFOVAngleAdd;
	public float BagCapacity;
	public float BagWeightLimit;
	public int PenaltyFreeWeight;

	public static Dictionary<int, AccessoryStat> tableDic = new Dictionary<int, AccessoryStat>();
}
