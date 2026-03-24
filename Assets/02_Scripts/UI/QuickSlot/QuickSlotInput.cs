using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputEnum;
using PlayerEnum;
using UnityEngine.InputSystem;

public class QuickSlotInput : MonoBehaviour
{
    [SerializeField] private UIActiveCharacterContext activeCharacterContext;

    private InputManager mg;

    private void Awake()
    {
        mg = InputManager.Instance;
        Debug.Log("QuickSlotInput Awake");
    }
    private void Start()
    {
        Debug.Log("QuickSlotInput Start");
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot1, OnQuickSlot1);
        Debug.Log($"Gameplay 활성화 여부: {mg.HasMaps(ActionMaps.Gameplay)}");
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot2, OnQuickSlot2);
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot3, OnQuickSlot3);
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot4, OnQuickSlot4);
        mg.BindInput(ActionMaps.Gameplay, Actions.QuickSlot5, OnQuickSlot5);
        Debug.Log("QuickSlot 바인딩 완료");
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
        PlayerType playerType = activeCharacterContext.CurrentActivePlayer;
        ItemTransferService.TrySelectQuickSlot(playerType, index);
    }
}
