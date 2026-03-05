using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DragPayload
{
    public SlotRef from;
    public SlotRef to;
    public bool hasTo; // 드래그한 아이템이 도착지까지 이동했는지 여부

    public DragPayload(SlotRef from)
    {
        this.from = from;
        hasTo = false;
    }

    public void SetTo(SlotRef to)
    {
        this.to = to;
        hasTo = true;
    }
}
public static class DragContext // 다른 스크립트에서의 호출용
{
    public static DragPayload payload;
}
