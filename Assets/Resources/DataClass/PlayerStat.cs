using System.Collections.Generic;
[System.Serializable]
public class PlayerStat
{
	public int PlayerID;
	public string PlayerName;
	public float BaseAttack;
	public float BaseMovement;
	public float RunSpeed;
	public int SilentWalkSpeed;
	public int MoveSpeedMin;
	public float BaseMaxStamina;
	public float BaseStamina;
	public int MaxHunger;
	public int StartHunger;
	public int MaxThirst;
	public int StartThirst;
	public int MaxTired;
	public int StartTired;
	public float NoiseRadiusWalk;
	public float NoiseRadiusRun;
	public float NoiseRadiusSilentWalk;
	public float AwarenessRadiusMax;
	public float AwarenessRadiusLimited;
	public float AwarenessRadiusMin;
	public float VisionRadiusMax;
	public float VisionRadiusLimited;
	public float VisionRadiusMin;
	public int VisionFOVAngleMax;
	public int VisionFOVAngleLimited;
	public int VisionFOVAngleMin;
	public int DefaultWeaponID;

	public static Dictionary<int, PlayerStat> tableDic = new Dictionary<int, PlayerStat>();
}
