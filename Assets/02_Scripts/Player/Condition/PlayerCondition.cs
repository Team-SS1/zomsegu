using System.Collections.Generic;
using EventEnum;
using ItemEnum;
using PlayerEnum;
using UnityEngine;

public class PlayerCondition : MonoBehaviour
{
    [SerializeField] private Player player;

    // 상태이상의 실제 값 저장소
    private Dictionary<AbnormalType, Abnormal> abnormalDict = new();

    // 읽기 전용
    public IReadOnlyDictionary<AbnormalType, Abnormal> AbnormalDict => abnormalDict;

    /// <summary>
    /// Player의 DataTable 초기화 이후
    /// </summary>
    public void Init()
    {
        if (player == null)
        {
            Debug.LogError("PlayerCondition: Player is null.");
            return;
        }

        if (player.data == null)
        {
            Debug.LogError("PlayerCondition: Player data not initialized.");
            return;
        }

        InitAbnormal();
        CalculateAllDebuffs();
    }

    /// <summary>
    /// Player의 기본 스탯 데이터 기반
    /// 상태이상 Dictionary를 생성
    /// </summary>
    private void InitAbnormal()
    {
        abnormalDict.Clear();

        var baseStat = player.data.Stat;

        abnormalDict.Add(
            AbnormalType.Hunger,
            new Abnormal(
                AbnormalType.Hunger,
                baseStat.StartHunger,
                baseStat.MaxHunger,
                new AbnormalDebuffType[] { AbnormalDebuffType.Attack, AbnormalDebuffType.AttackSpeed, AbnormalDebuffType.Stamina }
            )
        );

        abnormalDict.Add(
            AbnormalType.Thirst,
            new Abnormal(
                AbnormalType.Thirst,
                baseStat.StartThirst,
                baseStat.MaxThirst,
                new AbnormalDebuffType[] { AbnormalDebuffType.MoveSpeed, AbnormalDebuffType.AttackSpeed, AbnormalDebuffType.Stamina }
            )
        );

        abnormalDict.Add(
            AbnormalType.Shock,
            new Abnormal(
                AbnormalType.Shock,
                baseStat.StartShock,
                baseStat.MaxShock,
                new AbnormalDebuffType[] { AbnormalDebuffType.Attack, AbnormalDebuffType.AttackSpeed, AbnormalDebuffType.Stamina }
            )
        );

        abnormalDict.Add(
            AbnormalType.Tired,
            new Abnormal(
                AbnormalType.Tired,
                baseStat.StartTired,
                baseStat.MaxTired,
                new AbnormalDebuffType[] { AbnormalDebuffType.Stamina, AbnormalDebuffType.MoveSpeed }
            )
        );

        abnormalDict.Add(
            AbnormalType.Injury,
            new Abnormal(
                AbnormalType.Injury, /* 0 = Common,
                                      * 1 = Injury, 
                                      * 2 = CriticalInjury */
                0,
                2,
                new AbnormalDebuffType[] { AbnormalDebuffType.Attack, AbnormalDebuffType.AttackSpeed, AbnormalDebuffType.MoveSpeed }
            )
        );
    }

    /// <summary>
    /// AbnormalType의 대응하는 상태이상을 반환
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Abnormal GetAbnormal(AbnormalType type)
    {
        if (abnormalDict.TryGetValue(type, out Abnormal abnormal))
        {
            return abnormal;
        }

        Debug.LogWarning($"PlayerCondition: Abnormal {type} not found.");
        return null;
    }

    /// <summary>
    /// AbnormalType의 대응하는 상태이상의 현재 수치를 반환
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetValue(AbnormalType type)
    {
        Abnormal abnormal = GetAbnormal(type);
        return abnormal == null ? 0f : abnormal.CurValue;
    }

    /// <summary>
    /// AbnormalType의 대응하는 상태이상의 최대 수치를 반환
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetMaxValue(AbnormalType type)
    {
        Abnormal abnormal = GetAbnormal(type);
        return abnormal == null ? 0f : abnormal.MaxValue;
    }

    /// <summary>
    /// AbnormalType의 대응하는 상태이상 증가/감소
    /// 대응하는 상태의 Debuff 계산
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void AddValue(AbnormalType type, int value)
    {
        Abnormal abnormal = GetAbnormal(type);
        if (abnormal == null) 
            return;

        abnormal.AddValue(value);
        CalculateDebuff(type);
    }

    /// <summary>
    /// AbnormalType의 대응하는 상태이상 값을 재설정
    /// 대응하는 상태의 Debuff 계산
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void SetValue(AbnormalType type, int value)
    {
        Abnormal abnormal = GetAbnormal(type);
        if (abnormal == null) 
            return;

        abnormal.SetValue(value);
        CalculateDebuff(type);
    }

    /// <summary>
    /// AbnormalType의 대응하는 디버프 값을 반환
    /// </summary>
    /// <param name="abnormalType"></param>
    /// <param name="statType"></param>
    /// <returns></returns>
    public float GetDebuff(AbnormalType abnormalType, AbnormalDebuffType statType)
    {
        Abnormal abnormal = GetAbnormal(abnormalType);
        if (abnormal == null) 
            return 0f;

        return abnormal.GetDebuff(statType);
    }

