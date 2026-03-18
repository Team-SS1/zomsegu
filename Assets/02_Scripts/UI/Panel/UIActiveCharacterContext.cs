using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;
public class UIActiveCharacterContext : MonoBehaviour //지금 액티브 플레이어 기준이 없기 때문에 임시로 만듬
{
    public PlayerType CurrentActivePlayer { get; private set; } = PlayerType.Player_SHIN;

    public void SetActivePlayer(PlayerType playerType)
    {
        if (CurrentActivePlayer == playerType) return;

        CurrentActivePlayer = playerType;
        EventManager.TriggerEvent(EventKey.ActiveCharacterChanged, playerType);
    }
}
