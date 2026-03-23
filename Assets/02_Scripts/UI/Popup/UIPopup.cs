using System;
using UnityEngine.UI;

[UnityEngine.RequireComponent(typeof(Button))]
public abstract class UIPopup : BaseUI
{
    private Button btn;
    public event Action<UIPopup> OnUIClick;

    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    protected override void DestroyInternal()
    {
        OnUIClick = null;
        base.DestroyInternal();
    }

    private void OnClick()
    {
        OnUIClick?.Invoke(this);
    }

    protected void Close()
    {
        UIManager.Instance.ClosePopup(this);
    }
}
