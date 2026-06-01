// Enum 추가시 간단한 주석 추가하기
namespace PlayerEnum
{
    public enum PlayerType
    {
        Player_SHIN,
        Player_HAN
    }

    public enum AbnormalType
    {
        Hunger,
        Thirst,
        Shock,
        Tired,
        Injury,
        CriticalInjury, // 심한
        Stun // 행불
    }

    public enum AbnormalDebuffType
    {
        Attack,
        AttackSpeed,
        MoveSpeed,
        Stamina
    }

    public enum SpriteType
    {
        Punch,
        Bat,
        Blunt,
        Hammer,
        Knife
    }
    public enum GamePlayType
    {
        PlayMain,
        PlaySub,
        PlayBoth
    }
}
