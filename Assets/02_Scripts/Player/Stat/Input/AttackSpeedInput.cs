using UnityEngine;

[System.Serializable]
public class AttackSpeedInput
{
    public float weaponAttackSpeed;
    public float consumeBuff;
    public float otherBuff;

    public float conditionDebuff;

    public float painDebuff;
    public float painkillerBuff;

    public float minAttackSpeed = 2f;
}
