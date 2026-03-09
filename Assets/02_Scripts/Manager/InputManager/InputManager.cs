using InputEnum;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// InputActionAsset에서 InputLayer에 해당하는 InputActionMap을 활성화/비활성화하여 
/// 레이어별로 인풋을 관리하는 매니저
/// </summary>
public class InputManager : GlobalSingleton<InputManager>
{
    #region 필드
    [SerializeField] private InputActionAsset inputAssets;

    private readonly Dictionary<ActionMaps, InputHandler> handlers = new();
    private ActionMaps activeActionMaps = ActionMaps.None;
    #endregion

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        if (inputAssets != null)
        {
            InitializeHandlers();
        }
    }

    protected override void OnDestory()
    {
        DisposeHandlers();
    }
    #endregion

    #region 초기화
    /// <summary>
    /// InputActionAsset DI용 초기화 메서드
    /// </summary>
    /// <param name="asset"></param>
    public void Initialize(InputActionAsset asset)
    {
        inputAssets = asset;
        InitializeHandlers();
    }

    private void InitializeHandlers()
    {
        DisposeHandlers();

        foreach (var map in inputAssets.actionMaps)
        {
            if (Enum.TryParse(map.name, out ActionMaps actionMaps))
            {
                handlers[actionMaps] = new InputHandler(map);
            }
            else
            {
                Logger.LogWarning($"InputActionMap.'{map.name}' 없음");
            }
        }

        SyncAllMaps();
    }

    private void DisposeHandlers()
    {
        foreach (var handler in handlers.Values)
        {
            handler.Dispose();
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetMaps(ActionMaps.None);
        DisposeHandlers();
    }
    #endregion

    #region 레이어 설정
    /// <summary>
    /// 기존 Input Action Maps 설정 모두 제거, 새로운 설정
    /// 게임 초기화 시 사용 권장
    /// </summary>
    public void SetMaps(ActionMaps actionMaps)
    {
        ActionMaps prev = activeActionMaps;
        activeActionMaps = actionMaps;
        SyncChangedMaps(prev, activeActionMaps);
    }

    /// <summary>
    /// 특정 Input Action Maps 추가
    /// </summary>
    public void AddMaps(ActionMaps actionMaps)
    {
        ActionMaps prev = activeActionMaps;
        activeActionMaps |= actionMaps;
        SyncChangedMaps(prev, activeActionMaps);
    }

    /// <summary>
    /// 특정 Input Action Maps 제거
    /// </summary>
    public void RemoveMaps(ActionMaps actionMaps)
    {
        ActionMaps prev = activeActionMaps;
        activeActionMaps &= ~actionMaps;
        SyncChangedMaps(prev, activeActionMaps);
    }

    /// <summary>
    /// Input Action Maps 보유 여부 확인
    /// </summary>
    public bool HasMaps(ActionMaps actionMaps)
    {
        return (activeActionMaps & actionMaps) != 0;
    }

    private void SyncAllMaps()
    {
        foreach (var kvp in handlers)
        {
            if ((activeActionMaps & kvp.Key) != 0) kvp.Value.Enable();
            else kvp.Value.Disable();
        }
    }

    private void SyncChangedMaps(ActionMaps prev, ActionMaps next)
    {
        var changed = prev ^ next;
        if (changed == 0) return;

        foreach (var kvp in handlers)
        {
            if ((changed & kvp.Key) == 0) continue;

            if ((next & kvp.Key) != 0)
            {
                kvp.Value.Enable();
            }
            else
            {
                kvp.Value.Disable();
            }
        }
    }
    #endregion

    #region 인풋 설정
    /// <summary>
    /// ActionMaps의 Actions에 함수 바인딩하기
    /// </summary>
    public void BindInput(ActionMaps actionMaps, Actions actions, Action<InputAction.CallbackContext> action)
    {
        if (!TryGetInputHandler(actionMaps, out InputHandler handler)) return;
        handler.BindInput(actions, action);
    }

    public void LockInput(ActionMaps actionMaps, Actions actions)
    {
        if (!TryGetInputHandler(actionMaps, out InputHandler handler)) return;
        handler.LockInput(actions);
    }

    public void UnlockInput(ActionMaps actionMaps, Actions actions)
    {
        if (!TryGetInputHandler(actionMaps, out InputHandler handler)) return;
        handler.UnlockInput(actions);
    }

    /// <summary>
    /// ActionMaps의 Actions에 바인딩된 키를 newPath로 변경
    /// </summary>
    public void ApplyBindingOverride(ActionMaps actionMaps, Actions actions, string newPath, int bindingIndex = 0)
    {
        if (!TryGetInputHandler(actionMaps, out InputHandler handler)) return;
        handler.ApplyBindingOverride(actions, newPath, bindingIndex);
    }
    #endregion

    #region 키 설정 저장 & 불러오기
    /// <summary>
    /// DataManager에서 가져가서 사용
    /// </summary>
    public string ExportBindingJson()
    {
        return inputAssets.SaveBindingOverridesAsJson();
    }

    /// <summary>
    /// Datamanager에서 가져온 키 설정 JSON을 InputActionAsset에 적용
    /// </summary>
    public void ImportBindingJson(string json)
    {
        inputAssets.LoadBindingOverridesFromJson(json);
        SyncAllMaps();  // 상태 초기화
    }
    #endregion

    #region Utils
    private bool TryGetInputHandler(ActionMaps actionMaps, out InputHandler handler)
    {
        handler = null;

        if (!IsSingleFlag(actionMaps))
        {
            Logger.LogWarning($"ActionMaps.'{actionMaps}'는 단일 플래그가 아님");
            return false;
        }

        if (!handlers.TryGetValue(actionMaps, out handler))
        {
            Logger.LogWarning($"ActionMap.'{actionMaps}' 핸들러 없음");
            return false;
        }

        return true;
    }

    public static bool IsSingleFlag(ActionMaps value)
    {
        return value != 0 && (value & (value - 1)) == 0;
    }
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        inputAssets = AssetLoader.FindAndLoadByName<InputActionAsset>("InputActions");
    }
#endif
    #endregion
}
