using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;

public class UIInventory : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("Slot UI")]
    [SerializeField] private UIInventorySlot[] slotUIs;

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
        Refresh(selectedCharacterContext.CurrentInspectPlayer);
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

        int inventorySize = Mathf.Min(slotUIs.Length, inventory.Capacity);

        for(int i = 0; i< inventorySize; i++)
        {
            InventorySlot slot = inventory.GetSlot(i);
            slotUIs[i].SetSlot(slot, i, player);
        }
        for(int i = inventorySize; i < slotUIs.Length; i++)
        {
            slotUIs[i].Clear();
        }

    }
    private Inventory GetInventory(PlayerType player)
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(player);
        return data != null ? data.Inventory : null;
    }
    private void ClearAllSlots()
    {
        for(int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i].Clear();
        }
    }
}