using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerEnum;
using TMPro;
using Unity.VisualScripting;

public class UIInventorySlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;

    public SlotRef slotRef { get; private set; }

    private int slotIndex;
    private PlayerType player;

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
        else if (slot.IsInstance&&slot.instance.HasDurability) amountText.text = $"{slot.instance.durability}/{slot.instance.maxDurability}";
        else amountText.text = "";
    }
    public void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";
    }
}
