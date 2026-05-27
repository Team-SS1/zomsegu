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

    private UIMainPanelFlowController flowController;
    public void BindFlowController(UIMainPanelFlowController flowController)
    {
        this.flowController = flowController;
    }
    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        if (openInventoryButton != null)
            openInventoryButton.onClick.AddListener(OnClickOpenInventory);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickClose);
    }
    protected override void DestroyInternal()
    {
        if (openInventoryButton != null)
            openInventoryButton.onClick.RemoveListener(OnClickOpenInventory);

        if(closeButton != null)
            closeButton.onClick.RemoveListener(OnClickClose);

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
    public void RefreshByActivePlayer()
    {
        if (PlayerManager.Instance == null) return;

        Refresh(PlayerManager.Instance.CurrentActivePlayer);
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
        if (flowController != null)
            flowController.OnDurabilityPlusButtonClicked();
    }
    private void OnClickClose()
    {
        if (flowController != null)
            flowController.OnDurabilityCloseButtonClicked();
        else
            ClosePanel();
    }
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
