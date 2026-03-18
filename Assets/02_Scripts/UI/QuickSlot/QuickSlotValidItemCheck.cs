using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;

public class QuickSlotValidItemCheck : MonoBehaviour //이건 퀵슬롯 관련해서 장착 아이템 기준 아이템이 있나 체크용
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
