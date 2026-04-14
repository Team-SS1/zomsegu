using UnityEngine;

[System.Serializable]
public class AttackSpeedInput : MonoBehaviour
{
    public float weaponAttackSpeed;
    public float consumeBuff;
    public float otherBuff;

    public float conditionDebuff;

    public float painDebuff;
    public float painkillerBuff;

    public float minAttackSpeed = 2f;
}
