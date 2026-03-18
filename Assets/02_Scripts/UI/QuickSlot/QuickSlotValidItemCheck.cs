using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;

public class QuickSlotValidItemCheck : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);
    }
    private void OnEquipmentChanged(PlayerType playerType)
    {
        ItemTransferService.ValidateQuickSlots(playerType);
    }
}
