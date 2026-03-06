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
}