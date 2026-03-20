using DialogueEnum;
using InputEnum;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueInputController : MonoBehaviour
{
    private UIDialogue uiRoot;
    private DialogueTyper typer;

    private void Awake()
    {
        TryGetComponent(out uiRoot);
        typer = GetComponentInChildren<DialogueTyper>(true);
    }

    private void Start()
    {
        InputManager mg = InputManager.Instance;
        mg.BindInput(ActionMaps.Dialogue, Actions.Next, OnNext);
        mg.BindInput(ActionMaps.Dialogue, Actions.Previous, OnPrev);
        mg.BindInput(ActionMaps.Dialogue, Actions.Skip, OnSkip);
        mg.BindInput(ActionMaps.Dialogue, Actions.AllSkip, OnAllSkip);
        mg.BindInput(ActionMaps.Dialogue, Actions.Auto, OnAuto);
        mg.BindInput(ActionMaps.Dialogue, Actions.Backlog, OnBacklog);
    }

    private void OnEnable()
    {
        InputManager mg = InputManager.Instance;
        if (mg == null) return;
        mg.RemoveMaps(ActionMaps.Gameplay);
        mg.RemoveMaps(ActionMaps.UI);
        mg.AddMaps(ActionMaps.Dialogue);
    }

    private void OnDisable()
    {
        InputManager mg = InputManager.Instance;
        if (mg == null) return;
        mg.AddMaps(ActionMaps.Gameplay);
        mg.AddMaps(ActionMaps.UI);
        mg.RemoveMaps(ActionMaps.Dialogue);
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            uiRoot.AdvanceOrCompleteCurrentLine();
        }
    }

    private void OnPrev(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            uiRoot.ChangeMode(DialogueMode.None);
            if (typer.IsTyping) typer.SkipOrComplete();
            uiRoot.ShowPreviousLine();
        }
    }

    private void OnSkip(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            uiRoot.ChangeMode(DialogueMode.Skip);
        }
        else if (context.canceled)
        {
            uiRoot.ChangeMode(DialogueMode.None);
        }
    }

    private void OnAllSkip(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UIManager.Instance
                .OpenPopup<UIConfirmPopup>()
                .Open("현재 대화를 \n전체스킵하시겠습니까?\n", uiRoot.AllSkip);
        }
    }
}
