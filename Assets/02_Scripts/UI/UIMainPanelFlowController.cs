using UnityEngine;
using PlayerEnum;
using EventEnum;
using UIEnum;
using Unity.VisualScripting;

public class UIMainPanelFlowController : MonoBehaviour
{
    private UIMainPanel mainPanel;
    private UIDurabilityPanel durabilityPanel;

    private UIMainPanelFlowState currentState = UIMainPanelFlowState.None;

    public void OnInventoryInput() // tab
    {
        switch (currentState)
        {
            case UIMainPanelFlowState.None:
                OpenMainPanel();
                break;
            case UIMainPanelFlowState.MainPanelOepn:
                CloseMainAndOpenDurability();
                break;
            case UIMainPanelFlowState.DurabilityPanelOepn:
                CloseDurabilityAndOpenMain();
                break;
        }
    }
    public void OnCloseInput() // esc
    {
        switch (currentState)
        {
            case UIMainPanelFlowState.MainPanelOepn:
                CloseMainAndOpenDurability();
                break;
            case UIMainPanelFlowState.DurabilityPanelOepn:
                // 내구도 장비창 안닫고 기존에 창 열려 있으면 그거 닫거나 정지 화면 띄움
                break;
            case UIMainPanelFlowState.None:
                //이것도 기존에 창 열려 있으면 그거 닫거나 정지 화면 띄움
                break;
        }
    }
    public void OnMiniBarMainPanelButtonClicked() //미니바 메인 패널 버튼용
    {
        OnInventoryInput();
    }
    public void OnMainPanelCloseButtonClicked() // 메인 패널 닫기 버튼
    {
        CloseMainAndOpenDurability();
    }
    public void OnDurabilityPlusButtonClicked() // 내구도 확인창 + 버튼
    {
        CloseDurabilityAndOpenMain();
    }
    public void OnDurabilityCloseButtonClicked() // 내구도확인창 닫기 버튼
    {
        CloseDurabilityOnly();
    }
    private void OpenMainPanel()
    {
        PlayerType playerType = PlayerManager.Instance.CurrentActivePlayer;

        SyncInspectPlayerToActivePlayer(playerType);

        mainPanel = UIManager.Instance.OpenUI<UIMainPanel>();

        if (mainPanel != null)
            mainPanel.BindFlowController(this);

        currentState = UIMainPanelFlowState.MainPanelOepn;
    }
    private void CloseMainAndOpenDurability()
    {
        if (mainPanel != null)
            mainPanel.Close();

        OpenDurabilityPanel();

        currentState = UIMainPanelFlowState.DurabilityPanelOepn;
    }
    private void OpenDurabilityPanel()
    {
        durabilityPanel = UIManager.Instance.OpenUI<UIDurabilityPanel>();

        if(durabilityPanel != null)
        {
            durabilityPanel.BindFlowController(this);
            durabilityPanel.RefreshByActivePlayer();
        }
    }
    private void CloseDurabilityAndOpenMain()
    {
        if (durabilityPanel != null)
            durabilityPanel.ClosePanel();

        OpenMainPanel();

        currentState = UIMainPanelFlowState.MainPanelOepn;
    }
    private void CloseDurabilityOnly()
    {
        if (durabilityPanel != null)
            durabilityPanel.ClosePanel();

        currentState = UIMainPanelFlowState.None;
    }
    private void SyncInspectPlayerToActivePlayer(PlayerType activePlayer)
    {
        EventManager.TriggerEvent<PlayerType>(EventKey.InspectCharacterChanged, activePlayer);

    }
}
