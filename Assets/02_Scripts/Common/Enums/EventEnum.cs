namespace EventEnum
{
    public enum EventKey
    {
        /* ----- Item, Inventory, Stat ----- */
        InventoryChanged,
        EquipmentChanged, // 장착, 해제, 교체
        EquipmentDurabilityChanged, // 내구도 수치 변경
        QuickSlotChanged,
        DropItemChanged,
        StatChanged,
        ActiveCharacterChanged, // 플레이 캐릭터 변경
        InspectCharacterChanged, // 그냥 캐릭터 정보창 변경
        GamePlayTypeChanged, // 게임 플레이 타입 변경

        /* ----- Spawn ----- */
        PlayerSpawned, // 플레이어 스폰시 위치 전달용(월드 드롭)
        WorldDropRequested,

        /* ----- Player Condition ----- */
        OnHungerChanged,
        OnThirstChanged,
        OnShockChanged,
        OnInjuryChanged,
        OnStaminaChanged,
        OnTiredChanged,

    }
}