using InputEnum;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputHandler
{
    private InputActionMap map;
    private readonly Dictionary<InputAction, Action<InputAction.CallbackContext>> bindings = new();

    #region 생명주기
    public InputHandler(InputActionMap map)
    {
        this.map = map;
    }

    public void Dispose()
    {
        foreach (var kvp in bindings)
        {
            kvp.Key.started -= kvp.Value;
            kvp.Key.performed -= kvp.Value;
            kvp.Key.canceled -= kvp.Value;
        }
        bindings.Clear();

        map.Disable();
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

        // 기존 바인딩이 있으면 교체(중복 호출 방지)
        if (bindings.TryGetValue(inputAction, out var prev))
        {
            inputAction.started -= prev;
            inputAction.performed -= prev;
            inputAction.canceled -= prev;
        }

        bindings[inputAction] = action;

        inputAction.started += action;
        inputAction.performed += action;
        inputAction.canceled += action;
    }

    /// <summary>
    /// 특정 액션의 바인딩 오버라이드 적용. 
    /// bindingIndex는 InputAction의 bindings 리스트에서 몇 번째 바인딩을 변경할지 지정. 
    /// 기본값은 0(첫 번째 바인딩)
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="newPath"></param>
    /// <param name="bindingIndex"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ApplyBindingOverride(Actions actions, string newPath, int bindingIndex = 0)
    {
        InputAction inputAction = FindAction(actions);

        if (bindingIndex < 0 || bindingIndex >= inputAction.bindings.Count)
            throw new ArgumentOutOfRangeException(nameof(bindingIndex),
                $"bindingIndex 범위 오류: {inputAction.name} bindings.Count={inputAction.bindings.Count}, index={bindingIndex}");

        inputAction.ApplyBindingOverride(bindingIndex, newPath);
    }

    /// <summary>
    /// 전체 바인딩 오버라이드 제거. ApplyBindingOverride로 변경한 키를 원래대로 되돌릴 때 사용
    /// </summary>
    /// <param name="inputAction"></param>
    public void RemoveAllBindingOverrides(InputAction inputAction)
    {
        inputAction.RemoveAllBindingOverrides();
    }
    #endregion

    #region InputActionMap 제어
    public void Enable()
    {
        if (!map.enabled) map.Enable();
    }

    public void Disable()
    {
        if (map.enabled) map.Disable();
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
