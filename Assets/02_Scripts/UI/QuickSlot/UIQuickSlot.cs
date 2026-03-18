using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PlayerEnum;
using ItemEnum;
using TMPro;
using System.Runtime.CompilerServices;

public class UIQuickSlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IDropHandler
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

    public SlotRef slotRef {  get; private set; }

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if(canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }
    public void SetSlot(QuickSlotSlot slot, int index, PlayerType playerType, bool isSelected)
    {
        slotRef = SlotRef.Quick(playerType, index);

        if (selectedOutline != null)
            selectedOutline.enabled = isSelected;

        if(slot == null || slot.isEmpty)
        {
            ClearSlot();
            return;
        }

        CommonItemData common = ItemDB.GetCommon(slot.itemId);
        if(common != null && !string.IsNullOrEmpty(common.Icon))
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
            PlayerData playerData = PlayerManager.Instance.GetPlayerData(playerType);
            int amount = playerData != null && playerData.Inventory != null ?
                playerData.Inventory.GetStackAmount(slot.itemId) : 0;

            text.text = amount > 0 ? amount.ToString() : "0";
        }else if (slot.IsInstance)
        {
            PlayerData playerData = PlayerManager.Instance.GetPlayerData(playerType);
            ItemStack instance = FindInstance(playerData, slot.guid);

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
    private ItemStack FindInstance(PlayerData playerData, string guid)
    {
        if (playerData == null || playerData.Inventory == null || string.IsNullOrEmpty(guid)) return null;

        int index = playerData.Inventory.FindIndexByGuid(guid);
        if(index < 0) return null;

        InventorySlot slot = playerData.Inventory.GetSlot(index);
        if (slot == null || slot.IsStack) return null;

        return slot.instance;
    }
    private void ClearSlot()
    {
        if(icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        if (text != null)
            text.text = "";
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrop(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
