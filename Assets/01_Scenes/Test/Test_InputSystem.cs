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
    /// <summary>
    /// Input에 함수 묶기
    /// Input에 묶어야 할 함수는 형태 'Action<InputAction.CallbackContext>'
    /// ActionMaps: InputAsset의 Action Maps에 대응되는 enum
    /// Actions: InputAsset의 Actions에 대응되는 enum
    /// </summary>
    private void Example_BindInput()
    {
        mg.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Move, OnMove);
        mg.BindInput(InputEnum.ActionMaps.Dialogue, InputEnum.Actions.Next, OnNext);
        Logger.Log("바인드");
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputVec = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            inputVec = Vector2.zero;
        }
    }
    #endregion

    #region Example - Layer Control
    [SerializeField] GameObject dialoguePanel;

    public void Example_Open_Dialogue_Block_Gameplay()
    {                                                   // 활성화된 Maps
        mg.SetMaps(InputEnum.ActionMaps.Gameplay);      // Gameplay

        mg.AddMaps(InputEnum.ActionMaps.Dialogue);      // Gameplay | Dialogue
        mg.RemoveMaps(InputEnum.ActionMaps.Gameplay);   // Dialogue

        dialoguePanel.SetActive(true);
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dialoguePanel.SetActive(false);

            mg.RemoveMaps(InputEnum.ActionMaps.Dialogue);
            mg.AddMaps(InputEnum.ActionMaps.Gameplay);
        }
    }
    #endregion

    #region Example - Lock / Unlock Single Input
    private bool isLock = false;

    public void Example_Toggle_Input_Move()
    {
        if (isLock)
        {
            mg.UnlockInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Move);
        }
        else
        {
            mg.LockInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Move);
        }

        isLock = !isLock;
    }
    #endregion

    #region Utils
    public void ToggleLayer(string text)
    {
        Enum.TryParse(text, out InputEnum.ActionMaps actionMaps);

        if (mg.HasMaps(actionMaps))
        {
            mg.RemoveMaps(actionMaps);
            Logger.Log($"{actionMaps} 꺼짐");
        }
        else
        {
            mg.AddMaps(actionMaps);
            Logger.Log($"{actionMaps} 켜짐");
        }
    }
    #endregion
}
