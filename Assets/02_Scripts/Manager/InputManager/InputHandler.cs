using InputEnum;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputBindingEntry
{
    private InputAction inputAction;
    private Action<InputAction.CallbackContext> action;
    private bool isLocked;

    public InputAction InputAction => inputAction;
    public bool IsLocked => isLocked;

    public InputBindingEntry(
        InputAction inputAction,
        Action<InputAction.CallbackContext> action,
        bool isLocked)
    {
        this.inputAction = inputAction;
        this.action = action;
        this.isLocked = isLocked;
    }

    public void UnbindInput()
    {
        inputAction.started -= action;
        inputAction.performed -= action;
        inputAction.canceled -= action;
    }

    public void BindInput(Action<InputAction.CallbackContext> action)
    {
        UnbindInput();      // 기존 바인딩 제거
        this.action = action;
        inputAction.started += action;
        inputAction.performed += action;
        inputAction.canceled += action;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }
}

public class InputHandler
{
    private InputActionMap map;
    private readonly Dictionary<Actions, InputBindingEntry> bindings = new();

    #region 생명주기
    public InputHandler(InputActionMap map)
    {
        this.map = map;
    }

    public void Dispose()
    {
        foreach (var value in bindings.Values)
        {
            value.UnbindInput();
        }
        bindings.Clear();

        map.Disable();
    }
    #endregion

    #region 인풋 관리
    /// <summary>
    /// InputManager에서 사용하는 함수 바인딩
    /// </summary>
    public void BindInput(Actions actions, Action<InputAction.CallbackContext> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action), "바인딩할 함수가 null 입니다");
        }

        if (!bindings.TryGetValue(actions, out var entry))
        {
            if (!TryGetInputAction(actions, out InputAction inputAction)) return;

            entry = new InputBindingEntry(inputAction, action, false);
            bindings[actions] = entry;
        }

        entry.BindInput(action);
    }

    public void LockInput(Actions actions)
    {
        if (!bindings.TryGetValue(actions, out var entry)) return;
        entry.SetLocked(true);
        entry.InputAction.Disable();
    }

    public void UnlockInput(Actions actions)
    {
        if (!bindings.TryGetValue(actions, out var entry)) return;
        entry.SetLocked(false);
        entry.InputAction.Enable();
    }
    #endregion

    #region 키 관리
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
        if (!TryGetInputAction(actions, out InputAction inputAction)) return;

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
        map.Enable();

        foreach (InputBindingEntry entry in bindings.Values)
        {
            if (entry.IsLocked)
            {
                entry.InputAction.Disable();
            }
        }
    }

    public void Disable()
    {
        map.Disable();
    }
    #endregion

    #region Utils
    private bool TryGetInputAction(Actions actions, out InputAction action)
    {
        action = map.FindAction(actions.ToString());
        if (action == null)
        {
            Logger.LogWarning($"{action} 못 찾음");
            return false;
        }
        return true;
    }
    #endregion
}
