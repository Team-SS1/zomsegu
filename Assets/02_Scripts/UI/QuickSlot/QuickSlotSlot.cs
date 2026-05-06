using System;

[Serializable]
public class QuickSlotSlot
{
    public int itemId;
    public string guid; //인슽턴스형 아이템일 경우

    public bool isEmpty => itemId == 0;

    public bool IsInstance => !string.IsNullOrEmpty(guid);
    public bool IsStack => itemId != 0 && string.IsNullOrEmpty(guid); //스택형은 guid 없음

    public void SetStack(int itemId)
    {
        this.itemId = itemId;
        this.guid = null;
    }
    public void SetInstance(ItemStack item)
    {
        if(item == null)
        {
            Clear();
            return;
        }

        this.itemId = item.itemId;
        this.guid = item.guid;
    }
    public void Clear()
    {
        itemId = 0;
        guid = null;
    }
}
