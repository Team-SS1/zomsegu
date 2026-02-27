using UnityEngine;
using UnityEngine.InputSystem;

public class Test_InputSystem : MonoBehaviour
{
    Vector2 inputVec;

    void Start()
    {
        InputManager mg = InputManager.Instance;
        mg.SetLayer(InputEnum.ActionMaps.Gameplay);
        mg.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Move, OnMove);
    }

    private void Update()
    {
        transform.position += 5f * Time.deltaTime * (Vector3)inputVec;
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
}
