using System.Collections;
using System.Collections.Generic;
using PlayerEnum;
using UnityEngine;

public class PlayerManager : GlobalSingleton<PlayerManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    [Header("Current Active Player")]
    private PlayerType currentActivePlayer;

    [Header("Attack Value")]
    private float equipWeaponAtk;
    private float consumeBuffAtk;
    private float otherBuffAtk1;
    private float otherBuffAtk2 = 1f;

    [Header("AttackSpeed Value")]
    private float weaponAttackSpeed;
    private float consumeBuffAtkSpeed;
    private float otherBuffAtkSpeed;

    [Header("Movement Value")]
    private float equipMovement;
    private float inventoryWeight;
    private float consumeBuffMovement;
    private float otherBuffMovement1;
    private float otherBuffMovement2 = 1f;

    public PlayerType CurrentActivePlayer => currentActivePlayer;
    public void SetWeaponAttack(float atk) => equipWeaponAtk = atk;
    public void SetConsumeBuffAttack(float add) => consumeBuffAtk = add;
    public void SetOtherBuffAttackAdd(float add) => otherBuffAtk1 = add;
    public void SetOtherBuffAttackMul(float mul) => otherBuffAtk2 = mul;

    public void SetWeaponAttackSpeed(float value) => weaponAttackSpeed = value;
    public void SetConsumeBuffAttackSpeed(float value) => consumeBuffAtkSpeed = value;
    public void SetOtherBuffAttackSpeed(float value) => otherBuffAtkSpeed = value;

    public void SetEquipMovement(float value) => equipMovement = value;
    public void SetInventoryWeight(float value) => inventoryWeight = value;
    public void SetConsumeBuffMovement(float value) => consumeBuffMovement = value;
    public void SetOtherBuffMovementAdd(float value) => otherBuffMovement1 = value;
    public void SetOtherBuffMovementMul(float value) => otherBuffMovement2 = value;

    public void UpdateAll(Player player)
    {
        if (player == null)
            return;

        UpdateAttack(player);
        UpdateAttackSpeed(player);
        UpdateMovement(player);
    }

    public void UpdateAttack(Player player)
    {
        if (player == null)
            return;

        PlayerCondition condition = player.GetComponent<PlayerCondition>();
        if (condition == null)
            return;

        AttackInput input = new AttackInput
        {
            baseAttack = player.data != null ? player.data.Stat.BaseAttack : 0f,
            weaponAttack = equipWeaponAtk,
            consumeBuff = consumeBuffAtk,
            otherBuffAddition = otherBuffAtk1,
            otherBuffMultiplication = otherBuffAtk2,

            conditionDebuff = GetAttackConditionDebuff(condition),
            painDebuff = GetAttackPainDebuff(condition),
            painkillerBuff = GetPainkillerBuff(),

            minAttack = 30f
        };

        float result = StatCalculator.CalculateAttack(input);
        player.SetAttack(result);
    }

    public void UpdateAttackSpeed(Player player)
    {
        if (player == null)
            return;

        PlayerCondition condition = player.GetComponent<PlayerCondition>();
        if (condition == null)
            return;

        AttackSpeedInput input = new AttackSpeedInput
        {
            weaponAttackSpeed = weaponAttackSpeed,
            consumeBuff = consumeBuffAtkSpeed,
            otherBuff = otherBuffAtkSpeed,

            conditionDebuff = GetAttackSpeedConditionDebuff(condition),
            painDebuff = GetAttackSpeedPainDebuff(condition),
            painkillerBuff = GetPainkillerBuff(),

            minAttackSpeed = 2f
        };

        float result = StatCalculator.CalculateAttackSpeed(input);
        player.SetAttackSpeed(result);
    }

    public void UpdateMovement(Player player)
    {
        if (player == null)
            return;

        PlayerCondition condition = player.GetComponent<PlayerCondition>();
        if (condition == null)
            return;

        MovementInput input = new MovementInput
        {
            baseMovement = player.data != null ? player.data.Stat.BaseMovement : 0f,
            equipMovement = equipMovement,
            inventoryWeight = inventoryWeight,
            consumeBuff = consumeBuffMovement,
            otherBuffAddition = otherBuffMovement1,
            otherBuffMultiplication = otherBuffMovement2,

            conditionDebuff = GetMovementConditionDebuff(condition),
            painDebuff = GetMovementPainDebuff(condition),
            painkiller = GetPainkillerBuff(),

            minMovement = 50f
        };

        float result = StatCalculator.CalculateMovement(input);
        float runResult = StatCalculator.CalculateRunMovement(input);

        player.SetMovement(result);
        player.SetRunMovement(runResult);
    }

    private float GetAttackConditionDebuff(PlayerCondition condition)
    {
        return condition.GetDebuff(AbnormalType.Hunger, AbnormalDebuffType.Attack);
    }

    private float GetAttackPainDebuff(PlayerCondition condition)
    {
        float shock = condition.GetDebuff(AbnormalType.Shock, AbnormalDebuffType.Attack);
        float injury = condition.GetDebuff(AbnormalType.Injury, AbnormalDebuffType.Attack);
        return shock + injury;
    }

    private float GetAttackSpeedConditionDebuff(PlayerCondition condition)
    {
        float hunger = condition.GetDebuff(AbnormalType.Hunger, AbnormalDebuffType.AttackSpeed);
        float thirst = condition.GetDebuff(AbnormalType.Thirst, AbnormalDebuffType.AttackSpeed);
        return hunger + thirst;
    }

    private float GetAttackSpeedPainDebuff(PlayerCondition condition)
    {
        float shock = condition.GetDebuff(AbnormalType.Shock, AbnormalDebuffType.AttackSpeed);
        float injury = condition.GetDebuff(AbnormalType.Injury, AbnormalDebuffType.AttackSpeed);
        return shock + injury;
    }

    private float GetMovementConditionDebuff(PlayerCondition condition)
    {
        return condition.GetDebuff(AbnormalType.Thirst, AbnormalDebuffType.MoveSpeed);
    }

    private float GetMovementPainDebuff(PlayerCondition condition)
    {
        return condition.GetDebuff(AbnormalType.Injury, AbnormalDebuffType.MoveSpeed);
    }

    private float GetPainkillerBuff()
    {
        return 0f;
    }
}
