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
            int equippedItemId = EquipmentQueryService.GetEquippedItemId(from.playerType, from.equipSlot);
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

        if (icon == null || icon.sprite == null) return;

        Transform parent = UIDragIconRoot.Root;

        if(parent == null)
        {
            if(rootCanvas == null)
                rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null)
                return;
            parent = rootCanvas.transform;
        }


        dragIcon = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasRenderer),typeof(Image));
        dragIcon.transform.SetParent(parent, false);
        dragIcon.transform.SetAsLastSibling();

        Image dragImage = dragIcon.GetComponent<Image>();
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
            
            if(TryGetCompareEquipSlot(itemType, out EquipSlotType equipSlotType))
            {
                compareItemId = EquipmentQueryService.GetEquippedItemId(slotRef.playerType, equipSlotType);
                compareInstance = EquipmentQueryService.GetEquippedInstance(slotRef.playerType, equipSlotType);
            }
            
        }
        UITooltipManage toolTipManage = UIManager.Instance.GetUI<UITooltipManage>();
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
        UIManager.Instance.GetUI<UITooltipManage>()?.HideAll();
    }
    private bool TryGetCompareEquipSlot(ItemType itemType, out EquipSlotType equipSlotType)
    {
        equipSlotType = EquipSlotType.Weapon;

        switch (itemType)
        {
            case ItemType.Weapon:
                equipSlotType = EquipSlotType.Weapon;
                return true;
            case ItemType.Shoes:
                equipSlotType = EquipSlotType.Shoes;
                return true;
            case ItemType.Bag:
                equipSlotType = EquipSlotType.Bag;
                return true;
            default:
                return false;
        }
    }
}
