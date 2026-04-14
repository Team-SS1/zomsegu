using UnityEngine;

[System.Serializable]
public class MovementInput
{
    public float baseMovement;
    public float equipMovement;
    public float inventoryWeight;
    public float consumeBuff;
    public float OtherBuffAddition;
    public float otherBuffMultiplication = 1f;

    public float conditionDebuff;
    public float painDebuff;
    public float painkiller;

    public float minMovement = 50f;
}
