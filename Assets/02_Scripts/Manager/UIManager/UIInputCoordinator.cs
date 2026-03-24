using InputEnum;
using System;
using UnityEngine.InputSystem;

public class UIInputCoordinator
{
    private readonly UIManager ui;

    private readonly (Actions action, Action<InputAction.CallbackContext>)[] bindings;

    public UIInputCoordinator(UIManager ui, InputManager input)
    {
        this.ui = ui;

        bindings = new (Actions action, Action<InputAction.CallbackContext>)[]
        {
            (Actions.Close, OnClose)
        };

        input.AddMaps(ActionMaps.UI);
        input.BindInputs(ActionMaps.UI, bindings);
    }

    private void OnClose(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ui.CloseTopPopup();
        }
    }
}
