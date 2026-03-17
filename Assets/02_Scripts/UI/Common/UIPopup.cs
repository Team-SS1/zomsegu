using InputEnum;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private BaseButton btnYes;
    [SerializeField] private BaseButton btnNo;

    private UnityAction callback;

    private void OnEnable()
    {
        btnYes.onClick.AddListener(OnClickYes);
        btnNo.onClick.AddListener(OnClickNo);

        InputManager.Instance.AddMaps(ActionMaps.UI);
        InputManager.Instance.RemoveMaps(ActionMaps.Dialogue);
    }

    private void Start()
    {
        InputManager.Instance.BindInput(ActionMaps.UI, Actions.Close, OnClose);
        InputManager.Instance.BindInput(ActionMaps.UI, Actions.Submit, OnSubmit);
    }

    private void OnDisable()
    {
        btnYes.onClick.RemoveListener(OnClickYes);
        btnNo.onClick.RemoveListener(OnClickNo);

        InputManager.Instance.RemoveMaps(ActionMaps.UI);
        InputManager.Instance.AddMaps(ActionMaps.Dialogue);
    }

    public void OpenPopup(string text, UnityAction callback = null)
    {
        gameObject.SetActive(true);
        this.text.text = text;
        this.callback = callback;
    }

    private void OnClickYes()
    {
        callback?.Invoke();
        gameObject.SetActive(false);
        btnYes.onClick.RemoveListener(OnClickYes);
    }

    private void OnClickNo()
    {
        gameObject.SetActive(false);
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
            gameObject.SetActive(false);
        }
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        text = transform.FindChild<TMP_Text>("Text");
        btnYes = transform.FindChild<BaseButton>("Btn_Yes");
        btnNo = transform.FindChild<BaseButton>("Btn_No");
    }
#endif
    #endregion
}
