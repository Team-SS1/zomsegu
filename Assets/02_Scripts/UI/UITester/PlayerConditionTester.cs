using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
public class PlayerConditionTester : MonoBehaviour // 게이지 UI가 제대로 작동하는지 확인하기 위해 만든 스크립트
{
    [Header("Reference")]
    [SerializeField] private PlayerCondition playerCondition;

    [Header("Preset")]
    [SerializeField] private int dangerValue = 3;
    [SerializeField] private int zeroValue = 0;

    private void Update()
    {
        if (playerCondition == null) return;

        if (Input.GetKeyDown(KeyCode.Keypad7)) SetToMax(AbnormalType.Hunger);
        else if(Input.GetKeyDown(KeyCode.Keypad8)) SetToDanger(AbnormalType.Hunger);
        else if (Input.GetKeyDown(KeyCode.Keypad9)) SetToZero(AbnormalType.Hunger);
        else if(Input.GetKeyDown(KeyCode.Keypad4)) SetToMax(AbnormalType.Thirst);
        else if(Input.GetKeyDown(KeyCode.Keypad5)) SetToDanger(AbnormalType.Thirst);
        else if (Input.GetKeyDown(KeyCode.Keypad6)) SetToZero(AbnormalType.Thirst);
        else if(Input.GetKeyDown(KeyCode.Keypad1)) SetToMax(AbnormalType.Tired);
        else if(Input.GetKeyDown(KeyCode.Keypad2)) SetToDanger(AbnormalType.Tired);
        else if (Input.GetKeyDown(KeyCode.Keypad3)) SetToZero(AbnormalType.Tired);
    }
    private void SetToMax(AbnormalType type)
    {
        Debug.Log($"Set {type} to Max");
        int maxValue = playerCondition.GetAbnormal(type)?.MaxValue ?? 0;
        playerCondition.SetValue(type, maxValue);
    }
    private void SetToDanger(AbnormalType type)
    {
        Debug.Log($"Set {type} to Danger");
        playerCondition.SetValue(type, dangerValue);
    }
    private void SetToZero(AbnormalType type)
    {
        Debug.Log($"Set {type} to Zero");
        playerCondition.SetValue(type, zeroValue);
    }
}
