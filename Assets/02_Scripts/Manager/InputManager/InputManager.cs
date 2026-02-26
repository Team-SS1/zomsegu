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
            if (Enum.TryParse(map.name, out ActionMaps state))
            {
                maps.Add(state, map);
                map.Disable(); // 초기 비활성화
            }
            else
            {
                Logger.LogWarning($"InputActionMap '{map.name}'과 대응되는 InputState 없음");
            }
        }
    }
    #endregion

    #region [public] 레이어 설정
    public void SetLayer(ActionMaps layers)
    {
        activeLayers = layers;
        UpdateMaps();
    }

    public void AddLayer(ActionMaps layer)
    {
        activeLayers |= layer;
        UpdateMaps();
    }

    public void RemoveLayer(ActionMaps layer)
    {
        activeLayers &= ~layer;
        UpdateMaps();
    }

    public bool HasLayer(ActionMaps layer)
    {
        return (activeLayers & layer) != 0;
    }
    #endregion

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

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        inputAssets = AssetLoader.FindAndLoadByName<InputActionAsset>("InputActions");
    }
#endif
    #endregion
}
