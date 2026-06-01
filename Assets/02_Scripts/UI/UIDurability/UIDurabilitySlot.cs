using UnityEngine;
using ItemEnum;
using UnityEngine.UI;
using TMPro;
using PlayerEnum;

public class UIDurabilitySlot : MonoBehaviour
{
    [Header("Slot Type")]
    [SerializeField] private EquipSlotType slotType;

    [Header("UI")]
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI durabilityTXT;

    [SerializeField] private UIDurabilityDamageFill damageFill;
    public void Refresh(PlayerType playerType)
    {
        int itemId = EquipmentQueryService.GetEquippedItemId(playerType, slotType);
        ItemStack instance = EquipmentQueryService.GetEquippedInstance(playerType, slotType);

        if(itemId == 0)
        {
            Clear();
            return;
        }

        SetIcon(itemId);

        if(instance != null && instance.HasDurability)
        {
            durabilityTXT.text = $"{instance.durability}/{instance.maxDurability}";
            
            if (damageFill != null)
                damageFill.SetDurability(instance);
        }
        else if(ItemDB.IsRangedWeapon(itemId))
        {
            PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
            if (playerData == null) durabilityTXT.text = "";
            Inventory inventory = playerData.Inventory;
            if (inventory == null) durabilityTXT.text = "";
            durabilityTXT.text = inventory.GetStackAmount(itemId)>0 ? inventory.GetStackAmount(itemId).ToString() : "";

            if (damageFill != null)
                damageFill.Hide();
        }
        else
        {
            durabilityTXT.text = "";

            if(damageFill != null)
                damageFill.Hide();
        }
    }
    private void Clear()
    {
        if(iconImg != null)
        {
            iconImg.sprite = null;
            iconImg.enabled = false;
        }
        if(durabilityTXT != null)
            durabilityTXT.text = "";

        if (damageFill != null)
            damageFill.Hide();
    }
    private void SetIcon(int itemId)
    {
        if (iconImg == null) return;

        string iconPath = ItemDB.GetIconPath(itemId);

        if (string.IsNullOrEmpty(iconPath))
        {
            iconImg.sprite = null;
            iconImg.enabled = false;
            return;
        }

        Sprite icon = Resources.Load<Sprite>(iconPath);
        iconImg.sprite = icon;
        iconImg.enabled = icon != null;
    }
}
