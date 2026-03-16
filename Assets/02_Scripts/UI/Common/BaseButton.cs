using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BaseButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] protected Button btn;
    [SerializeField] private AudioData audioData;
    private AudioEnum.AudioName audioName;

    [Header("(Optional) Text Settings")]
    [SerializeField] protected TMP_Text text;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.gray;

    public Button.ButtonClickedEvent onClick => btn.onClick;

    protected bool isPressed = false;

    // ===== Unity API ===== 
    protected virtual void Awake()
    {
        if (audioData != null)
        {
            Enum.TryParse(audioData.name, out audioName);
        }
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
        // todo: 공통 로직 추가히기 (효과음 / 애니메이션)
        AudioManager.Instance.PlaySfx(audioName);
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
        btn = GetComponent<Button>();
        audioData = AssetLoader.FindAndLoadByName<AudioData>("UI_Button_Click");
        text = GetComponentInChildren<TMP_Text>();
    }
#endif
    #endregion
}
