// Enum 추가시 간단한 주석 추가하기
namespace MonsterEnum
{
    public enum AttackType // 몬스터의 공격 유형을 정의 현재는 좀비만 추가 (추후 다른 몬스터 추가 시 확장 가능)
    {
        Scratch,
        Bite
    }

    public enum ZombieType
    {
        ElderFemale,
        ElderMale,
        AdultFemale,
        AdultMale,
        YoungAdultFemale,
        YoungAdultMale,
        Athlete,
        Police,
        Soldier,
        Firefighter
    }

    public enum ZombieStateType
    {
        Idle,
        Investigate,
        Aggro,
        Dead
    }

    public enum ArmorType
    {
        Head,
        Body,
        Leg
    }
}