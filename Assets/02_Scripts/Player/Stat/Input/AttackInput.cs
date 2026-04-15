using UnityEngine;

[System.Serializable]
public class AttackInput
{
    public float baseAttack;
    public float weaponAttack;
    public float consumeBuff;
    public float equipAttack;
    public float otherBuffAddition = 0f;
    public float otherBuffMultiplication = 1f;

    public float conditionDebuff;
    
    public float painDebuff;
    public float painkillerBuff;

    public float minAttack = 30f;
    
}
