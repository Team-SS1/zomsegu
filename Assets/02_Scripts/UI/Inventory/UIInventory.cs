using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;
using UnityEngine.UI;
using ItemEnum;
using System.Linq;

public class UIInventory : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("Slot UI")]
    [SerializeField] private UIInventorySlot[] slotUIs;

    [Header("Filter Buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button weaponButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button accessoryButton;
    [SerializeField] private Button consumableButton;
    [SerializeField] private Button miscButton;
    [SerializeField] private Button sortButton;

    [Header("Button Colors")]
    [SerializeField]
    private Color normalButtonColor =
    new Color(118f / 255f, 113f / 255f, 113f / 255f);

    [SerializeField]
    private Color selectedButtonColor =
        new Color(164f / 255f, 164f / 255f, 164f / 255f);

    private InventoryFilterType currentFilter = InventoryFilterType.All;
    public bool IsFiltered => currentFilter != InventoryFilterType.All;

    private void Awake()
    {
        if (allButton != null) allButton.onClick.AddListener(OnClickAll);
        if (weaponButton != null) weaponButton.onClick.AddListener(OnClickWeapon);
        if (equipmentButton != null) equipmentButton.onClick.AddListener(OnClickEquipment);
        if (accessoryButton != null) accessoryButton.onClick.AddListener(OnClickAccessory);
        if (consumableButton != null) consumableButton.onClick.AddListener(OnClickConsumable);
        if (miscButton != null) miscButton.onClick.AddListener(OnClickMisc);
        if (sortButton != null) sortButton.onClick.AddListener(OnClickSort);
    }

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
        UpdateButtonColors();
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
    }
    private void OnInventoryChanged(PlayerType player)
    {
        if (player != selectedCharacterContext.CurrentInspectPlayer) return;
        Refresh(player);
    }
    private void OnInspectCharacterChanged(PlayerType player)
    {
        Refresh(player);
    }
    public void Refresh(PlayerType player)
    {
        Inventory inventory = GetInventory(player);
        if (inventory == null)
        {
            ClearAllSlots();
            return;
        }

        List<int> visibleIndices = BuildVisibleIndices(inventory);
        int showCount = Mathf.Min(slotUIs.Length, visibleIndices.Count);

        for(int i = 0; i< showCount; i++)
        {
            int realIndex = visibleIndices[i];
            InventorySlot slot = inventory.GetSlot(realIndex);
            slotUIs[i].SetSlot(slot, realIndex, player);
        }
        for(int i = showCount; i < slotUIs.Length; i++)
        {
            slotUIs[i].Clear();
        }

        UpdateButtonColors();
    }
    private Inventory GetInventory(PlayerType player)
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(player);
        return data != null ? data.Inventory : null;
    }
    private void ClearAllSlots()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i].Clear();
        }
    }
    //필터 관련 코드들
    private List<int> BuildVisibleIndices(Inventory inventory)
    {
        List<int> indices = new List<int>();  // 정렬 아이템 인덱스

        if(currentFilter == InventoryFilterType.All)
        {
            for(int i = 0; i<inventory.Capacity&&indices.Count < slotUIs.Length; i++)
            {
                indices.Add(i);
            }

            return indices;
        }

        for(int i = 0; i < inventory.Capacity; i++)
        {
            InventorySlot slot = inventory.GetSlot(i);
            if (slot == null || slot.isEmpty) continue;

            if(IsMatchFilter(slot))
                indices.Add(i);
        }
        indices = indices.OrderBy(i => inventory.GetSlot(i).itemId).ThenBy(i => i).ToList();

        int lastOccupidIndex = -1; //전체 기준 마지막 아이템 위치
        for (int i = inventory.Capacity -1; i>=0; i--)
        {
            InventorySlot slot = inventory.GetSlot(i);
            if(slot != null && !slot.isEmpty)
            {
                lastOccupidIndex = i;
                break;
            }
        }
        for (int i = lastOccupidIndex + 1; i < inventory.Capacity && indices.Count < slotUIs.Length; i++) //마지막 아이템 뒤쪽 빈 슬롯부터 추가
        {
            InventorySlot slot = inventory.GetSlot(i);
            if(slot != null && slot.isEmpty)
                indices.Add(i);
        }
            return indices;
    }
    private bool IsMatchFilter(InventorySlot slot) //정렬필터에 맞는 아이템인지 구분
    {
        if(slot == null || slot.isEmpty) return false;

        if (currentFilter == InventoryFilterType.All) return true;

        CommonItemData itemData = ItemDB.GetCommon(slot.itemId);
        if(itemData == null) return false;

        ItemType itemType = (ItemType)itemData.ItemType;

        switch (currentFilter)
        {
            case InventoryFilterType.Weapon:
                return itemType == ItemType.Weapon;

            case InventoryFilterType.Equipment:
                return itemType == ItemType.Head ||
                    itemType == ItemType.Body ||
                    itemType == ItemType.Leg;

            case InventoryFilterType.Accessory:
                return itemType == ItemType.Accessory ||
                    itemType == ItemType.Bag ||
                    itemType == ItemType.Shoes;

            case InventoryFilterType.Consumable:
                return itemType == ItemType.Consumable;

            case InventoryFilterType.Misc:
                return itemType == ItemType.Misc;
        }
        return false;
    }
    private void UpdateButtonColors()
    {
        SetButtonColor(allButton, currentFilter == InventoryFilterType.All);
        SetButtonColor(weaponButton, currentFilter == InventoryFilterType.Weapon);
        SetButtonColor(equipmentButton, currentFilter == InventoryFilterType.Equipment);
        SetButtonColor(consumableButton, currentFilter == InventoryFilterType.Consumable);
        SetButtonColor(accessoryButton, currentFilter == InventoryFilterType.Accessory);
        SetButtonColor(miscButton, currentFilter == InventoryFilterType.Misc);

        SetButtonColor(sortButton, false);
    }
    private void SetButtonColor(Button button, bool isSelected)
    {
        if (button == null || button.image == null) return;

        button.image.color = isSelected ? selectedButtonColor : normalButtonColor;
    }
    public void OnClickAll()
    {
        currentFilter = InventoryFilterType.All;
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    public void OnClickWeapon()
    {
        currentFilter = InventoryFilterType.Weapon;
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    public void OnClickEquipment()
    {
        currentFilter = InventoryFilterType.Equipment;
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    public void OnClickConsumable()
    {
        currentFilter = InventoryFilterType.Consumable;
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    public void OnClickMisc()
    {
        currentFilter = InventoryFilterType.Misc;
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    public void OnClickAccessory()
    {
        currentFilter = InventoryFilterType.Accessory;
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    public void OnClickSort()
    {
        Inventory inventory = GetInventory(selectedCharacterContext.CurrentInspectPlayer);
        if (inventory == null) return;

        inventory.SortByItemId();

        currentFilter = InventoryFilterType.All;
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
}