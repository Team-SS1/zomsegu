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

        PlayerData fromData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (fromData == null || fromData.Inventory == null) return;

        InventorySlot fromSlot = fromData.Inventory.GetSlot(from.index);
        if (fromSlot == null || fromSlot.isEmpty) return;

        if (fromSlot.IsStack)
        {
            UIItemAmountPopup popup = UIManager.Instance.OpenUI<UIItemAmountPopup>();
            popup.OpenForGive(from, target, fromSlot.amount);

            return;
        }

        ItemTransferService.TryGiveToOtherPlayer(from, target);
    }
}
