using System.Collections;
using System.Collections.Generic;
using PlayerEnum;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Player : MonoBehaviour
{
    /*----- Field -----*/
    public int Hunger { get; private set; }
    public int MaxHunger { get; private set; }
    public int Thirst { get; private set; }
    public int MaxThirst { get; private set; }
    public int MaxShock { get; private set; }
    public int CurrentShock { get; private set; }
    public float MaxStamina { get; private set; }
    public float CurrentStamina { get; private set; }
    public float Tired { get; private set; }
    public float MaxTired { get; private set; }
    public int Injury { get; private set; } // enum type ?

    public Inventory Inventory { get; private set; }

    public PlayerType playerType;

    public PlayerData data;

    /*----- Initialize -----*/
    public void Init()
    {
        data = PlayerManager.Instance.GetPlayerData(playerType);
        var BaseStat = data.Stat;

        Hunger = BaseStat.StartHunger;
        MaxHunger = BaseStat.MaxHunger;

        Thirst = BaseStat.StartThirst;
        MaxThirst = BaseStat.MaxThirst;

        MaxShock = BaseStat.MaxShock;
        CurrentShock = BaseStat.StartShock;

        MaxStamina = BaseStat.BaseMaxStamina; // 추후 최대 계산값 적용
        CurrentStamina = BaseStat.BaseStamina;

        Tired = BaseStat.StartTired;
        MaxTired = BaseStat.MaxTired;

        Injury = 0;

        Inventory = new Inventory(playerType);
    }

    public void AddHunger(int value)
    {
        Hunger = Mathf.Clamp(Hunger + value, 0, MaxHunger);
    }
    public void AddThirst(int value)
    {
        Thirst = Mathf.Clamp(Thirst + value, 0, MaxThirst);
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
    public void AddTired(int value)
    {
        Tired = Mathf.Clamp(Tired + value, 0, MaxTired);
    }
    public void SetStamina(float value) // 스태미나의 최종 수치값을 계산하는데 사용
    {
        MaxStamina += value;
    }
}
