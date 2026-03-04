using System.Collections;
using System.Collections.Generic;
using PlayerEnum;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int Hunger { get; private set; }
    public int Thirst { get; private set; }
    public int MaxShock { get; private set; }
    public int CurrentShock { get; private set; }
    public float MaxStamina { get; private set; }
    public float CurrentStamina { get; private set; }
    public int Injury { get; private set; } // enum type ?

    public PlayerType playerType;
    public PlayerStat BaseStat { get; private set; }

    public void Init()
    {
        int id = DataManager.Instance.GetPlayerID(playerType);
        BaseStat = DataManager.Instance.GetPlayerStat(id);

        Hunger = 10;
        Thirst = 10;
        MaxShock = 7;
        CurrentShock = 0;
        MaxStamina = BaseStat.BaseMaxStamina; // 추후 최대 계산값 적용
        CurrentStamina = 100;
        Injury = 0;
    }
    public void AddHunger(int value)
    {
        Hunger = Mathf.Clamp(Hunger + value, 0, BaseStat.MaxHunger);
    }
    public void AddThirst(int value)
    {
        Thirst = Mathf.Clamp(Thirst + value, 0, BaseStat.MaxThirst);
    }
    public void AddShock(int value)
    {
        CurrentShock = Mathf.Clamp(CurrentShock + value, 0, MaxShock);
    }
    public void AddStamina(float value)
    {
        CurrentStamina = Mathf.Clamp(CurrentStamina + value, 0, MaxStamina);
    }
    public void AddInjury(int value)
    {
        Injury = Mathf.Clamp(Injury + value, 0, 4);
    }
    public void SetStamina(float value) // 스태미나의 최종 수치값을 계산하는데 사용
    {
        MaxStamina += value;
    }
}
