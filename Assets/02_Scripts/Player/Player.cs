using System.Collections;
using System.Collections.Generic;
using PlayerEnum;
using EventEnum;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class Player : MonoBehaviour
{
    /*----- Field -----*/
    public float MaxStamina { get; private set; }
    public float CurrentStamina { get; private set; }

    public bool IsDead { get; private set; } = false;

    public PlayerType playerType;

    public PlayerData data;

    /*----- Initialize -----*/
    public void Init()
    {
        data = PlayerManager.Instance.GetPlayerData(playerType);
        var BaseStat = data.Stat;

        MaxStamina = BaseStat.BaseMaxStamina; // 추후 최대 계산값 적용
        CurrentStamina = BaseStat.BaseStamina;
    }

    public void SetDead(bool value)
    {
        IsDead = value;
    }

    public void AddStamina(float value)
    {
        CurrentStamina = Mathf.Clamp(CurrentStamina + value, 0, MaxStamina);
        EventManager.TriggerEvent(EventKey.OnStaminaChanged);
    }
    public void SetMaxStamina(float value) // 스태미나의 최종 수치값을 계산하는데 사용
    {
        MaxStamina += value;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0f, MaxStamina);
    }
}
