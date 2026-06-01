using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Player player;
    public Vector2 InputVec { get; private set; }
    public float Movement {  get; private set; }
    public Rigidbody2D Rb { get; private set; }
    public Collider2D Co { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Co = GetComponent<Collider2D>();
        player = GetComponent<Player>();
    }
    private void Start()
    {
        /* --- Test Set --- */
        InputManager.Instance.PushMode(InputEnum.InputMode.Gameplay);
        /* --- -------- --- */
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Move, OnMove);
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Run, OnRun);
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Crouch, OnCrouch);
    }



    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Movement = player.CurrentMovement; // float speed; 이동속도의 계산값을 받아 사용
        Vector2 dir = InputVec; // Vector2 dir; // Input의 Vector2 값을 받아 사용
        Rb.velocity = dir * Movement; // Rb.velocity = dir * speed;
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InputVec = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            InputVec = Vector2.zero;
        }
    }
    private void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Movement = player.CurrentMovement * 1.5f;
        }
        else if (context.canceled)
        {
            Movement = player.CurrentMovement;
        }
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Movement = player.CurrentMovement * 0.5f;
        }
        else if (context.canceled)
        {
            Movement = player.CurrentMovement;
        }
    }
}
