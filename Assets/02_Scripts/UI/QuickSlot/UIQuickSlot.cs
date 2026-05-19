using UnityEngine;
using PlayerEnum;
using EventEnum;


public class UIQuickSlot : BaseUI
{
    [SerializeField] private UIActiveCharacterContext activeCharacterContext;
    [SerializeField] private UIQuickSlotSlot[] slotUIs;

    protected override void EnableInternal()
    {
        base.EnableInternal();

        EventManager.Subscribe<PlayerType>(EventKey.QuickSlotChanged, OnQuickSlotChanged);
        EventManager.Subscribe<PlayerType>(EventKey.ActiveCharacterChanged, OnActiveCharacterChanged);
        EventManager.Subscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);
        EventManager.Subscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.Subscribe<PlayerType>(EventKey.EquipmentDurabilityChanged, OnEquipmentDurabilityChanged);

        Refresh(activeCharacterContext.CurrentActivePlayer);
    }
    protected override void DisableInternal()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.QuickSlotChanged, OnQuickSlotChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.ActiveCharacterChanged, OnActiveCharacterChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.EquipmentDurabilityChanged, OnEquipmentDurabilityChanged);

        base.DisableInternal();
    }
    private void OnQuickSlotChanged(PlayerType playerType)
    {
        if (playerType != activeCharacterContext.CurrentActivePlayer) return;

        Refresh(playerType);
    }
    private void OnActiveCharacterChanged(PlayerType playerType)
    {
        Refresh(playerType);
    }
    private void OnInventoryChanged(PlayerType playerType)
    {
        if (playerType != activeCharacterContext.CurrentActivePlayer) return;

        Refresh(playerType);
    }
    private void OnEquipmentChanged(PlayerType playerType)
    {
        if (playerType != activeCharacterContext.CurrentActivePlayer) return;
        Refresh(playerType);
    }
    private void OnEquipmentDurabilityChanged(PlayerType playerType)
    {
        if (playerType != activeCharacterContext.CurrentActivePlayer) return;
        Refresh(playerType);
    }
    private void Refresh(PlayerType playerType)
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        if(playerData == null || playerData.QuickSlot == null)
        {
            ClearAll();
            return;
        }

        QuickSlot quickSlot = playerData.QuickSlot;

        for(int i = 0; i< slotUIs.Length; i++)
        {
            QuickSlotSlot slot = quickSlot.GetSlot(i);
            bool isSelected = quickSlot.SelectedIndex == i;
            slotUIs[i].SetSlot(slot, i, playerType, isSelected);
        }
    }
    private void ClearAll()
    {
        for(int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i].SetSlot(null, i, PlayerType.Player_SHIN, false);
        }
    }
}
