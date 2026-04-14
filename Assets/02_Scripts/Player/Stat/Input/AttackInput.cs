using UnityEngine;

[System.Serializable]
public class AttackInput : MonoBehaviour
{
    public float baseAttack;
    public float weaponAttack;
    public float consumeBuff;
    public float otherBuffAddition;
    public float otherBuffmultiplication = 1f;

    public float conditionDebuff;
    
    public float painDebuff;
    public float painkillerBuff;

    public float minAttack = 30f;
}
