using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlayerEnum;
using ItemEnum;
using TMPro;
using Newtonsoft.Json.Bson;

public class UIEquipmentSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountTXT;

    [Header("BG")]
    [SerializeField] private Image bgImg;
    [SerializeField] private Sprite normalBG;
    [SerializeField] private Sprite equipBG;
    public SlotRef slotRef { get; private set; }

    public void SetSlot(EquipSlotType equipSlotType, PlayerType playerType, EquipmentSlot slot)
    {
        slotRef = SlotRef.Equip(playerType, equipSlotType);

        if (slot == null || slot.isEmpty)
        {
            Clear();
            return;
        }

        int itemId = slot.GetItemId();
        if(itemId == 0)
        {
            Clear();
            return;
        }
        CommonItemData itemData = ItemDB.GetCommon(itemId);

        if(itemData != null && !string.IsNullOrEmpty(itemData.Icon))
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
            if(data != null && data.Inventory != null)
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
        if(icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        if (amountTXT != null)
            amountTXT.text = "";

        SetBG(false);
    }
    public void OnDrop(PointerEventData eventData)
    {
        if(DragContext.payload == null) return;

        DragContext.payload.SetTo(slotRef);
        ItemTransferService.TryTransferBetweenSlots(DragContext.payload);
    }
}
