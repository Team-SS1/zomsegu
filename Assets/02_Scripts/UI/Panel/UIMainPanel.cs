using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainPanel : UIPopup
{
    [Header("Button")]
    [SerializeField] private Button closeButton;
    private UIMainPanelFlowController flowController;

    public void BindFlowController(UIMainPanelFlowController flowController)
    {
        this.flowController = flowController;
    }
    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        if(closeButton != null )
            closeButton.onClick.AddListener(OnClickClose);
    }
    protected override void DestroyInternal()
    {
        if(closeButton != null)
            closeButton.onClick.RemoveListener(OnClickClose);
        base.DestroyInternal();
    }
    private void OnClickClose()
    {
        if (flowController != null)
            flowController.OnMainPanelCloseButtonClicked();
        else
            Close();
    }
}
