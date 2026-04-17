using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ItemEnum;

public class ItemDropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] private UIItemAmountPopup amountPopup;
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
            if (amountPopup != null)
                amountPopup.OpenForDrop(from, fromSlot.amount);

            return;
        }

        ItemTransferService.TryDropOutside(from);
    }
}
