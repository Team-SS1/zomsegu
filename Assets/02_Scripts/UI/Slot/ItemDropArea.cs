using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (DragContext.payload == null) return;

        ItemTransferService.TryDromOutside(DragContext.payload.from);
    }
}
