using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_InputSystem : MonoBehaviour
{
    InputManager mg;
    Vector2 inputVec;

    void Start()
    {
        mg = InputManager.Instance;

        Example_BindInput();
    }

    private void Update()
    {
        transform.position += 5f * Time.deltaTime * (Vector3)inputVec;
    }

    #region Example - Bind Input
    private void Example_BindInput()
    {
        mg.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Move, OnMove);
        mg.BindInput(InputEnum.ActionMaps.Dialogue, InputEnum.Actions.Next, OnNext);
        Logger.Log("바인드");
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            inputVec = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            inputVec = Vector2.zero;
        }
    }
    #endregion

    #region Example - Layer Control
    [SerializeField] GameObject dialoguePanel;

    public void Example_Open_Dialogue_Block_Gameplay()
    {
        mg.SetLayer(InputEnum.ActionMaps.Gameplay);

        mg.AddLayer(InputEnum.ActionMaps.Dialogue);
        mg.RemoveLayer(InputEnum.ActionMaps.Gameplay);

        dialoguePanel.SetActive(true);
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            dialoguePanel.SetActive(false);

            mg.RemoveLayer(InputEnum.ActionMaps.Dialogue);
            mg.AddLayer(InputEnum.ActionMaps.Gameplay);
        }
    }
    #endregion

    #region Utils
    public void ToggleLayer(string text)
    {
        Enum.TryParse(text, out InputEnum.ActionMaps actionMaps);

        if (mg.HasLayer(actionMaps))
        {
            mg.RemoveLayer(actionMaps);
            Logger.Log($"{actionMaps} 꺼짐");
        }
        else
        {
            mg.AddLayer(actionMaps);
            Logger.Log($"{actionMaps} 켜짐");
        }
    }
    #endregion
}
