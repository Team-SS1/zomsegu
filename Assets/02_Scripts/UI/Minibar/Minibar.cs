using UnityEngine;
public class Minibar : BaseUI
{
    [SerializeField] private MiniBarBtn miniBarBtn;

    public void BindFlowController(UIMainPanelFlowController flowController)
    {
        if (miniBarBtn != null)
        {
            miniBarBtn.BindFlowController(flowController);
        }
    }
}
