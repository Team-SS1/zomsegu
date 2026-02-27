using InputEnum;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InputActionAsset에서 InputLayer에 해당하는 InputActionMap을 활성화/비활성화하여 
/// 레이어별로 인풋을 관리하는 매니저
/// </summary>
public class InputManager : GlobalSingleton<InputManager>
{
    #region 필드
    [SerializeField] private InputActionAsset inputAssets;

    private Dictionary<ActionMaps, InputHandler> handlers;
    private ActionMaps activeLayers = ActionMaps.None;
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

    private void OnDestroy()
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
        handlers = new();

        foreach (var map in inputAssets.actionMaps)
        {
            if (Enum.TryParse(map.name, out ActionMaps actionMaps))
            {
                handlers.Add(actionMaps, new InputHandler(map));
                map.Disable(); // 초기 비활성화
            }
            else
            {
                Logger.LogWarning($"InputActionMap.'{map.name}' 없음");
            }
        }
    }

    private void DisposeHandlers()
    {
        if (handlers != null)
        {
            foreach (var handler in handlers.Values)
            {
                handler.Dispose();
            }
            handlers.Clear();
        }
    }
    #endregion

    #region 레이어 설정
    /// <summary>
    /// 기존 레이어 설정 모두 제거, 새로운 레이어 설정
    /// 게임 초기화 시 사용 권장
    /// </summary>
    /// <param name="actionMaps"></param>
    public void SetLayer(ActionMaps actionMaps)
    {
        activeLayers = actionMaps;
        UpdateMaps();
    }

    /// <summary>
    /// 특정 레이어 추가
    /// </summary>
    /// <param name="actionMaps"></param>
    public void AddLayer(ActionMaps actionMaps)
    {
        activeLayers |= actionMaps;
        UpdateMaps();
    }

    /// <summary>
    /// 특정 레이어 제거
    /// </summary>
    /// <param name="actionMaps"></param>
    public void RemoveLayer(ActionMaps actionMaps)
    {
        activeLayers &= ~actionMaps;
        UpdateMaps();
    }

    /// <summary>
    /// 레이어 보유 여부 확인
    /// </summary>
    /// <param name="actionMaps"></param>
    /// <returns></returns>
    public bool HasLayer(ActionMaps actionMaps)
    {
        return (activeLayers & actionMaps) != 0;
    }

    private void UpdateMaps()
    {
        foreach (var kvp in handlers)
        {
            if ((activeLayers & kvp.Key) != 0)
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

    #region 바인딩
    /// <summary>
    /// ActionMaps의 Actions에 함수 바인딩하기
    /// </summary>
    /// <param name="actionMaps"></param>
    /// <param name="actions"></param>
    /// <param name="action"></param>
    public void BindInput(
        ActionMaps actionMaps,
        Actions actions,
        Action<InputAction.CallbackContext> action)
    {
        if (!handlers.TryGetValue(actionMaps, out InputHandler handler))
        {
            Logger.LogWarning($"ActionMap.'{actionMaps}' 핸들러 없음");
            return;
        }
        handler.BindInput(actions, action);
    }

    /// <summary>
    /// ActionMaps의 Actions에 바인딩된 키를 newPath로 변경
    /// </summary>
    /// <param name="actionMaps"></param>
    /// <param name="actions"></param>
    /// <param name="newPath"></param>
    public void ApplyBindingOverride(ActionMaps actionMaps, Actions actions, string newPath)
    {
        if (!handlers.TryGetValue(actionMaps, out InputHandler handler))
        {
            Logger.LogWarning($"ActionMap.'{actionMaps}' 핸들러 없음");
            return;
        }
        handler.ApplyBindingOverride(actions, newPath);
    }
    #endregion

    #region 키 설정 저장 & 불러오기
    /// <summary>
    /// DataManager에서 가져가서 사용
    /// </summary>
    /// <returns></returns>
    public string ExportBindingJson()
    {
        return inputAssets.SaveBindingOverridesAsJson();
    }

    /// <summary>
    /// Datamanager에서 가져온 키 설정 JSON을 InputActionAsset에 적용
    /// </summary>
    /// <param name="json"></param>
    public void ImportBindingJson(string json)
    {
        inputAssets.LoadBindingOverridesFromJson(json);
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
