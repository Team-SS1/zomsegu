namespace InputEnum
{
    // 입력 처리 시 레이어
    [System.Flags]
    public enum ActionMaps
    {
        None = 0,
        Gameplay = 1 << 0,
        UI = 1 << 1,
        System = 1 << 2,
        Cutscene = 1 << 3,
    }

    public enum Actions
    {
        // === Gameplay ===
        Move,
        Run,
        QuietWalk,
        Attack,
        QuickSlot1,
        QuickSlot2,
        QuickSlot3,
        QuickSlot4,
        QuickSlot5,
        TemporaryDwelling,
        Check,

        // === UI ===
        Inventory,

        // === Dialogue ===
        Advance,
        Navigate,
        Select,

        // === System ===
        StopGame,
    }
}