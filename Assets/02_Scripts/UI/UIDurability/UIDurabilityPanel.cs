using UnityEngine;
using PlayerEnum;
using EventEnum;
using UnityEngine.UI;
using Unity.VisualScripting;
public class UIDurabilityPanel : BaseUI
{
    [Header("Buttons")]
    [SerializeField] private Button openInventoryButton;
    [SerializeField] private Button closeButton;

    [Header("Slots")]
    [SerializeField] private UIDurabilitySlot weaponSlot;
    [SerializeField] private UIDurabilitySlot headSlot;
    [SerializeField] private UIDurabilitySlot bodySlot;
    [SerializeField] private UIDurabilitySlot legSlot;

    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        if (openInventoryButton != null)
            openInventoryButton.onClick.AddListener(OnClickOpenInventory);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }
    protected override void DestroyInternal()
    {
        if (openInventoryButton != null)
            openInventoryButton.onClick.RemoveListener(OnClickOpenInventory);

        if(closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        base.DestroyInternal();
    }
    protected override void EnableInternal()
    {
        base.EnableInternal();

        EventManager.Subscribe<PlayerType>(EventKey.ActiveCharacterChanged, OnActiveCharacterChanged);
        EventManager.Subscribe<PlayerType>(EventKey.EquipmentDurabilityChanged, OnEquipmentChanged);
        EventManager.Subscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);

        Refresh(PlayerManager.Instance.CurrentActivePlayer);
    }
    protected override void DisableInternal()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.ActiveCharacterChanged, OnActiveCharacterChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.EquipmentDurabilityChanged, OnEquipmentChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);

        base.DisableInternal();
    }
    private void OnActiveCharacterChanged(PlayerType playerType)
    {
        Refresh(playerType);
    }
    private void OnEquipmentChanged(PlayerType playerType)
    {
        if (playerType != PlayerManager.Instance.CurrentActivePlayer) return;

        Refresh(playerType);
    }
    private void Refresh(PlayerType playerType)
    {
        if (weaponSlot != null) weaponSlot.Refresh(playerType);
        if(headSlot != null) headSlot.Refresh(playerType);
        if(bodySlot != null) bodySlot.Refresh(playerType);
        if(legSlot != null) legSlot.Refresh(playerType);
    }
    private void OnClickOpenInventory()
    {
        //이따가 만들 예정
    }
    private void Close()
    {
        gameObject.SetActive(false);
    }
}
