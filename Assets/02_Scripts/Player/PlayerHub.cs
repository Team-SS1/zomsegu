using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHub : MonoBehaviour
{
    private Player player;
    private PlayerAttack attack;
    private PlayerAim aim;
    private PlayerController controller;
    private PlayerCondition condition;
    private SpriteController spriteController;
    private PlayerAnimationHandler animationHandler;

    public Player Player => player;
    public PlayerController Controller => controller;
    public PlayerAttack Attack => attack;
    public PlayerAim Aim => aim;
    public PlayerCondition Condition => condition;
    public SpriteController SpriteController => spriteController;
    public PlayerAnimationHandler AnimationHandler => animationHandler;

    private void Awake()
    {
        player = GetComponent<Player>();
        attack = GetComponent<PlayerAttack>();
        aim = GetComponent<PlayerAim>();
        controller = GetComponent<PlayerController>();
        condition = GetComponent<PlayerCondition>();
        spriteController = GetComponentInChildren<SpriteController>();
        animationHandler = GetComponent<PlayerAnimationHandler>();
    }
}
