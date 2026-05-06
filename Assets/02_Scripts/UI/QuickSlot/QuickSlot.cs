using System;
using UnityEngine;
using PlayerEnum;
using EventEnum;

[Serializable]
public class QuickSlot
{
    public int Capacity { get; private set; } = 5;
    public int SelectedIndex { get; private set; } = 0;

    private readonly QuickSlotSlot[] slots;
    private readonly PlayerType playerType;

    public QuickSlot(PlayerType playerType, int capacity = 5)
    {
        this.playerType = playerType;
        Capacity = Mathf.Max(1, capacity);

        slots = new QuickSlotSlot[Capacity];
        for(int i = 0; i< Capacity; i++)
            slots[i] = new QuickSlotSlot();
    }

    public void SetSelectedIndex(int index)
    {
        if(index < 0 || index >= Capacity) return;
        if(SelectedIndex == index) return;

        SelectedIndex = index;
        NotifyChanged();
    }
    public bool BindStack(int index, int itemId)
    {
        if (index < 0 || index >= Capacity) return false;
        if (itemId ==0) return false;

        slots[index].SetStack(itemId);
        NotifyChanged();
        return true;
    }
    public bool BindInstance(int index, ItemStack item)
    {
        if (index < 0 || index >= Capacity) return false;
        if(item == null) return false;

        slots[index].SetInstance(item);
        NotifyChanged();
        return true;
    }
    public bool ClearSlot(int index)
    {
        if(index <0 || index >= Capacity) return false;
        if (slots[index].isEmpty) return false;

        slots[index].Clear();
        NotifyChanged();
        return true;
    }
    public bool Swap(int index1, int index2)
    {
        if (index1 < 0 || index1 >= Capacity || index2 < 0 || index2 >= Capacity) return false;
        if (index1 == index2) return true;

        (slots[index1], slots[index2]) = (slots[index2], slots[index1]);
        NotifyChanged();
        return true;
    }
    public int FindSlotByItemId(int itemId) //퀵슬롯에서 슬롯 이동을 위함
    {
        if (itemId == 0) return -1;

        for(int i = 0; i < Capacity; i++)
        {
            if (!slots[i].isEmpty && slots[i].itemId == itemId) return i;
        }
        return -1;
    }
    public QuickSlotSlot GetSlot(int index)
    {
        if(index < 0 || index >= Capacity) return null;
        return slots[index];
    }
    public int FindSlotByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return -1;

        for (int i = 0; i < Capacity; i++)
        {
            if (!slots[i].isEmpty && slots[i].guid == guid) return i;
        }
        return -1;
    }
    private void NotifyChanged()
    {
        EventManager.TriggerEvent(EventKey.QuickSlotChanged, playerType);
    }
}
