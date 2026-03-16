using System.Collections.Generic;
using ItemEnum;
using PlayerEnum;
using UnityEngine;

public class Abnormal
{
    public AbnormalType Type { get; private set; }

    public int BaseValue { get; private set; }

    public int CurValue { get; private set; }

    public int MaxValue { get; private set; }

    /*-- Debuff Dictionary --*/
    public Dictionary<AbnormalDebuffType, float> DebuffDict { get; private set; }
    /*-----------------------*/

    /// <summary>
    /// 상태이상 객체를 생성합니다.
    /// 시작값, 최대값, 영향을 줄 스탯 목록을 초기화합니다.
    /// </summary>
    public Abnormal(
        AbnormalType type,
        int baseValue,
        int maxValue,
        AbnormalDebuffType[] debuffStats)
    {
        Type = type;
        BaseValue = baseValue;
        CurValue = baseValue;
        MaxValue = maxValue;

        DebuffDict = new Dictionary<AbnormalDebuffType, float>();

        if (debuffStats != null)
        {
            foreach (AbnormalDebuffType stat in debuffStats)
            {
                if (!DebuffDict.ContainsKey(stat))
                {
                    DebuffDict.Add(stat, 0);
                }
            }
        }
    }

    /// <summary>
    /// 현재 상태이상 수치를 증가 / 감소
    /// </summary>
    /// <param name="value"></param>
    public void AddValue(int value)
    {
        CurValue = (int)Mathf.Clamp(CurValue + value, 0f, MaxValue);
    }

    /// <summary>
    /// 현재 상태이상 수치를 직접 세팅
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(int value)
    {
        CurValue = (int)Mathf.Clamp(value, 0f, MaxValue);
    }

    /// <summary>
    /// 특정 스탯의 디버프 값을 설정합니다.
    /// DebuffDict에 등록된 스탯만 수정합니다.
    /// </summary>
    public void SetDebuff(AbnormalDebuffType statType, float value)
    {
        if (DebuffDict.ContainsKey(statType))
        {
            DebuffDict[statType] = value;
        }
    }

    /// <summary>
    /// 특정 스탯의 디버프 값을 반환합니다.
    /// 등록되지 않은 스탯이면 0을 반환합니다.
    /// </summary>
    public float GetDebuff(AbnormalDebuffType statType)
    {
        if (DebuffDict.TryGetValue(statType, out float value))
        {
            return value;
        }
        return 0f;
    }
}