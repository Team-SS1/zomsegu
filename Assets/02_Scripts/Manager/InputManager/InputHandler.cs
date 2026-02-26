using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputHandler
{
    private InputActionMap map;
    private Dictionary<InputAction, Action<InputAction.CallbackContext>> bindings;

    #region 생성자 & 소멸자
    public InputHandler(InputActionMap map)
    {
        this.map = map;
        bindings = new();
    }

    ~InputHandler()
    {
        foreach (var kvp in bindings)
        {
            kvp.Key.performed -= kvp.Value;
        }
    }
    #endregion

    /// <summary>
    /// InputManager에서 사용하는 함수 바인딩
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="action"></param>
    public void BindInput(InputEnum.Actions actions, Action<InputAction.CallbackContext> action)
    {
        InputAction inputAction = map.FindAction(actions.ToString());

        if (bindings.ContainsKey(inputAction))
        {
            inputAction.performed -= bindings[inputAction];
            bindings[inputAction] = action;
        }
        else
        {
            bindings.Add(inputAction, action);
        }

        inputAction.performed += bindings[inputAction];
    }

    public void ApplyBindingOverride(InputEnum.Actions actions, string newPath)
    {
        InputAction inputAction = map.FindAction(actions.ToString());
        inputAction.ApplyBindingOverride(newPath);
    }

    #region InputActionMap 제어
    public void Enable()
    {
        map.Enable();
    }

    public void Disable()
    {
        map.Disable();
    }
    #endregion
}
