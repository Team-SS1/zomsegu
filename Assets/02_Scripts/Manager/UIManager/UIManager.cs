using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIManager : GlobalSingleton<UIManager>
{
    [Header("Database")]
    [SerializeField] private GoDatabase uiDatabase;

    [Header("Canvas Prefab")]
    [SerializeField] private GameObject panelCanvasPrefab;
    [SerializeField] private GameObject popupCanvasPrefab;

    private readonly Dictionary<Type, BaseUI> uiCache = new();  // 리소스 캐시

    private RectTransform panelRoot;
    private RectTransform popupRoot;

    private readonly Dictionary<Type, BaseUI> panelMap = new(); // 기본 ui
    private readonly List<UIPopup> popupStack = new();          // 팝업 ui

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
        panelRoot = Instantiate(panelCanvasPrefab).GetComponent<RectTransform>();
        popupRoot = Instantiate(popupCanvasPrefab).GetComponent<RectTransform>();
        popupRoot.gameObject.SetActive(false);

        GameObject newGo = new("EventSystem");
        newGo.AddComponent<EventSystem>();
        newGo.AddComponent<StandaloneInputModule>();
    }
    #endregion

    #region 씬 관리
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoaded(scene, mode);

        panelMap.Clear();
        popupStack.Clear();
        popupRoot.gameObject.SetActive(false);
    }
    #endregion

    #region 생성 / 조회
    /// <summary>
    /// ui 프리팹 가져오기
    /// gameobject 활성화 상태를 지정하고 싶을 경우 active 사용
    /// </summary>
    public T GetPanel<T>(bool active = false) where T : BaseUI
    {
        var type = typeof(T);

        if (panelMap.TryGetValue(type, out BaseUI ui))  // 이미 존재
        {
            ui.gameObject.SetActive(active);
            return ui as T;
        }

        T prefab = GetResource<T>();
        ui = Instantiate(prefab, panelRoot);
        ui.gameObject.SetActive(active);

        panelMap[type] = ui;

        return ui as T;
    }

    private T GetOrCreatePopup<T>() where T : UIPopup
    {
        foreach (UIPopup popup in popupStack)
        {
            if (popup is T) return popup as T;
        }

        T prefab = GetResource<T>();
        T ui = Instantiate(prefab, popupRoot);

        popupStack.Add(ui);

        return ui;
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
            Logger.LogWarning($"{type} 프리팹 못찾음. 경로: Resources/UI");
            return null;
        }

        uiCache[type] = prefab;
        return prefab;
    }
    #endregion

    #region 열기 / 닫기 - panel
    public void OpenPanel<T>() where T : BaseUI
    {
        T ui = GetPanel<T>(true);
    }

    public void ClosePanel<T>() where T : BaseUI
    {
        if (!panelMap.TryGetValue(typeof(T), out BaseUI ui)) return;
        ui.gameObject.SetActive(false);
    }

    public void CloseAllPanels()
    {
        foreach (BaseUI ui in panelMap.Values)
        {
            ui.gameObject.SetActive(false);
        }
    }
    #endregion

    #region 열기 / 닫기 - 팝업 ui
    public T OpenPopup<T>() where T : UIPopup
    {
        T ui = GetOrCreatePopup<T>();

        ui.OnUIClick -= ClickPopup;
        ui.OnUIClick += ClickPopup;

        popupRoot.gameObject.SetActive(true);
        ui.gameObject.SetActive(true);

        return ui;
    }

    public void ClosePopup(UIPopup ui)
    {
        ui.Close();

        if (GetActivePopupCount() == 0)
        {
            popupRoot.gameObject.SetActive(false);
        }
    }

    public void CloseTopPopup()
    {
        if (GetActivePopupCount() == 0) return;

        UIPopup ui = popupStack[^1];
        ClosePopup(ui);
    }


    public void CloseAllPopups()
    {
        for (int i = popupStack.Count - 1; i >= 0; i--)
        {
            popupStack[i].gameObject.SetActive(false);
            popupStack.RemoveAt(i);
        }

        popupRoot.gameObject.SetActive(false);
    }

    private void ClickPopup(UIPopup ui)
    {
        int idx = popupStack.IndexOf(ui);
        popupStack.RemoveAt(idx);
        popupStack.Add(ui);

        ui.TryGetComponent<RectTransform>(out var rect);
        rect.SetSiblingIndex(rect.parent.childCount - 1);
    }
    #endregion

    #region Utils
    /// <summary>
    /// ui (팝업 제외) 활성화 확인
    /// </summary>
    public bool IsOpen<T>() where T : BaseUI
    {
        if (!panelMap.TryGetValue(typeof(T), out BaseUI ui))
        {
            return false;
        }

        return ui.gameObject.activeSelf;
    }

    /// <summary>
    /// ui 정리하고 싶을 경우 (ex. 씬 전환)
    /// </summary>
    public void ClearCache() => uiCache.Clear();

    private int GetActivePopupCount()
    {
        int activeCount = 0;
        popupStack.ForEach(ui => activeCount += ui.gameObject.activeSelf ? 1 : 0);
        return activeCount;
    }
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        uiDatabase = AssetLoader.FindAndLoadByName<GoDatabase>("UIDatabase");
        panelCanvasPrefab = AssetLoader.FindAndLoadByName("MainCanvas");
        popupCanvasPrefab = AssetLoader.FindAndLoadByName("PopupCanvas");
    }
#endif
    #endregion
}
