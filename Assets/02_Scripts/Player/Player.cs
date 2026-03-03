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

    private void Awake()
    {
        DataTableLoader.LoadTables();
        int id = GetPlayerID(playerType);
        BaseStat = DataManager.Instance.GetPlayerStat(id);

    }
    private int GetPlayerID(PlayerType type)
    {
        switch (type)
        {
            case PlayerType.Player_SHIN:
                return GameConstants.PlayerID_A;
            case PlayerType.Player_HAN:
                return GameConstants.PlayerID_B;
            default:
                return 0;
        }
    }
    public void Init()
    {
        Hunger = 10;
        Thirst = 10;
        MaxShock = 7;
        CurrentShock = 7;
        MaxStamina = 100;
        CurrentStamina = 100;
        Injury = 0;
    }
    public void AddHunger(int value)
    {
        Hunger = Mathf.Clamp(Hunger + value, 0, 10);
    }
    public void AddThirst(int value)
    {
        Thirst = Mathf.Clamp(Thirst + value, 0, 10);
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
    public void SetStamina(float value)
    {
        MaxStamina += value;
    }
}
