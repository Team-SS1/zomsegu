using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Camera Camera;
    public Vector2 InputVec { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    public Collider2D Co { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Co = GetComponent<Collider2D>();
        Camera = Camera.main;
    }
    private void Start()
    {
        InputManager.Instance.BindInput(InputEnum.ActionMaps.Gameplay, InputEnum.Actions.Move, OnMove);
    }
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float speed = 5; // float speed; 이동속도의 계산값을 받아 사용
        Vector2 dir = InputVec; // Vector2 dir; // Input의 Vector2 값을 받아 사용
        Rb.velocity = dir * speed; // Rb.velocity = dir * speed;
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
    public Vector2 MouseDirection()
    {
        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.ScreenToWorldPoint(mouseScreen);

        Vector2 origin = transform.position;
        Vector2 dir = (Vector2)mouseWorld - origin;

        if (dir.sqrMagnitude < 0.001f)
            return Vector2.zero;

        return dir.normalized;
    }
}
