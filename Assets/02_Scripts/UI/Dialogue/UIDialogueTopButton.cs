using System;

public class UIDialogueTopButton : BaseButton
{
    private bool isOn;
    private event Action<bool> OnButtonClicked;

    private void OnDestroy()
    {
        OnButtonClicked = null;
    }

    protected override void OnClickInternal()
    {
    }

    public override void SetState(bool active)
    {
        if (isOn == active) return;

        base.SetState(active);
        isOn = active;

        OnButtonClicked?.Invoke(active);
    }
}
