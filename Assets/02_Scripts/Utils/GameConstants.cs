// 전역적으로 사용하는 상수값 정의하기 ex) String(Dialogue), Int(Item Id) 등
public static class GameConstants
{
    // 랜덤
    public static System.Random Random = new();

    public const int PlayerID_A = 100001;
    public const int PlayerID_B = 100002;

    // 오디오
    public const float DefaultVolume = 0.8f;
}