public class UIDialogueTopButton : BaseButton
{
    private bool isOn;

    protected override void OnClickInternal() { }

    public override void SetState(bool active)
    {
        if (isOn == active) return;

        base.SetState(active);
        isOn = active;
    }
}
