using UnityEngine;
using InputEnum;
using PlayerEnum;
using UnityEngine.InputSystem;

public class QuickSlotInput : MonoBehaviour
{
    private InputManager mg;

    private void Awake()
    {
        mg = InputManager.Instance;
    }
    private void Start()
    {
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot1, OnQuickSlot1);
        Debug.Log($"Gameplay 활성화 여부: {mg.HasMaps(ActionMaps.Gameplay)}");
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot2, OnQuickSlot2);
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot3, OnQuickSlot3);
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot4, OnQuickSlot4);
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot5, OnQuickSlot5);
    }
    private void OnQuickSlot1(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Debug.Log("입력");
        SelectQuickSlot(0);
    }
    private void OnQuickSlot2(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Debug.Log("입력");
        SelectQuickSlot(1);
    }
    private void OnQuickSlot3(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Debug.Log("입력");
        SelectQuickSlot(2);
    }
    private void OnQuickSlot4(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Debug.Log("입력");
        SelectQuickSlot(3);
    }
    private void OnQuickSlot5(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Debug.Log("입력");
        SelectQuickSlot(4);
    }
    private void SelectQuickSlot(int index)
    {
        PlayerType playerType = PlayerManager.Instance.CurrentActivePlayer;
        ItemTransferService.TrySelectQuickSlot(playerType, index);
    }
}
