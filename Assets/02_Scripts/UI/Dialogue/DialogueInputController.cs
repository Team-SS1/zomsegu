using DialogueEnum;
using InputEnum;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UIDialogue))]
public class DialogueInputController : MonoBehaviour
{
    private UIDialogue dialogue;
    private DialogueTyper typer;

    private (Actions action, Action<InputAction.CallbackContext>)[] bindings;

    #region Unity API
    private void Awake()
    {
        dialogue = GetComponent<UIDialogue>();
        typer = GetComponentInChildren<DialogueTyper>(true);

        InitDialogueBindings();
    }

    private void Start()
    {
        InputManager mg = InputManager.Instance;

        mg.BindInputs(ActionMaps.Dialogue, bindings);

        mg.LockInput(ActionMaps.Dialogue, Actions.Navigate);
        mg.LockInput(ActionMaps.Dialogue, Actions.Submit);
    }

    private void OnEnable()
    {
        InputManager.Instance?.PushMode(InputMode.Dialogue);
    }

    private void OnDisable()
    {
        InputManager.Instance?.PopMode();
    }
    #endregion

    #region 초기화
    private void InitDialogueBindings()
    {
        bindings = new (Actions action, Action<InputAction.CallbackContext>)[]
        {
            (Actions.Next, OnNext),
            (Actions.Previous, OnPrev),
            (Actions.Navigate, OnNavigate),
            (Actions.Skip, OnSkip),
            (Actions.AllSkip, OnAllSkip),
            (Actions.Submit, OnSubmit),
            (Actions.Auto, OnAuto),
            (Actions.Backlog, OnBacklog),
        };
    }
    #endregion

    private void OnNext(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dialogue.AdvanceOrCompleteCurrentLine();
        }
    }

    private void OnPrev(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dialogue.ChangeMode(DialogueMode.None);
            if (typer.IsTyping) typer.SkipOrComplete();
            dialogue.ShowPreviousLine();
        }
    }

    private void OnSkip(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dialogue.ChangeMode(DialogueMode.Skip);
        }
        else if (context.canceled)
        {
            dialogue.ChangeMode(DialogueMode.None);
        }
    }

    private void OnAllSkip(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UIManager.Instance
                .OpenUI<UIConfirmPopup>()
                .Register("현재 대화를 \n전체스킵하시겠습니까?\n", dialogue.AllSkip);
        }
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!dialogue.CanSelectChoice()) return;

        if (context.started)
        {
            bool isAuto = dialogue.CurMode == DialogueMode.Auto;
            dialogue.AdvanceOrCompleteCurrentLine();

            float axis = context.ReadValue<float>();
            if (Mathf.Approximately(axis, 0f)) return;

            dialogue.SelectChoice(axis);

            if (isAuto) dialogue.ChangeMode(DialogueMode.Auto);
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dialogue.SubmitCurChoice();
        }
    }

    private void OnAuto(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dialogue.ToggleMode(DialogueMode.Auto);
        }
    }

    private void OnBacklog(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dialogue.ToggleMode(DialogueMode.Backlog);
        }
    }
}
