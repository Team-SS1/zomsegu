public class UIDialogueTopButton : BaseButton
{
    protected bool isOn;

    public override void SetState(bool active)
    {
        if (isOn == active) return;

        base.SetState(active);
        isOn = active;
    }
}
