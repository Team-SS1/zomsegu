using InputEnum;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIConfirmPopup : UIPopup
{
    [SerializeField] private TMP_Text message;
    [SerializeField] private BaseButton btnYes;
    [SerializeField] private BaseButton btnNo;

    private UnityAction onConfirm;

    #region Unity API
    private void Start()
    {
        InputManager.Instance.BindInput(ActionMaps.UI, Actions.Close, OnClose);
        InputManager.Instance.BindInput(ActionMaps.UI, Actions.Submit, OnSubmit);
    }

    private void OnEnable()
    {
        btnYes.onClick.AddListener(OnClickYes);
        btnNo.onClick.AddListener(OnClickNo);

        InputManager.Instance.RemoveMaps(ActionMaps.Dialogue);
    }

    private void OnDisable()
    {
        btnYes.onClick.RemoveListener(OnClickYes);
        btnNo.onClick.RemoveListener(OnClickNo);

        InputManager.Instance.AddMaps(ActionMaps.Dialogue);
    }
    #endregion

    public void Open(string text, UnityAction callback = null)
    {
        gameObject.SetActive(true);
        this.message.text = text;
        this.onConfirm = callback;
    }

    private void OnClickYes()
    {
        onConfirm?.Invoke();
        btnYes.onClick.RemoveListener(OnClickYes);
        onConfirm = null;
        Close();
    }

    private void OnClickNo()
    {
        Close();
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnClickYes();
        }
    }

    private void OnClose(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Close();
        }
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        message = transform.FindChild<TMP_Text>("Text");
        btnYes = transform.FindChild<BaseButton>("Btn_Yes");
        btnNo = transform.FindChild<BaseButton>("Btn_No");
    }
#endif
    #endregion
}
