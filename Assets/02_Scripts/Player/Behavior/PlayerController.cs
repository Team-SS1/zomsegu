using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int movement;

    public Rigidbody2D rb;
    public Collider2D co;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        co = GetComponent<Collider2D>();
    }
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {

    }
}
