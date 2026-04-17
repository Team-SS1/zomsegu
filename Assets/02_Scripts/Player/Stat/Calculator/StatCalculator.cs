using UnityEngine;

public static class StatCalculator
{
    public static float CalculateAttack(AttackInput input)
    {
        float playerBaseAttack =
            (input.baseAttack
            + input.weaponAttack
            + input.consumeBuff
            + input.equipAttack
            + input.otherBuffAddition)
            * input.otherBuffMultiplication;

        float finalPainDebuff = input.painDebuff * (1f - input.painkillerBuff);
        float totalDebuff = input.conditionDebuff + finalPainDebuff;

        totalDebuff = Mathf.Clamp01(totalDebuff);

        float finalAttack = playerBaseAttack * (1f - totalDebuff);

        return Mathf.Max(input.minAttack, finalAttack);
    }

    public static float CalculateAttackSpeed(AttackSpeedInput input)
    {
        float playerBaseAttackSpeed =
            (input.weaponAttackSpeed - input.consumeBuff)
            - input.otherBuff;

        float finalPainDebuff = input.painDebuff * (1f - input.painkillerBuff);
        float finalAttackSpeed =
            (playerBaseAttackSpeed - input.conditionDebuff)
            - finalPainDebuff;

        finalAttackSpeed = Mathf.Min(input.minAttackSpeed, finalAttackSpeed);
        return Mathf.Max(0f, finalAttackSpeed);
    }

    public static float CalculateMovement(MovementInput input)
    {
        float playerBaseMovement =
            (input.baseMovement
            + input.equipMovement
            - input.inventoryWeight
            + input.consumeBuff
            + input.otherBuffAddition)
            * input.otherBuffMultiplication;

        float finalPainDebuff = input.painDebuff * (1f - input.painkiller);
        float totalDebuff = input.conditionDebuff + finalPainDebuff;

        totalDebuff = Mathf.Clamp01(totalDebuff);

        float finalMovement = playerBaseMovement * (1f - totalDebuff);

        return Mathf.Max(input.minMovement, finalMovement);
    }

    public static float CalculateRunMovement(MovementInput input)
    {
        return CalculateMovement(input) * 1.5f;
    }
}