using UnityEngine;
using UnityEngine.EventSystems;
using ItemEnum;

public class ItemDropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (DragContext.payload == null) return;

        SlotRef from = DragContext.payload.from;

        if(from.slotType != SlotType.Inventory)
        {
            ItemTransferService.TryDropOutside(from);
            return;
        }

        PlayerData fromData = PlayerDataManager.Instance.GetPlayerData(from.playerType);
        if (fromData == null || fromData.Inventory == null) return;

        InventorySlot fromSlot = fromData.Inventory.GetSlot(from.index);
        if(fromSlot == null || fromSlot.isEmpty) return;

        if (fromSlot.IsStack)
        {
            Debug.Log($"[ItemDropArea] AmountPopup 열기 시도 amount={fromSlot.amount}");
            UIItemAmountPopup popup = UIManager.Instance.OpenUI<UIItemAmountPopup>();
            Debug.Log($"[ItemDropArea] popup null 여부 = {popup == null}");
            popup.OpenForDrop(from, fromSlot.amount);

            return;
        }

        ItemTransferService.TryDropOutside(from);
    }
}
