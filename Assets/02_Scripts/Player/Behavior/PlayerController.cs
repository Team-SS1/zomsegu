using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Rigidbody2D Rb { get; private set; }
    public Collider2D Co { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Co = GetComponent<Collider2D>();
    }
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //Vector2 dir; // Input의 Vector2 값을 받아 사용
        //float speed; // 이동속도의 계산값을 받아 사용
        //Rb.velocity = dir * speed;
    }
}
