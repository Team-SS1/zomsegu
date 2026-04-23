using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerEnum;
using TMPro;
using UnityEngine.EventSystems;
using ItemEnum;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Drag")]
    [SerializeField] private Vector2 dragIconOffset = new Vector2(30f, -30f);

    [Header("Click")]
    [SerializeField] private float doubleClick = 0.25f;

    [SerializeField] private UITooltipManage toolTipManage;

    private float lastClickTime = -1f;

    private GameObject dragIcon;
    private RectTransform dragIconRect;
    private Canvas rootCanvas;

    private UIInventory uiInventory;

    public SlotRef slotRef { get; private set; }

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        uiInventory = GetComponentInParent<UIInventory>();

        if (canvasGroup == null)
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

        if (itemData != null && !string.IsNullOrEmpty(itemData.Icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>(itemData.Icon);

            if (iconSprite != null)
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
        else if (slot.IsInstance && slot.instance != null && slot.instance.HasDurability) amountText.text = $"{slot.instance.durability}/{slot.instance.maxDurability}";
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
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (icon == null || icon.sprite == null) return;

        Inventory inventory = GetInventory();
        if (inventory == null) return;

        InventorySlot slot = inventory.GetSlot(slotRef.index);
        if (slot == null || slot.isEmpty) return;

        DragContext.payload = new DragPayload(slotRef);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
        SetIconAlpha(0.4f);

        CreateDragIcon(eventData.position);
    }
    private void SetIconAlpha(float alpha)
    {
        Color color = icon.color;
        color.a = alpha;
        icon.color = color;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (DragContext.payload == null) return;
        if (dragIconRect == null) return;

        dragIconRect.position = eventData.position + dragIconOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        ResetIcon();
        DestroyDragIcon();

        DragContext.payload = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragContext.payload == null) return;

        SlotRef from = DragContext.payload.from;

        bool wasFiltered = uiInventory != null && uiInventory.IsFiltered;

        if (from.slotType == SlotType.Equipment && uiInventory != null)
        {
            int equippedItemId = ItemTransferService.GetEquippedItemId(from);
            if (equippedItemId != 0)
                uiInventory.AdjustFilterBeforeUnEquip(equippedItemId);
        }
        if (wasFiltered &&
            from.slotType == SlotType.Equipment)
        {
            bool success = ItemTransferService.TryUnEquipToFirstEmptyInventory(from);

            if(success && uiInventory != null)
                uiInventory.Refresh(slotRef.playerType);  

            return;
        }

        DragContext.payload.SetTo(slotRef);

        ItemTransferService.TryTransferBetweenSlots(DragContext.payload);
    }

    private Inventory GetInventory()
    {
        PlayerData data = PlayerDataManager.Instance.GetPlayerData(slotRef.playerType);
        return data != null ? data.Inventory : null;
    }

    private void CreateDragIcon(Vector2 startPos)
    {
        DestroyDragIcon();

        if (rootCanvas == null || icon == null || icon.sprite == null) return;

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
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
        SetIconAlpha(1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (Time.unscaledTime - lastClickTime <= doubleClick)
        {
            ItemTransferService.TryUseOrEquipFromInventory(slotRef);
            lastClickTime = -1f;
        }
        else
        {
            lastClickTime = Time.unscaledTime;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Inventory inventory = GetInventory();
        if (inventory == null) return;

        InventorySlot slot = inventory.GetSlot(slotRef.index);
        if (slot == null || slot.isEmpty) return;

        int compareItemId = 0;
        ItemStack compareInstance = null;

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if(common != null)
        {
            ItemType itemType = (ItemType)common.ItemType;
            PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(slotRef.playerType);

            if(playerData != null&&playerData.Equipment != null)
            {
                if(itemType == ItemType.Weapon)
                {
                    EquipmentSlot equipSlot = playerData.Equipment.GetSlot(EquipSlotType.Weapon);
                    if(equipSlot != null && !equipSlot.isEmpty)
                    {
                        compareItemId = equipSlot.GetItemId();
                        compareInstance = equipSlot.equippedItem;
                    }
                }
                else if(itemType == ItemType.Shoes)
                {
                    EquipmentSlot equipmentSlot = playerData.Equipment.GetSlot(EquipSlotType.Shoes);
                    if(equipmentSlot != null && !equipmentSlot.isEmpty)
                    {
                        compareItemId = equipmentSlot.GetItemId();
                        compareInstance = equipmentSlot.equippedItem;
                    }
                }
                else if(itemType == ItemType.Bag)
                {
                    EquipmentSlot equipmentSlot = playerData.Equipment.GetSlot(EquipSlotType.Bag);
                    if(equipmentSlot != null && !equipmentSlot.isEmpty)
                    {
                        compareItemId = equipmentSlot.GetItemId();
                        compareInstance = equipmentSlot.equippedItem;
                    }
                }
            }       
        }

        toolTipManage?.ShowInventoryTooltip(
            transform as RectTransform,
            slot.itemId,
            slot.instance,
            compareItemId,
            compareInstance
            );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTipManage?.HideAll();
    }
}
