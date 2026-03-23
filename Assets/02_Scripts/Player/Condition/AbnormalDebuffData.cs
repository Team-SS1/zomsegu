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
    [SerializeField] private DebuffSet common;
    public DebuffSet Common => common;


    [Header("Hunger")]
    [SerializeField] private DebuffSet hungry;
    public DebuffSet Hungry => hungry;

    [SerializeField] private DebuffSet starving;
    public DebuffSet Starving => starving;


    [Header("Thirst")]

    [SerializeField] private DebuffSet thirsty;
    public DebuffSet Thirsty => thirsty;

    [SerializeField] private DebuffSet dehydrated;
    public DebuffSet Dehydrated => dehydrated;


    [Header("Shock")]
    [SerializeField] private DebuffSet shock;
    public DebuffSet Shock => shock;

    [SerializeField] private DebuffSet heavyShock;
    public DebuffSet HeavyShock => heavyShock;

    [SerializeField] private DebuffSet severeShock;
    public DebuffSet SevereShock => severeShock;


    [Header("Injury")]
    [SerializeField] private DebuffSet injured;
    public DebuffSet Injured => injured;


    [Header("Tired")]
    [SerializeField] private DebuffSet tired;
    public DebuffSet Tired => tired;
}