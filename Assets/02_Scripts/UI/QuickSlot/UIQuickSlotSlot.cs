using PlayerEnum;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ItemEnum;

public class UIQuickSlotSlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IDropHandler
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image selectedOutline; //선택시 빨간 테두리

    [Header("Drag")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector2 dragIconOffset = new Vector2(30f, -30f);

    [Header("Click")]
    [SerializeField] private float doubleClick = 0.25f;

    private float lastClickTime = -1f;

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
    public void SetSlot(QuickSlotSlot slot, int index, PlayerType playerType, bool isSelected)
    {
        slotRef = SlotRef.Quick(playerType, index);

        if (selectedOutline != null)
            selectedOutline.enabled = isSelected;

        if (slot == null || slot.isEmpty)
        {
            ClearSlot();
            return;
        }

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if (common != null && !string.IsNullOrEmpty(common.Icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>(common.Icon);
            icon.sprite = iconSprite;
            icon.enabled = iconSprite != null;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (slot.IsStack)
        {
            PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
            int amount = playerData != null && playerData.Inventory != null ?
                playerData.Inventory.GetStackAmount(slot.itemId) : 0;

            text.text = amount > 0 ? amount.ToString() : "0";
        }
        else if (slot.IsInstance)
        {
            PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
            ItemStack instance = ItemTransferCommon.FindInstancePlayer(playerData, slot.guid);

            if (instance != null && instance.HasDurability)
                text.text = $"{instance.durability}/{instance.maxDurability}";
            else
                text.text = "";
        }
        else
        {
            text.text = "";
        }
    }
    private void ClearSlot()
    {
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        if (text != null)
            text.text = "";
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (icon == null || icon.sprite == null) return;

        QuickSlot quickSlot = GetQuickSlot();
        if (quickSlot == null) return;

        QuickSlotSlot quickSlotSlot = quickSlot.GetSlot(slotRef.index);
        if (quickSlotSlot == null || quickSlotSlot.isEmpty) return;

        DragContext.payload = new DragPayload(slotRef);

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        SetIconAlpha(0.4f);
        CreateDragIcon(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (DragContext.payload == null || dragIconRect == null) return;

        dragIconRect.position = eventData.position + dragIconOffset;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragContext.payload == null) return;

        if(DragContext.payload.from.slotType == SlotType.QuickSlot ||
            DragContext.payload.from.slotType == SlotType.Inventory)
        {
            DragContext.payload.SetTo(slotRef);
            ItemTransferService.TryTransferBetweenSlots(DragContext.payload);
        }       
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (DragContext.payload != null && DragContext.payload.from.slotType == SlotType.QuickSlot)
        {
            bool droppedToQuickSlot =
                DragContext.payload.hasTo &&
                DragContext.payload.to.slotType == SlotType.QuickSlot;

            if (!droppedToQuickSlot)
                ItemTransferService.TryClearQuickSlot(DragContext.payload.from);
        }


        SetIconAlpha(1f);
        DestroyDragIcon();

        DragContext.payload = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ItemTransferService.TryClearQuickSlot(slotRef);
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.unscaledTime - lastClickTime <= doubleClick)
            {
                ItemTransferService.TryClearQuickSlot(slotRef);
                lastClickTime = -1f;
            }
            else
            {
                lastClickTime = Time.unscaledTime;
            }
        }
    }
    private QuickSlot GetQuickSlot()
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(slotRef.playerType);
        return playerData != null ? playerData.QuickSlot : null;
    }
    private void SetIconAlpha(float alpha)
    {
        if (icon == null) return;

        Color color = icon.color;
        color.a = alpha;
        icon.color = color;
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
}
