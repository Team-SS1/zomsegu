using MonsterEnum;
using UnityEngine;

[System.Serializable]
public class WildDogStats
{
    public WildDogType DogType;

    public string Name;
    public string Description;

    public float MaxHP;
    public float MoveSpeed;
    public float RunSpeedMultiplier;
    public float AttackDamage;
    public float Defense;

    public float AttackDuration;

    public float AggroLoseDistance;
    public float AggroLoseTime;

    public float VisionFrontRadius360;
    public float VisionFrontFOVRadius;
    public float VisionFrontFOVAngle;

    public float StaggerResistance;

    // 공격 시작 범위
    public float AttackStartRange;

    // 실제 히트 판정 범위
    public float BiteRange;

    public float AttackResumeChaseRange;

    public static WildDogStats CreateLeader()
    {
        return new WildDogStats
        {
            DogType = WildDogType.Leader,
            Name = "Leader Wild Dog",
            Description = "대장 들개",
            MaxHP = 250f,
            MoveSpeed = 100f,
            RunSpeedMultiplier = 1.5f,
            AttackDamage = 5f,
            Defense = 0f,
            AttackDuration = 0.8f,
            AggroLoseDistance = 12f,
            AggroLoseTime = 10f,
            VisionFrontRadius360 = 10f,
            VisionFrontFOVRadius = 20f,
            VisionFrontFOVAngle = 150f,
            StaggerResistance = 0.3f,
            AttackStartRange = 0.15f,
            BiteRange = 0.25f,
            AttackResumeChaseRange = 0.45f
        };
    }

    public static WildDogStats CreateGrunt()
    {
        return new WildDogStats
        {
            DogType = WildDogType.Grunt,
            Name = "Grunt Wild Dog",
            Description = "졸병 들개",
            MaxHP = 160f,
            MoveSpeed = 90f,
            RunSpeedMultiplier = 1.5f,
            AttackDamage = 3f,
            Defense = 0f,
            AttackDuration = 0.9f,
            AggroLoseDistance = 12f,
            AggroLoseTime = 7f,
            VisionFrontRadius360 = 8f,
            VisionFrontFOVRadius = 15f,
            VisionFrontFOVAngle = 135f,
            StaggerResistance = 0.2f,
            AttackStartRange = 0.15f,
            BiteRange = 0.25f,
            AttackResumeChaseRange = 0.45f
        };
    }
}