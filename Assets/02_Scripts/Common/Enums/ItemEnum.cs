// Enum 추가시 간단한 주석 추가하기
namespace ItemEnum 
{
    public enum ItemType
    {
        Head,
        Body,
        Leg,
        Shoes,
        Bag,
        Weapon,
        Accessory,
        Consumable,
        Misc
    }
    public enum EquipSlotType
    {
        Head,
        Body,
        Leg,
        Shoes,
        Bag,
        Weapon,
        Accessory1,
        Accessory2
    }
    public enum SlotType
    {
        Inventory,
        Equipment,
        QuickSlot,
        DropItem
    }
    public enum SlotKeyType
    {
        Index,
        EquipSlot
    }
    public enum ItemAmountPopupMode
    {
        None,
        GiveToOtherPlayer,
        DropToWorld
    }
    public enum InventoryFilterType
    {
        All,
        Weapon,
        Equipment,
        Accessory,
        Consumable,
        Misc
    }
    public enum LootSourceType // 아이템이 생성되는 출처
    {
        GroundScan,
        Container,
    }
    public enum DurabilityDamageResult
    {
        None,
        Damaged,
        Broken
    }
}