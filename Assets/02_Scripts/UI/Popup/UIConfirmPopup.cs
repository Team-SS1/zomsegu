using InputEnum;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIConfirmPopup : UIPopup
{
    [SerializeField] private TMP_Text message;
    [SerializeField] private BaseButton btnYes;
    [SerializeField] private BaseButton btnNo;

    private readonly Queue<(string, UnityAction)> events = new();

    private UnityAction OnConfirm;
    private bool isFirst = true;

    #region Unity API
    private void Start()
    {
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

    public void Register(string text, UnityAction callback = null)
    {
        events.Enqueue((text, callback));
        Logger.Log("팝업 이벤트 설정");

        if (isFirst)
        {
            isFirst = false;
            (message.text, OnConfirm) = events.Dequeue();
        }
    }

    private void OnClickYes()
    {
        OnConfirm?.Invoke();
        btnYes.onClick.RemoveListener(OnClickYes);
        OnConfirm = null;
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

    public override void Close()
    {
        if (events.Count > 0)
        {
            (message.text, OnConfirm) = events.Dequeue();
            return;
        }

        isFirst = true;
        base.Close();
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
