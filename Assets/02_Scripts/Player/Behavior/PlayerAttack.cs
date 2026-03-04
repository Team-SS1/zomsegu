using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Player player;
    public bool IsAttacking {  get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();
    }
    void Start()
    {
        
    }
}
