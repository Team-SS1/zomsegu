using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using ItemEnum;
using EventEnum;

public class UIEquipment : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("Slots")]
    [SerializeField] private UIEquipmentSlot headSlot;
    [SerializeField] private UIEquipmentSlot bodySlot;
    [SerializeField] private UIEquipmentSlot legSlot;
    [SerializeField] private UIEquipmentSlot shoesSlot;
    [SerializeField] private UIEquipmentSlot bagSlot;
    [SerializeField] private UIEquipmentSlot weaponSlot;
    [SerializeField] private UIEquipmentSlot accessory1Slot;
    [SerializeField] private UIEquipmentSlot accessory2Slot;

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspecCharacterChanged);

        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.EquipmentChanged, OnEquipmentChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
    }
    private void OnEquipmentChanged(PlayerType playerType)
    {
        if (playerType != selectedCharacterContext.CurrentInspectPlayer) return;
        Refresh(playerType);
    }
    private void OnInspectCharacterChanged(PlayerType playerType)
    {
        Refresh(playerType);
    }
    private void Refresh(PlayerType playerType)
    {
        PlayerData data = PlayerManager.Instance.GetPlayerData(playerType);
        if(data == null || data.Equipment == null)
        {
            ClearAll();
            return;
        }

        Equipment equipment = data.Equipment;

        headSlot.SetSlot(EquipSlotType.Head, playerType, equipment.GetSlot(EquipSlotType.Head));
        bodySlot.SetSlot(EquipSlotType.Body, playerType, equipment.GetSlot(EquipSlotType.Body));
        legSlot.SetSlot(EquipSlotType.Leg, playerType, equipment.GetSlot(EquipSlotType.Leg));
        shoesSlot.SetSlot(EquipSlotType.Shoes, playerType, equipment.GetSlot(EquipSlotType.Shoes));
        bagSlot.SetSlot(EquipSlotType.Bag, playerType, equipment.GetSlot(EquipSlotType.Bag));
        weaponSlot.SetSlot(EquipSlotType.Weapon, playerType, equipment.GetSlot(EquipSlotType.Weapon));
        accessory1Slot.SetSlot(EquipSlotType.Accessory1, playerType, equipment.GetSlot(EquipSlotType.Accessory1));
        accessory2Slot.SetSlot(EquipSlotType.Accessory2, playerType, equipment.GetSlot(EquipSlotType.Accessory2));
    }
    private void ClearAll()
    {
        headSlot.Clear();
        bodySlot.Clear();
        legSlot.Clear();
        shoesSlot.Clear();
        bagSlot.Clear();
        weaponSlot.Clear();
        accessory1Slot.Clear();
        accessory2Slot.Clear();
    }
}
