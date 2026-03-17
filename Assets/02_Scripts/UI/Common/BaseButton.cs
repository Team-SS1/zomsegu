using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BaseButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] protected Button btn;
    [SerializeField] protected AudioEnum.AudioName audioName;

    [Header("(Optional) Text Settings")]
    [SerializeField] protected TMP_Text text;
    [SerializeField] protected Color defaultColor = Color.white;
    [SerializeField] protected Color selectedColor = Color.gray;

    public Button.ButtonClickedEvent onClick => btn.onClick;

    private Coroutine coroutine;

    // ===== Unity API ===== 
    protected virtual void Awake()
    {
        AwakeInternal();
    }

    private void OnEnable()
    {
        btn.onClick.AddListener(OnClick);
        SetState(false);
        EnableInternal();
    }

    private void OnDisable()
    {
        btn.onClick.RemoveListener(OnClick);

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        DisableInternal();
    }

    protected virtual void AwakeInternal() { }

    protected virtual void EnableInternal() { }

    protected virtual void DisableInternal() { }

    // ===== Button Click Event ===== 
    private void OnClick()
    {
        // todo: 공통 로직 추가히기 (효과음 / 애니메이션)
        AudioManager.Instance.PlaySfx(audioName);
        OnClickInternal();
        EventSystem.current?.SetSelectedGameObject(null);       // 버튼 캐싱 삭제
    }

    protected virtual void OnClickInternal()
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(OnClickRoutine());
    }

    // ===== State Control ===== 
    private IEnumerator OnClickRoutine()
    {
        SetState(true);
        yield return GameConstants.BtnDelayTime;
        SetState(false);
    }

    public virtual void SetState(bool active)
    {
        if (text == null) return;

        text.color = active ? selectedColor : defaultColor;
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        btn = GetComponent<Button>();
        Enum.TryParse("UI_Button_Click", out audioName);
        text = GetComponentInChildren<TMP_Text>();
    }
#endif
    #endregion
}
