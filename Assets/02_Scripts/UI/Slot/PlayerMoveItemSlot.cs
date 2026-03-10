using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using UnityEngine.EventSystems;

public class PlayerMoveItemSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;
    public void OnDrop(PointerEventData eventData)
    {
        if(DragContext.payload == null) return;

        SlotRef from = DragContext.payload.from;

        PlayerType current = selectedCharacterContext.CurrentInspectPlayer;
        PlayerType target = current == PlayerType.Player_SHIN ? PlayerType.Player_HAN : PlayerType.Player_SHIN;

        ItemTransferService.TryGiveToOtherPlayer(from, target);
    }
}
