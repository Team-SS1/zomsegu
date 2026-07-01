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

    [Header("Vehicle")]
    [SerializeField] private LayerMask vehicleLayer;
    [SerializeField] private float vehicleEnterHoldTime = 0.7f;

    private Coroutine vehicleHoldRoutine;
    private VehicleSeatController currentVehicle;


    void Start()
    {
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Interact, OnInteraction);
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.ExtraInteract, OnVehicleInteraction);
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

    public void OnVehicleInteraction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (vehicleHoldRoutine != null)
                StopCoroutine(vehicleHoldRoutine);

            vehicleHoldRoutine = StartCoroutine(VehicleHoldRoutine());
        }
        else if (context.canceled)
        {
            if (vehicleHoldRoutine != null)
            {
                StopCoroutine(vehicleHoldRoutine);
                vehicleHoldRoutine = null;

                // 짧게 누른 경우
                if (currentVehicle != null)
                {
                    currentVehicle.ToggleEngine();
                }
            }
        }
    }

    private IEnumerator VehicleHoldRoutine()
    {
        yield return new WaitForSeconds(vehicleEnterHoldTime);

        // 탑승 중이면 E 꾹으로 시동 끄고 하차
        if (currentVehicle != null)
        {
            currentVehicle.ExitVehicleWithEngineOff();
            currentVehicle = null;
            vehicleHoldRoutine = null;
            yield break;
        }

        // 탑승 전이면 E 꾹으로 탑승
        VehicleSeatController vehicle = FindNearestVehicle();

        if (vehicle != null)
        {
            Player player = GetComponent<Player>();

            if (vehicle.TryEnter(player))
                currentVehicle = vehicle;
        }

        vehicleHoldRoutine = null;
    }

    private VehicleSeatController FindNearestVehicle()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            InteractionRange,
            vehicleLayer
        );

        VehicleSeatController nearest = null;
        float best = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            VehicleSeatController v = hits[i].GetComponentInParent<VehicleSeatController>();
            if (v == null)
                continue;

            float sqr = ((Vector2)v.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqr < best)
            {
                best = sqr;
                nearest = v;
            }
        }

        return nearest;
    }
}
