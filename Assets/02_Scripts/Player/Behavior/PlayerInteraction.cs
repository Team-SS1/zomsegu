using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{    
    public LayerMask interactableLayer;
    public float interactionDelay = 0.3f;

    public bool InteractionTrigger { get; private set; }
    public bool IsInteractable { get; private set; }
    public float InteractionRange { get; private set; } = 3;


    void Start()
    {
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Interact, OnInteraction);
    }
    public void Interaction()
    {
        if (IsInteractable)
        {
#if UNITY_EDITOR
            Debug.Log("Interact return");
#endif
            return;
        }

        IsInteractable = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            InteractionRange,
            interactableLayer
        );

        if(hits == null || hits.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Interact null (no collider in range / layer)");
#endif
            IsInteractable = false;
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.OnInteract();
            }
        }
#if UNITY_EDITOR
        Debug.Log("Interact Complete");
#endif
        StartCoroutine(InteractionDelay(interactionDelay));
    }
    private IEnumerator InteractionDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsInteractable = false;
    }
    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractionTrigger = true;
        }
        else if (context.performed)
        {
            Interaction();
        }
        else if (context.canceled)
        {
            InteractionTrigger = false;
        }
    }
}
