using MonsterEnum;
using UnityEngine;

public static class WildDogHitPartUtility
{
    public static ArmorType GetRandomArmorPart()
    {
        float r = Random.value;

        if (r < 0.2f) return ArmorType.Head;
        if (r < 0.5f) return ArmorType.Body;
        return ArmorType.Leg;
    }
}