using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;

public class UIInventory : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("Player Ref")]
    [SerializeField] private Player playerShin;
    [SerializeField] private Player playerHan;

    [Header("Slot UI")]
    [SerializeField] private UIInventorySlot[] slotUIs;

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
    }
    private void OnInventoryChanged(PlayerType player)
    {
        if (player != selectedCharacterContext.CurrentInspectPlayer) return;
        Refresh(player);
    }
    public void Refresh(PlayerType player)
    {

    }
}