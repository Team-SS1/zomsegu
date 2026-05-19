using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainPanel : UIPopup
{
    [Header("Button")]
    [SerializeField] private Button closeButton;

    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        if(closeButton != null )
            closeButton.onClick.AddListener(Close);
    }
    protected override void DestroyInternal()
    {
        if(closeButton != null)
            closeButton.onClick.RemoveListener(Close);
        base.DestroyInternal();
    }
}