    /// <summary>
    /// AbnormalType의 대응하는 모든 디버프 값을 계산
    /// </summary>
    public void CalculateAllDebuffs()
    {
        foreach (AbnormalType type in abnormalDict.Keys)
        {
            CalculateDebuff(type);
        }
    }

    /// <summary>
    /// AbnormalType의 대응하는 디버프 값을 계산
    /// </summary>
    /// <param name="type"></param>
    private void CalculateDebuff(AbnormalType type)
    {
        Abnormal abnormal = GetAbnormal(type);
        if (abnormal == null) return;

        switch (type)
        {
            case AbnormalType.Hunger:
                HungerDebuff(abnormal);
                EventManager.TriggerEvent(EventKey.OnHungerChanged);
                break;

            case AbnormalType.Thirst:
                ThirstDebuff(abnormal);
                EventManager.TriggerEvent(EventKey.OnThirstChanged);
                break;

            case AbnormalType.Shock:
                ShockDebuff(abnormal);
                EventManager.TriggerEvent(EventKey.OnShockChanged);
                break;

            case AbnormalType.Tired:
                TiredDebuff(abnormal);
                EventManager.TriggerEvent(EventKey.OnTiredChanged);
                break;

            case AbnormalType.Injury:
                InjuryDebuff(abnormal);
                EventManager.TriggerEvent(EventKey.OnInjuryChanged);
                break;
        }
    }

    /// <summary>
    /// Hunger 수치에 따라 공격력, 공격속도, 스태미나 디버프를 계산
    /// </summary>
    /// <param name="abnormal"></param>
    private void HungerDebuff(Abnormal abnormal)
    {
        float hunger = abnormal.CurValue;

        abnormal.SetDebuff(AbnormalDebuffType.Attack, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.Stamina, 0f);

        if (hunger >= 1 && hunger <= 3)
        {
            abnormal.SetDebuff(AbnormalDebuffType.Attack, 0.1f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.1f);
        }
        else if (hunger == 0)
        {
            abnormal.SetDebuff(AbnormalDebuffType.Attack, 0.2f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.2f);
            abnormal.SetDebuff(AbnormalDebuffType.Stamina, 30f);
        }
    }

    /// <summary>
    /// Thirst 수치에 따라 이동속도, 공격속도, 스태미나 디버프를 계산
    /// </summary>
    /// <param name="abnormal"></param>
    private void ThirstDebuff(Abnormal abnormal)
    {
        float thirst = abnormal.CurValue;

        abnormal.SetDebuff(AbnormalDebuffType.MoveSpeed, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.Stamina, 0f);

        if (thirst >= 1 && thirst <= 3)
        {
            abnormal.SetDebuff(AbnormalDebuffType.MoveSpeed, 0.1f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.1f);
        }
        else if (thirst == 0)
        {
            abnormal.SetDebuff(AbnormalDebuffType.MoveSpeed, 0.3f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.3f);
            abnormal.SetDebuff(AbnormalDebuffType.Stamina, 30f);
        }
    }

    /// <summary>
    /// Shock 수치에 따라 공격력, 공격속도, 스태미나 디버프를 계산
    /// </summary>
    /// <param name="abnormal"></param>
    private void ShockDebuff(Abnormal abnormal)
    {
        float shock = abnormal.CurValue;

        abnormal.SetDebuff(AbnormalDebuffType.Attack, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.Stamina, 0f);

        if (shock >= 2 && shock <= 3)
        {
            abnormal.SetDebuff(AbnormalDebuffType.Attack, 0.1f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.1f);
        }
        else if (shock >= 4 && shock <= 5)
        {
            abnormal.SetDebuff(AbnormalDebuffType.Attack, 0.2f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.2f);
            abnormal.SetDebuff(AbnormalDebuffType.Stamina, 20f);
        }
        else if (shock >= 6)
        {
            abnormal.SetDebuff(AbnormalDebuffType.Attack, 0.3f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.3f);
            abnormal.SetDebuff(AbnormalDebuffType.Stamina, 30f);
        }
    }

    /// <summary>
    /// Tired 수치에 따라 스태미나, 이동속도 디버프를 계산
    /// </summary>
    /// <param name="abnormal"></param>
    private void TiredDebuff(Abnormal abnormal)
    {
        float tired = abnormal.CurValue;

        abnormal.SetDebuff(AbnormalDebuffType.Stamina, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.MoveSpeed, 0f);

        if (tired <= 0)
        {
            abnormal.SetDebuff(AbnormalDebuffType.Stamina, 20f);
        }
    }

    /// <summary>
    /// Injury 수치에 따라 공격력, 공격속도, 이동속도 디버프를 계산
    /// </summary>
    /// <param name="abnormal"></param>
    private void InjuryDebuff(Abnormal abnormal)
    {
        float injury = abnormal.CurValue;

        abnormal.SetDebuff(AbnormalDebuffType.Attack, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0f);
        abnormal.SetDebuff(AbnormalDebuffType.MoveSpeed, 0f);

        if (injury > 0)
        {
            abnormal.SetDebuff(AbnormalDebuffType.Attack, 0.2f);
            abnormal.SetDebuff(AbnormalDebuffType.AttackSpeed, 0.2f);
            abnormal.SetDebuff(AbnormalDebuffType.MoveSpeed, 0.2f);
        }
    }
}