namespace InputEnum
{
    public enum InputMode
    {
        Default,
        Gameplay,
        Dialogue,
        Modal,
        Cutscene,
    }

    // 입력 처리 시 레이어
    [System.Flags]
    public enum ActionMaps
    {
        None = 0,
        Gameplay = 1 << 0,
        UI = 1 << 1,
        Dialogue = 1 << 2,
        System = 1 << 3,
        Cutscene = 1 << 4,
    }

    public enum Actions
    {
        // === Gameplay ===
        Move,
        Run,
        Crouch,
        Attack,
        Interact,
        ExtraInteract,
        Reload,
        Aim,
        QuickSlot1,
        QuickSlot2,
        QuickSlot3,
        QuickSlot4,
        QuickSlot5,
        TemporaryShelter_Sleep,
        Check,
        Zoom,

        // === UI ===
        Inventory,
        Phone,
        Diary,
        Close,
        MiniMenu,
        Map,

        // === Dialogue ===
        Next,
        Previous,
        Navigate,
        Skip,
        AllSkip,
        Auto,
        Backlog,

        // === Common ===
        Submit,     // UI & Dialogue

        // === System ===
        StopGame,
    }
}