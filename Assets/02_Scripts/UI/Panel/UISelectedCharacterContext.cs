using PlayerEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISelectedCharacterContext : MonoBehaviour
{
    public PlayerType CurrentInspectPlayer { get; private set; } = PlayerType.Player_SHIN; // 현재 선택된 캐릭터(플레이 대상 X)

    public void SetInspectPlayer(PlayerType player)
    {
        CurrentInspectPlayer = player;
    }
}
