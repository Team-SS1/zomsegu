using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BaseButton : MonoBehaviour
{
    [Header("(Optional) Text Settings")]
    [SerializeField] protected TMP_Text text;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.gray;

    protected Button btn;
    public Button.ButtonClickedEvent onClick => btn.onClick;

    protected bool isPressed = false;

    // ===== Unity API ===== 
    protected virtual void Awake()
    {
        btn = GetComponent<Button>();
        AwakeInternal();
    }

    private void OnEnable()
    {
        btn.onClick.AddListener(OnClick);
        ResetState();
        EnableInternal();
    }

    private void OnDisable()
    {
        btn.onClick.RemoveListener(OnClick);
        DisableInternal();
    }

    protected virtual void AwakeInternal() { }

    protected virtual void EnableInternal() { }

    protected virtual void DisableInternal() { }

    // ===== Button Click Event ===== 
    private void OnClick()
    {
        isPressed = true;
        // todo: 공통 로직 추가히기
        // 사운드 / button vfx
        OnClickInternal();

        if (text == null) return;
        text.color = selectedColor;
    }

    protected virtual void OnClickInternal() { }

    // ===== State Control ===== 
    public virtual void ResetState()
    {
        isPressed = false;
        text.color = defaultColor;
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        text = GetComponentInChildren<TMP_Text>();
    }
#endif
    #endregion
}
