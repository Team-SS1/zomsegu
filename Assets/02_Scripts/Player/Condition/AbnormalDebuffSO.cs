using UnityEngine;

[System.Serializable]
public class DebuffSet
{
    public float attack;
    public float attackSpeed;
    public float moveSpeed;
    public float stamina;
}

[CreateAssetMenu(fileName = "AbnormalDebuffData", menuName = "SO/Abnormal/AbnormalDebuffData")]
public class AbnormalDebuffData : ScriptableObject
{
    [Header("Common")]
    public DebuffSet common;

    [Header("Hunger")]
    public DebuffSet hungry;
    public DebuffSet starving;

    [Header("Thirst")]
    public DebuffSet thirsty;
    public DebuffSet dehydrated;

    [Header("Shock")]
    public DebuffSet shock;
    public DebuffSet heavyShock;
    public DebuffSet severeShock;

    [Header("Injury")]
    public DebuffSet injured;

    [Header("Tired")]
    public DebuffSet tired;
}