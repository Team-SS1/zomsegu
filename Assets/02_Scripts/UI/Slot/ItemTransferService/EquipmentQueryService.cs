using PlayerEnum;
using ItemEnum;

public static class EquipmentQueryService
{
    // 장착한 아이템 itemId
    // 장착한 아이템의 변하지 않는 정보는 이걸로 조회
    public static int GetEquippedItemId(PlayerType playerType, EquipSlotType equipSlotType) 
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null || playerData.Equipment == null) return 0;

        EquipmentSlot equipmentSlot = playerData.Equipment.GetSlot(equipSlotType);
        if (equipmentSlot == null || equipmentSlot.isEmpty) return 0;

        return equipmentSlot.GetItemId();
    }

    // 장착한 아이템 ItemStack (인스턴스형 아이템)
    // 장착한 아이템의 변하는 정보(내구도 등)가 필요할 때 조회
    public static ItemStack GetEquippedInstance(PlayerType playerType, EquipSlotType equipSlotType) 
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if (playerData == null || playerData.Equipment == null) return null;

        EquipmentSlot equipmentSlot = playerData.Equipment.GetSlot(equipSlotType);
        if (equipmentSlot == null || equipmentSlot.isEmpty || !equipmentSlot.HasInstance || equipmentSlot.equippedItem == null) return null;

        return equipmentSlot.equippedItem;
    }

    // 해당 슬롯에 아이템이 장착되어 있는지 확인하는 편의 메서드
    public static bool HasEquippedItem(PlayerType playerType, EquipSlotType equipSlotType)
    {
        return GetEquippedItemId(playerType, equipSlotType) != 0;
    }
}
