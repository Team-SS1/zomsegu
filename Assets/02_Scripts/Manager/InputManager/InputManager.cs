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

    private Dictionary<InputLayer, InputActionMap> maps;
    private InputLayer activeLayers;
    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        InitializeMaps();
    }

    private void InitializeMaps()
    {
        maps = new();

        foreach (var map in inputAssets.actionMaps)
        {
            if (Enum.TryParse(map.name, out InputLayer state))
            {
                maps.Add(state, map);
            }
            else
            {
                Logger.LogWarning($"InputActionMap '{map.name}'과 대응되는 InputState 없음");
            }
        }
    }
    #endregion

    #region [public] 레이어 설정
    public void SetLayer(InputLayer layers)
    {
        activeLayers = layers;
        UpdateMaps();
    }

    public void AddLayer(InputLayer layer)
    {
        activeLayers |= layer;
        UpdateMaps();
    }

    public void RemoveLayer(InputLayer layer)
    {
        activeLayers &= ~layer;
        UpdateMaps();
    }

    public bool HasLayer(InputLayer layer)
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
