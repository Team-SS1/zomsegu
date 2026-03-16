using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlayerEnum;
using ItemEnum;
using TMPro;
using Newtonsoft.Json.Bson;

public class UIEquipmentSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountTXT;

    [Header("BG")]
    [SerializeField] private Image bgImg;
    [SerializeField] private Sprite normalBG;
    [SerializeField] private Sprite equipBG;

    [Header("Drag")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector2 dragIconOffset = new Vector2(30f, -30f);

    private GameObject dragIcon;
    private RectTransform dragIconRect;
    private Canvas rootCanvas;
    public SlotRef slotRef { get; private set; }

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetSlot(EquipSlotType equipSlotType, PlayerType playerType, EquipmentSlot slot)
    {
        slotRef = SlotRef.Equip(playerType, equipSlotType);

        if (slot == null || slot.isEmpty)
        {
            Clear();
            return;
        }

        int itemId = slot.GetItemId();
        if (itemId == 0)
        {
            Clear();
            return;
        }
        CommonItemData itemData = ItemDB.GetCommon(itemId);

        if (itemData != null && !string.IsNullOrEmpty(itemData.Icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>(itemData.Icon);
            icon.sprite = iconSprite;
            icon.enabled = iconSprite != null;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (slot.HasInstance && slot.equippedItem != null && slot.equippedItem.HasDurability)
            amountTXT.text = $"{slot.equippedItem.durability}/{slot.equippedItem.maxDurability}";
        else if (slot.HasRangedWeapon)
        {
            PlayerData data = PlayerManager.Instance.GetPlayerData(playerType);
            if (data != null && data.Inventory != null)
            {
                int amount = data.Inventory.GetStackAmount(slot.rangedWeaponItem);
                amountTXT.text = amount > 0 ? amount.ToString() : "";
            }
        }
        else
            amountTXT.text = "";
        SetBG(true);
    }
    private void SetBG(bool equipped)
    {
        if (bgImg == null) return;
        bgImg.sprite = equipped ? equipBG : normalBG;
    }
    public void Clear()
    {
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        if (amountTXT != null)
            amountTXT.text = "";

        SetBG(false);
    }
    private Equipment GetEquipment()
    {
        PlayerData playerData = PlayerManager.Instance.GetPlayerData(slotRef.playerType);
        return playerData != null ? playerData.Equipment : null;
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (DragContext.payload == null) return;

        DragContext.payload.SetTo(slotRef);
        ItemTransferService.TryTransferBetweenSlots(DragContext.payload);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (icon == null || icon.sprite == null) return;

        Equipment equipment = GetEquipment();
        if (equipment == null) return;

        EquipmentSlot equipmentSlot = equipment.GetSlot(slotRef.equipSlot);
        if (equipmentSlot == null || equipmentSlot.isEmpty) return;

        DragContext.payload = new DragPayload(slotRef);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.4f;
        }
        CreateDragIcon(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        ResetIcon();
        DestroyDragIcon();
        
        DragContext.payload = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (DragContext.payload == null || dragIconRect == null) return;

        dragIconRect.position = eventData.position + dragIconOffset;
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
        if(dragIcon != null)
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
