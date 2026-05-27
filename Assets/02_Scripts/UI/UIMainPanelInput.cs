using UnityEngine;
using UnityEngine.InputSystem;
using InputEnum;

public class UIMainPanelInput : MonoBehaviour
{
    [SerializeField] private UIMainPanelFlowController flowController;

    private InputManager inputManager;

    private void Awake()
    {
        inputManager = InputManager.Instance;   
    }
    private void Start()
    {
        inputManager.PushMode(InputMode.Gameplay);
        
        inputManager.BindInput(ActionMaps.UI, Actions.Inventory, OnInventoryInput);
        inputManager.BindInput(ActionMaps.UI, Actions.Close, OnCloseInput);
    }

    private void OnInventoryInput(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        flowController.OnInventoryInput();
    }
    private void OnCloseInput(InputAction.CallbackContext context)
    {
        if(!context.started) return;

        flowController.OnCloseInput();
    }
}
