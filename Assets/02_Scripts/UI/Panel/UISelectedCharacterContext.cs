using PlayerEnum;
using UnityEngine;
using EventEnum;

public class UISelectedCharacterContext : MonoBehaviour
{
    public PlayerType CurrentInspectPlayer { get; private set; } = PlayerType.Player_SHIN; // 현재 선택된 캐릭터(플레이 대상 X)

    public void SetInspectPlayer(PlayerType player)
    {
        if (CurrentInspectPlayer == player) return;

        CurrentInspectPlayer = player;
        EventManager.TriggerEvent(EventKey.InspectCharacterChanged, player);
    }
}
