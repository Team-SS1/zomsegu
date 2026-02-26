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

    private Dictionary<ActionMaps, InputActionMap> maps;
    private ActionMaps activeLayers;

    public IReadOnlyDictionary<ActionMaps, InputActionMap> Maps => maps;
    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();

        if (inputAssets != null)
        {
            InitializeMaps();
        }
    }

    /// <summary>
    /// InputActionAsset DI용 초기화 메서드
    /// </summary>
    /// <param name="asset"></param>
    public void Initialize(InputActionAsset asset)
    {
        inputAssets = asset;
        InitializeMaps();
    }

    private void InitializeMaps()
    {
        maps = new();

        foreach (var map in inputAssets.actionMaps)
        {
            if (Enum.TryParse(map.name, out ActionMaps actionMaps))
            {
                maps.Add(actionMaps, map);
                map.Disable(); // 초기 비활성화
            }
            else
            {
                Logger.LogWarning($"InputActionMap '{map.name}'과 대응되는 InputState 없음");
            }
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
        foreach (var kvp in maps)
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

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        inputAssets = AssetLoader.FindAndLoadByName<InputActionAsset>("InputActions");
    }
#endif
    #endregion
}
