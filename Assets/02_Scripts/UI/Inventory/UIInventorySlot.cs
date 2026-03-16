using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerEnum;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.ShaderGraph.Internal;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Drag")]
    [SerializeField] private Vector2 dragIconOffset = new Vector2(30f, -30f);


    private GameObject dragIcon;
    private RectTransform dragIconRect;
    private Canvas rootCanvas;

    public SlotRef slotRef { get; private set; }

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if(canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetSlot(InventorySlot slot, int index, PlayerType playerType)
    {
        slotRef = SlotRef.Inv(playerType, index);

        if (slot == null || slot.isEmpty)
        {
            Clear();
            return;
        }

        CommonItemData itemData = ItemDB.GetCommon(slot.itemId);

        if(itemData != null && !string.IsNullOrEmpty(itemData.Icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>(itemData.Icon);

            if(iconSprite != null)
            {
                icon.enabled = true;
                icon.sprite = iconSprite;
            }
            else
            {
                icon.enabled = false;
                icon.sprite = null;
            }
        }
        else
        {
            icon.enabled = false;
            icon.sprite = null;
        }
        if (slot.IsStack) amountText.text = slot.amount.ToString();
        else if (slot.IsInstance&&slot.instance != null && slot.instance.HasDurability) amountText.text = $"{slot.instance.durability}/{slot.instance.maxDurability}";
        else amountText.text = "";
    }
    public void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        if (icon == null || icon.sprite == null) return;

        Inventory inventory = GetInventory();
        if (inventory == null) return;

        InventorySlot slot = inventory.GetSlot(slotRef.index);
        if(slot == null || slot.isEmpty) return;

        DragContext.payload = new DragPayload(slotRef);

        if(canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.4f;
        }

        CreateDragIcon(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        if(DragContext.payload == null) return;
        if(dragIconRect == null) return;

        dragIconRect.position = eventData.position + dragIconOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;

        ResetIcon();
        DestroyDragIcon();

        DragContext.payload = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if(DragContext.payload == null) return;

        DragContext.payload.SetTo(slotRef);

        ItemTransferService.TryTransferBetweenSlots(DragContext.payload);
    }

    private Inventory GetInventory()
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(slotRef.playerType);
        return data != null ? data.Inventory : null;
    }

    private void CreateDragIcon(Vector2 startPos)
    {
        DestroyDragIcon();

        if(rootCanvas == null || icon == null || icon.sprite == null) return;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        Image dragImage = dragIcon.AddComponent<Image>();
        dragImage.sprite = icon.sprite;
        dragImage.raycastTarget = false;
        dragImage.preserveAspect = true;

        dragIconRect = dragIcon.GetComponent<RectTransform>();

        RectTransform rect = icon.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = rect.rect.size;
        dragIconRect.position = startPos + dragIconOffset;
    }
    private void DestroyDragIcon()
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
            dragIconRect = null;
        }
    }
    private void ResetIcon()
    {
        if(canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
    }

}
