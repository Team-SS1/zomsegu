using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PlayerEnum;
using EventEnum;

public class UICalculateWeightVolume : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;
    [SerializeField] private TextMeshProUGUI text;
    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectorChanged);

        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectorChanged);
    }
    private void OnInventoryChanged(PlayerType playerType)
    {
        if (playerType != selectedCharacterContext.CurrentInspectPlayer) return;
        Refresh(playerType);
    }
    private void OnInspectorChanged(PlayerType playerType)
    {
        Refresh(playerType);
    }
    private void Refresh(PlayerType playerType)
    {
        Inventory inventory = GetInventory(playerType);
        if (inventory == null) return;

        if (text != null)
            text.text = $"용량 {inventory.CurrentVolume:0.#}/{inventory.MaxVolume} 무게 {inventory.CurrentWeight:0.#}/{inventory.MaxWeight}kg";
    }
    private Inventory GetInventory(PlayerType playerType)
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        return playerData !=null ? playerData.Inventory : null;
    }
}
