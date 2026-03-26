using System;
using System.Collections.Generic;
using UIEnum;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : SceneSingleton<UIManager>
{
    [Header("Database")]
    [SerializeField] private GoDatabase uiDatabase;

    [Header("Canvas Prefab")]
    [SerializeField] private GameObject inputBlockingCanvasPrefab;
    [SerializeField] private GameObject inputPassthroughCanvasPrefab;

    private readonly Dictionary<Type, BaseUI> uiCache = new();  // 리소스 캐시

    private Dictionary<UIOrder, RectTransform> uiRoots = new();
    private Dictionary<UIOrder, List<BaseUI>> uisByOrder = new();

    private UIInputCoordinator coordinator;

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    private void Start()
    {
        InputManager input = InputManager.Instance;
        if (input == null) return;
        coordinator = new(this, input);
    }
    #endregion

    #region 초기화
    private void Init()
    {
        foreach (UIOrder order in Enum.GetValues(typeof(UIOrder)))
        {
            GameObject root;

            string[] splits = order.ToString().Split("_");
            if (splits.Length == 2)
            {
                if (order == UIOrder.Top_Popup)
                {
                    root = Instantiate(inputBlockingCanvasPrefab);
                    root.SetActive(false);
                }
                else
                {
                    root = Instantiate(inputPassthroughCanvasPrefab);
                    root.SetActive(true);
                }
                root.name = $"UI_Root_{order}";

                uiRoots[order] = root.GetComponent<RectTransform>();

                var renderer = root.GetComponent<Canvas>();
                renderer.sortingOrder = (int)order;
            }

            uisByOrder[order] = new List<BaseUI>();
        }

        if (FindAnyObjectByType<EventSystem>() != null) return;
        GameObject newGo = new("EventSystem");
        newGo.AddComponent<EventSystem>();
        newGo.AddComponent<StandaloneInputModule>();
    }
    #endregion

    #region 생성 / 조회
    /// <summary>
    /// ui 프리팹 가져오기
    /// gameobject 활성화 상태를 지정하고 싶을 경우 active 사용
    /// </summary>
    public T GetPanel<T>() where T : BaseUI
    {
        return GetOrCreateUI<T>();
    }

    private T GetOrCreatePopup<T>() where T : UIPopup
    {
        return GetOrCreateUI<T>();
    }

    private T GetOrCreateUI<T>() where T : BaseUI
    {
        var type = typeof(T);

        if (uiCache.TryGetValue(type, out BaseUI cachedPrefab))
        {
            foreach (BaseUI ui in uisByOrder[cachedPrefab.Order])
            {
                if (ui is T)
                {
                    return ui as T;
                }
            }
        }

        T prefab = GetResource<T>();

        T newUi = Instantiate(prefab, uiRoots[prefab.Order]);
        var rect = newUi.GetComponent<RectTransform>();
        foreach (UIOrder order in GetUIOrderPanels())
        {
            if (newUi.Order == order)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
            }
        }
        rect.localScale = Vector3.one;

        uisByOrder[newUi.Order].Add(newUi);

        return newUi;
    }

    private T GetResource<T>() where T : BaseUI
    {
        var type = typeof(T);

        if (uiCache.TryGetValue(type, out BaseUI ui))
        {
            return ui as T;
        }

        T prefab = null;
        foreach (GameObject go in uiDatabase.GetDatabase())
        {
            if (go.TryGetComponent(out prefab)) break;
        }

        if (prefab == null)
        {
            throw new InvalidOperationException($"{type} 프리팹 못찾음. 경로: Resources/UI");
        }

        uiCache[type] = prefab;
        return prefab;
    }
    #endregion

    #region 열기 / 닫기 - panel
    public T OpenPanel<T>() where T : BaseUI
    {
        T ui = GetPanel<T>();
        ui.gameObject.SetActive(true);
        return ui;
    }

    public void ClosePanel<T>() where T : BaseUI
    {
        if (!uiCache.TryGetValue(typeof(T), out BaseUI cachedPrefab)) return;

        foreach (BaseUI ui in uisByOrder[cachedPrefab.Order])
        {
            if (ui is T)
            {
                ui.gameObject.SetActive(false);
            }
        }
    }

    public void CloseAllPanels()
    {
        foreach (UIOrder order in GetUIOrderPanels())
        {
            foreach (BaseUI ui in uisByOrder[order])
            {
                ui.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region 열기 / 닫기 - 팝업 ui
    public T OpenPopup<T>() where T : UIPopup
    {
        T ui = GetOrCreatePopup<T>();

        ui.OnUIClick -= ClickPopup;
        ui.OnUIClick += ClickPopup;

        uiRoots[ui.Order].gameObject.SetActive(true);
        ui.gameObject.SetActive(true);

        return ui;
    }

    public void ClosePopup(UIPopup ui)
    {
        ui.gameObject.SetActive(false);

        if (GetActivePopupCount(ui.Order) == 0)
        {
            uiRoots[ui.Order].gameObject.SetActive(false);
        }
    }

    public void CloseTopPopup()
    {
        foreach (UIOrder order in GetUIOrderPopups())
        {
            if (GetActivePopupCount(order) == 0) continue;

            UIPopup ui = uisByOrder[order][^1] as UIPopup;
            ui.Close();
        }
    }

    public void CloseAllPopups()
    {
        foreach (UIOrder order in GetUIOrderPopups())
        {
            List<BaseUI> uiList = uisByOrder[order];
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                UIPopup ui = uiList[i] as UIPopup;
                ui.Close();
            }

            uiRoots[order].gameObject.SetActive(false);
        }
    }

    private void ClickPopup(UIPopup ui)
    {
        int idx = uisByOrder[ui.Order].IndexOf(ui);
        uisByOrder[ui.Order].RemoveAt(idx);
        uisByOrder[ui.Order].Add(ui);

        ui.TryGetComponent<RectTransform>(out var rect);
        rect.SetSiblingIndex(rect.parent.childCount - 1);
    }
    #endregion

    #region Utils
    /// <summary>
    /// ui 활성화 확인
    /// </summary>
    public bool IsOpen<T>() where T : BaseUI
    {
        foreach (List<BaseUI> uIs in uisByOrder.Values)
        {
            foreach (var ui in uIs)
            {
                if (ui is T)
                {
                    return ui.gameObject.activeSelf;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// ui 캐시 정리하고 싶을 경우 (ex. 씬 전환)
    /// </summary>
    public void ClearCache() => uiCache.Clear();

    private int GetActivePopupCount(UIOrder order)
    {
        int activeCount = 0;
        uisByOrder[order].ForEach(ui => activeCount += ui.gameObject.activeSelf ? 1 : 0);
        return activeCount;
    }

    private UIOrder[] GetUIOrderPopups()
    {
        return new UIOrder[] { UIOrder.Top_Popup, UIOrder.Middle_Popup };
    }

    private UIOrder[] GetUIOrderPanels()
    {
        return new UIOrder[] { UIOrder.Top_Panel, UIOrder.Middle_Panel, UIOrder.Bottom_Panel };
    }
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        uiDatabase = AssetLoader.FindAndLoadByName<GoDatabase>("UIDatabase");
        inputBlockingCanvasPrefab = AssetLoader.FindAndLoadByName("InputBlockingCanvas");
        inputPassthroughCanvasPrefab = AssetLoader.FindAndLoadByName("InputPassthroughCanvas");
    }
#endif
    #endregion
}
