using ItemEnum;
using PlayerEnum;
public static class ItemTransferService
{
    public static bool TryTransferBetweenSlots(DragPayload payload) // 슬롯 간 이동
    {
        if (payload == null || !payload.hasTo) return false;

        SlotRef from = payload.from;
        SlotRef to = payload.to;

        if (from.playerType != to.playerType) return false; // 플레이어 간 이동은 원래도 안되지만 일단 여기서도 막아둠

        if (from.slotType == SlotType.Inventory && to.slotType == SlotType.Inventory) // 인벤 -> 인벤 이동
            return InventoryTransferService.TryInventoryToInventory(from, to);

        if (from.slotType == SlotType.Inventory && to.slotType == SlotType.Equipment) // 인벤 -> 장비 이동
            return EquipmentTransferService.TryInventoryToEquipment(from, to, true);

        if (from.slotType == SlotType.Equipment && to.slotType == SlotType.Inventory) // 장비 -> 인벤 이동
            return EquipmentTransferService.TryEquipmentToInventory(from, to);

        if (from.slotType == SlotType.Inventory && to.slotType == SlotType.QuickSlot) // 인벤 -> 퀵슬롯 이동
            return QuickSlotService.TryInventoryToQuickSlot(from, to);

        if (from.slotType == SlotType.QuickSlot && to.slotType == SlotType.QuickSlot) // 퀵슬롯 -> 퀵슬롯 이동
            return QuickSlotService.TryQuickSlotToQuickSlot(from, to);

        if (from.slotType == SlotType.DropItem && to.slotType == SlotType.Inventory) // 드롭아이템 -> 인벤 이동
            return DropTransferService.TryDropItemToInventory(from, to);

        return false;
    }

    // 슬롯에서 다른 플레이어에게 주기
    public static bool TryGiveToOtherPlayer(SlotRef from, PlayerType targetPlayer) // 인스턴스형 아이템
    {
        return InventoryTransferService.TryGiveToOtherPlayer(from, targetPlayer, -1);
    }
    public static bool TryGiveToOtherPlayer(SlotRef from, PlayerType targetPlayer, int amount) //스택형 아이템
    {
        return InventoryTransferService.TryGiveToOtherPlayer(from, targetPlayer, amount);
    }

    // 슬롯에서 외부로 드롭
    public static bool TryDropOutside(SlotRef from) // 인스턴스형 아이템
    {
        //return DropTransferService.TryDropOutside(from, -1);
        return true;
    }
    public static bool TryDropOutside(SlotRef from, int amount) // 스택형 아이템
    {
        //return DropTransferService.TryDropOutside(from, amount);
        return true;
    }

    public static bool TryUnEquipToFirstEmptyInventory(SlotRef from) // 아이템 장착 해제 (인벤 맨 앞 빈칸으로)
    {
        return EquipmentTransferService.TryUnEquipToFirstEmptyInventory(from);
    }
    public static bool TryClearQuickSlot(SlotRef from) // 퀵슬롯 등록 해제
    {
        return QuickSlotService.TryClearQuickSlot(from);
    }
    public static void ValidateQuickSlots(PlayerType playerType) // 퀵슬롯에 아이템이 존재하는 플레이어한테 존재하는 아이템인지 확인
    {
        QuickSlotService.ValidateQuickSlots(playerType);
    }
    public static bool TrySelectQuickSlot(PlayerType playerType, int quickSlotIndex) // 퀵슬롯 선택하기
    {
        return QuickSlotService.TrySelectQuickSlot(playerType, quickSlotIndex);
    }
    public static bool TryUseOrEquipFromInventory(SlotRef from) // 인벤 아이템 사용 또는 장착하기 (퀵슬롯 기능에서 사용)
    {
        return ItemUseService.TryUseOrEquipFromInventory(from);
    }
}