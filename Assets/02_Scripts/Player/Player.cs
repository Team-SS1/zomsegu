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

    private float currentAttack;
    private float currentAttackSpeed;
    private float currentMovement;
    private float currentRunMovement;
    private SpriteController spriteController;

    public float CurrentAttack => currentAttack;
    public float CurrentAttackSpeed => currentAttackSpeed;
    public float CurrentMovement => currentMovement;
    public float CurrentRunMovement => currentRunMovement;
    public SpriteController SpriteController => spriteController;

    public PlayerType playerType;

    public PlayerData data;

    /*----- Initialize -----*/
    private void Awake()
    {
        spriteController = GetComponentInChildren<SpriteController>();
    }
    public void Init()
    {
        data = PlayerDataManager.Instance.GetPlayerData(playerType);
        
        var BaseStat = data.Stat;

        MaxStamina = BaseStat.BaseMaxStamina; // 추후 최대 계산값 적용
        CurrentStamina = BaseStat.BaseStamina;
        spriteController.ChangeSprite(SpriteType.Bat);
    }

    public void SetDead(bool value)
    {
        IsDead = value;

        //컨디션 UI 쪽 이벤트

        PlayerCondition condition = GetComponent<PlayerCondition>();    
        EventManager.TriggerEvent<PlayerCondition>(EventKey.OnInjuryChanged, condition);
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

    public void SetAttack(float value)
    {
        currentAttack = value;
    }

    public void SetAttackSpeed(float value)
    {
        currentAttackSpeed = value;
    }

    public void SetMovement(float value)
    {
        currentMovement = value;
    }

    public void SetRunMovement(float value)
    {
        currentRunMovement = value;
    }
}
