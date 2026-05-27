// Enum 추가시 간단한 주석 추가하기
namespace MonsterEnum
{
    public enum AttackType // 몬스터의 공격 유형을 정의 현재는 좀비만 추가 (추후 다른 몬스터 추가 시 확장 가능)
    {
        Scratch,
        Bite
    }

    public enum ZombieType // 좀비의 유형을 정의
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

    public enum ZombieStateType // 좀비의 상태를 정의
    {
        Idle,
        Investigate,
        Aggro,
        Dead
    }

    public enum ArmorType // 공격이 명중한 부위를 정의
    {
        Head,
        Body,
        Leg
    }

    public enum WildDogType
    {
        Leader,
        Grunt
    }

    public enum WildDogStateType
    {
        Idle,
        Move,
        Aggro,
        Flee,
        Dead
    }

    public enum WildDogAggroTargetKind
    {
        None,
        Character,
        Sound
    }

    public enum WildDogIdleActionType
    {
        WalkRandom,
        RunRandom,
        Howling
    }
}