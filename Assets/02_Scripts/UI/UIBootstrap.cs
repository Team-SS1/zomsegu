using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBootstrap : MonoBehaviour
{
    [SerializeField] private UIMainPanelFlowController flowController;
    void Start()
    {
        UIManager.Instance.OpenUI<UIQuickSlot>();
        UIManager.Instance.OpenUI<ItemDropArea>();

        UITooltipManage tooltipmanager = UIManager.Instance.OpenUI<UITooltipManage>();
        flowController.BindTooltipmanager(tooltipmanager);

        Minibar miniBar = UIManager.Instance.OpenUI<Minibar>();
        miniBar.BindFlowController(flowController);
    }
}
