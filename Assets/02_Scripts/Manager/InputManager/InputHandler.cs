using InputEnum;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputHandler
{
    private InputActionMap map;
    private Dictionary<InputAction, Action<InputAction.CallbackContext>> bindings;

    #region 생명주기
    public InputHandler(InputActionMap map)
    {
        this.map = map;
        bindings = new();
    }

    public void Dispose()
    {
        foreach (var kvp in bindings)
        {
            kvp.Key.performed -= kvp.Value;
        }
        bindings.Clear();
    }
    #endregion

    #region 바인딩 관리
    /// <summary>
    /// InputManager에서 사용하는 함수 바인딩
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="action"></param>
    public void BindInput(Actions actions, Action<InputAction.CallbackContext> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action), "바인딩할 함수가 null 입니다");
        }

        InputAction inputAction = FindAction(actions);

        if (bindings.TryGetValue(inputAction, out var prev))
        {
            inputAction.performed -= prev;
        }

        bindings[inputAction] = action;
        inputAction.performed += action;
    }

    public void ApplyBindingOverride(Actions actions, string newPath)
    {
        InputAction inputAction = FindAction(actions);
        RemoveAllBindingOverrides(inputAction); // 기존 바인딩 제거
        inputAction.ApplyBindingOverride(0, newPath);
    }

    private void RemoveAllBindingOverrides(InputAction inputAction)
    {
        inputAction.RemoveAllBindingOverrides();
    }
    #endregion

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

    #region Utils
    private InputAction FindAction(Actions actions)
    {
        InputAction inputAction = map.FindAction(actions.ToString())
            ?? throw new InvalidOperationException($"{map.name}.{actions} 없음");
        return inputAction;
    }
    #endregion
}
