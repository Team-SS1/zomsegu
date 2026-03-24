using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZombieHitBodyPartUtility
{
    // 좀비 공격 시 확률적으로 맞추는 부위
    public static ArmorType GetRandomPart()
    {
        float r = Random.value;

        if (r < 0.3f) return ArmorType.Head;
        if (r < 0.8f) return ArmorType.Body;
        return ArmorType.Leg;
    }
}