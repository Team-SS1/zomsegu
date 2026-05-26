using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBootstrap : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.OpenUI<UIQuickSlot>();
        UIManager.Instance.OpenUI<ItemDropArea>();
        UIManager.Instance.OpenUI<UITooltipManage>();
    }
}
