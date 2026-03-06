using PlayerEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public int Capacity { get; private set; } = 80; // 최대 슬롯 수
    public float MaxWeight { get; private set; } = 40f; // 최대 무게
    public float MaxVolume { get; private set; } = 30f; // 최대 용량

    public float CurrentWeight { get; private set; }
    public float CurrentVolume { get; private set; }

    private readonly InventorySlot[] slots; //메인 저장소

    private readonly Dictionary<string, int> guidToIndex = new(); //인스턴스형 슬롯 인덱스
    private readonly Dictionary<int, int> stackToIndex = new(); //스택형 슬롯 인덱스

    private readonly PlayerType playerType; //플레이어 타입

    public Inventory(PlayerType player, int capacity = 80, float maxWeight = 40f, float maxVolume = 30f)
    {
        this.playerType = player;
        Capacity = Mathf.Max(1, capacity);
        MaxWeight = maxWeight;
        MaxVolume = maxVolume;

        slots = new InventorySlot[Capacity];
        for (int i = 0; i < Capacity; i++) slots[i] = new InventorySlot();

        RebuildIndicesAndCapacity();
    }

    // 인벤토리 아이템 정보
    public InventorySlot GetSlot(int index) => slots[index];
    public IReadOnlyList<InventorySlot> GetAllSlots() => System.Array.AsReadOnly(slots);

    private bool CanAddByCapacity(float addWeight, float addVolume) //용량, 무게 체크
        => (CurrentWeight + addWeight <= MaxWeight) && (CurrentVolume + addVolume <= MaxVolume);
    private int FindEmptySlot() // 빈 슬롯 찾기
    {
        for (int i = 0; i< Capacity; i++)if(slots[i].isEmpty) return i;

        return -1;
    }
    public bool TryAddStack(int itemId, int amount) //스택형 아이템 추가
    {
        if(amount <= 0) return false;
        if(!ItemDB.IsStackable(itemId)) return false;

        float addWeight = ItemDB.GetWeight(itemId) * amount;
        float addVolume = ItemDB.GetVolume(itemId) * amount;

        if(!CanAddByCapacity(addWeight, addVolume)) return false;

        if(stackToIndex.TryGetValue(itemId, out var index)) //이미 존재하는 스택형 아이템이 있다면
        {
            slots[index].amount += amount;
        }
        else //새로운 스택형 아이템 추가
        {
            int emptyIndex = FindEmptySlot();
            if(emptyIndex < 0) return false; //빈 슬롯 없음

            slots[emptyIndex].itemId = itemId;
            slots[emptyIndex].amount = amount;
            slots[emptyIndex].instance = null; //스택형 아이템이므로 인스턴스는 null

            stackToIndex[itemId] = emptyIndex;
        }

        CurrentWeight += addWeight;
        CurrentVolume += addVolume;

        NotifyChanged();
        return true;
    }
    public bool TryAddInstance(int itemId, ItemStack instance) // 외부에서 인스턴스형 아이템 추가
    {
        if(instance == null || instance.itemId != itemId) return false;
        if(ItemDB.IsStackable(itemId)) return false;
        
        float addWeight = ItemDB.GetWeight(itemId);
        float addVolume = ItemDB.GetVolume(itemId);

        if(!CanAddByCapacity(addWeight, addVolume)) return false;

        int emptyIndex = FindEmptySlot();
        if(emptyIndex < 0) return false; //빈 슬롯 없음

        if(string.IsNullOrEmpty(instance.guid) || guidToIndex.ContainsKey(instance.guid)) // guid 중복 방지
        {
            instance.guid = ItemGuid.NewGuid(); //새로운 GUID 생성
            while (guidToIndex.ContainsKey(instance.guid)) instance.guid = ItemGuid.NewGuid(); //중복되지 않을 때까지 반복
        }
        slots[emptyIndex].itemId = instance.itemId;
        slots[emptyIndex].amount = 0;
        slots[emptyIndex].instance = instance;

        guidToIndex[instance.guid] = emptyIndex;

        CurrentWeight += addWeight;
        CurrentVolume += addVolume;

        NotifyChanged();
        return true;
    }
    public bool TryAddNewInstance(int itemId) //인스턴스형 아이템을 새로 생성하여 추가하는 메서드
    {
        if(ItemDB.IsStackable(itemId)) return false;

        float addWeight = ItemDB.GetWeight(itemId);
        float addVolume = ItemDB.GetVolume(itemId);

        if(!CanAddByCapacity(addWeight, addVolume)) return false;

        int emptyIndex = FindEmptySlot();
        if(emptyIndex < 0) return false; //빈 슬롯 없음

        int maxDurability = ItemDB.GetDefaultDurability(itemId);
        int currentDurability = maxDurability; // 새로 생성하는 아이템은 내구도가 최대인 상태로 시작

        ItemStack inst = new ItemStack(itemId, currentDurability, maxDurability);
        while(guidToIndex.ContainsKey(inst.guid)) inst.guid = ItemGuid.NewGuid(); //중복되지 않을 때까지 반복

        slots[emptyIndex].itemId = itemId;  
        slots[emptyIndex].amount = 0;
        slots[emptyIndex].instance = inst;

        guidToIndex[inst.guid] = emptyIndex;

        CurrentWeight += addWeight;
        CurrentVolume += addVolume;

        NotifyChanged();
        return true;
    }
    public bool TryRemoveStack(int index, int amount) //스택형 아이템 제거
    {
        if(amount <= 0) return false;
        if(index < 0 || index >= Capacity) return false;

        InventorySlot slot = slots[index];
        if(!slot.IsStack) return false; //스택형 아이템이 아님
        if(slot.amount < amount) return false; //제거하려는 양이 현재 양보다 많음

        slot.amount -= amount;

        CurrentWeight = Mathf.Max(0f, CurrentWeight - ItemDB.GetWeight(slot.itemId) * amount);
        CurrentVolume = Mathf.Max(0f, CurrentVolume - ItemDB.GetVolume(slot.itemId) * amount);

        if(slot.amount <= 0)
        {
            int id = slot.itemId;
            slot.clear();
            stackToIndex.Remove(id);
        }

        NotifyChanged();
        return true;
    }
    public bool TryRemoveInstance(string guid, out ItemStack removedItem) // 인스턴스 아이템 제거
    {
        removedItem = null;
        if(string.IsNullOrEmpty(guid)) return false;
        if(!guidToIndex.TryGetValue(guid, out var index)) return false; // guid에 해당하는 아이템이 없음

        InventorySlot slot = slots[index];
        if(!slot.IsInstance) return false; //인스턴스형 아이템이 아님
        
        removedItem = slot.instance;

        CurrentWeight = Mathf.Max(0f, CurrentWeight - ItemDB.GetWeight(slot.itemId));
        CurrentVolume = Mathf.Max(0f, CurrentVolume - ItemDB.GetVolume(slot.itemId));

        slot.clear();
        guidToIndex.Remove(guid);

        NotifyChanged();
        return true;
    }
    public bool Swap(int a, int b) //슬롯 간 아이템 교환
    {
        if(a < 0 || a >= Capacity || b < 0 || b >= Capacity) return false;
        if(a == b) return true;

        (slots[a], slots[b]) = (slots[b], slots[a]); //슬롯 교환

        UpdateIndexForSlot(a);
        UpdateIndexForSlot(b);

        NotifyChanged();
        return true;
    }
    public bool Move(int from, int to) //슬롯 간 아이템 이동 (교환과 달리 빈 슬롯으로 이동할 때만 허용)
    {
        if(from < 0 || from >= Capacity || to < 0 || to >= Capacity) return false;
        if(from == to) return true;

        InventorySlot fromSlot = slots[from];
        InventorySlot toSlot = slots[to];
        if(fromSlot.isEmpty) return false; //이동하려는 슬롯이 빈 슬롯

        if (toSlot.isEmpty)
        {
            CopySlot(slots[from], slots[to]);
            slots[from].clear();

            UpdateIndexForSlot(from);
            UpdateIndexForSlot(to);
            return true;
        }

        return Swap(from, to);
    }
    private void CopySlot(InventorySlot from, InventorySlot to)
    {
        to.itemId = from.itemId;
        to.amount = from.amount;
        to.instance = from.instance;
    }
    private void UpdateIndexForSlot(int index)
    {
        InventorySlot slot = slots[index];
        if(slot.IsInstance) guidToIndex[slot.instance.guid] = index;
        else if(slot.IsStack) stackToIndex[slot.itemId] = index;
    }

    public void RebuildIndicesAndCapacity()
    {
        guidToIndex.Clear();
        stackToIndex.Clear();

        float weight = 0f, volume = 0f;

        for (int i = 0; i < Capacity; i++)
        {
            InventorySlot slot = slots[i];
            if (slot.isEmpty) continue;

            if (slot.IsInstance)
            {
                guidToIndex[slot.instance.guid] = i;
                weight += ItemDB.GetWeight(slot.itemId);
                volume += ItemDB.GetVolume(slot.itemId);
            }
            else if (slot.IsStack)
            {
                stackToIndex[slot.itemId] = i;
                weight += ItemDB.GetWeight(slot.itemId) * slot.amount;
                volume += ItemDB.GetVolume(slot.itemId) * slot.amount;
            }
        }
        CurrentVolume = Mathf.Max(0f, volume);
        CurrentWeight = Mathf.Max(0f, weight);
    }
    private void NotifyChanged()
    {
        EventManager.TriggerEvent(EventEnum.EventKey.InventoryChanged, playerType);
    }
}