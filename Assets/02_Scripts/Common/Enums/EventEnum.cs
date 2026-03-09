namespace EventEnum
{
    public enum EventKey
    {
        /* ----- Item, Inventory, Stat ----- */
        InventoryChanged,
        EquipmentChanged,
        QuickSlotChanged,
        DropItemChanged,
        StatChanged,
        ActiveCharacterChanged, // 플레이 캐릭터 변경
        InspectCharacterChanged, // 그냥 캐릭터 정보창 변경

        /* ----- Player Condition ----- */
        OnHungerChanged,
        OnThirstChanged,
        OnShockChanged,
        OnInjuryChanged,
        OnStaminaChanged,
        OnTiredChanged,

    }
}