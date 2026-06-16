using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerEnum;

public class PlayTypeTest : MonoBehaviour
{
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;
        else if (keyboard.numpad5Key.wasPressedThisFrame)
            PlayerManager.Instance.SetGamePlayType(GamePlayType.PlaySHIN);
        else if (keyboard.numpad6Key.wasPressedThisFrame)
            PlayerManager.Instance.SetGamePlayType(GamePlayType.PlayHAN);
        else if(keyboard.numpad7Key.wasPressedThisFrame)
            PlayerManager.Instance.SetGamePlayType(GamePlayType.PlayBOTH);
    }
}
