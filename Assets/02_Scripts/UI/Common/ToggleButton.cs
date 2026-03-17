public class ToggleButton : BaseButton
{
    protected bool isOn;

    protected override void OnClickInternal()
    {
        SetState(!isOn);
    }

    public override void SetState(bool active)
    {
        if (isOn == active) return;

        base.SetState(active);
        isOn = active;
    }
}
