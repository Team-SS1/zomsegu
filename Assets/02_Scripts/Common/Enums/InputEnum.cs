namespace InputEnum
{
    // 입력 처리 시 레이어
    [System.Flags]
    public enum InputLayer
    {
        None = 0,
        Gameplay = 1 << 0,
        UI = 1 << 1,
        System = 1 << 2,
        Cutscene = 1 << 3,
    }
}