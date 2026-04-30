using ItemEnum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// F1 : 스택형 묶어서 보이는 바닥 탐색창
// F2 : 스택형도 낱개로 보이는 바닥 탐색창
// F3 : 여러 source를 한 번에 여는 Tab 탐색창 테스트
// F4 : 컨테이너창 테스트
// ESC : 닫기
// F : 현재 선택 아이템 획득
public class SearchWindowTest : MonoBehaviour //임시 테스트용임 input도 그냥 막 집어넣음
{
    [Header("Reference")]
    [SerializeField] private UISearchWindow searchWindow;

    [Header("Test Item Id")]
    [SerializeField] private int stackItemId;
    [SerializeField] private int secondStackItemId;
    [SerializeField] private int instanceItemId;

    [Header("Count")]
    [SerializeField] private int stackCount = 3;
    [SerializeField] private int secondStackCount = 2;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || searchWindow == null)
            return;
        else if (keyboard.f1Key.wasPressedThisFrame)
            OpenMergedGroundScan();
        else if (keyboard.f2Key.wasPressedThisFrame)
            OpenSeparatedGroundScan();
        else if (keyboard.f3Key.wasPressedThisFrame)
            OpenMultiSourceGroundScan();
        else if (keyboard.f4Key.wasPressedThisFrame)
            OpenContainerWindow();
        else if (keyboard.escapeKey.wasPressedThisFrame && searchWindow.gameObject.activeSelf)
            searchWindow.CloseWindow();
        else if (keyboard.fKey.wasPressedThisFrame && searchWindow.gameObject.activeSelf)
            searchWindow.TryPickupSelected();
    }

    private void OpenMergedGroundScan() // 스택형 합쳐서 보이는 탐색창
    {
        LootSource source = new LootSource("바닥", LootSourceType.GroundScan);
        source.mergeStackableForDisplay = true;

        AddStackItems(source, stackItemId, stackCount);
        AddStackItems(source, secondStackItemId, secondStackCount);
        AddInstanceItem(source, instanceItemId);

        searchWindow.OpenWithSource(source);
    }

    private void OpenSeparatedGroundScan() // 스택형도 낱개로 보이는 탐색창
    {
        LootSource source = new LootSource("바닥", LootSourceType.GroundScan);
        source.mergeStackableForDisplay = false;

        AddStackItems(source, stackItemId, stackCount);
        AddStackItems(source, secondStackItemId, secondStackCount);
        AddInstanceItem(source, instanceItemId);

        searchWindow.OpenWithSource(source);
    }

    private void OpenMultiSourceGroundScan() // 여러 source를 한 번에 여는 Tab 탐색창 테스트
    {
        LootSource sourceA = new LootSource("바닥", LootSourceType.GroundScan);
        sourceA.mergeStackableForDisplay = true;
        AddStackItems(sourceA, stackItemId, 2);

        LootSource sourceB = new LootSource("바닥", LootSourceType.GroundScan);
        sourceB.mergeStackableForDisplay = true;
        AddStackItems(sourceB, secondStackItemId, 3);
        AddInstanceItem(sourceB, instanceItemId);

        List<LootSource> sources = new List<LootSource> { sourceA, sourceB };
        searchWindow.OpenWithSources(sources, "바닥");
    }

    private void OpenContainerWindow()
    {
        LootSource source = new LootSource("선반", LootSourceType.Container);
        source.mergeStackableForDisplay = true;
        source.requiredInvestigation = false;

        AddStackItems(source, stackItemId, stackCount);
        AddInstanceItem(source, instanceItemId);

        searchWindow.OpenWithSource(source);
    }

    private void AddStackItems(LootSource source, int itemId, int count)
    {
        if (source == null || itemId == 0 || count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            source.AddItem(new LootItem(itemId, 1));
        }
    }

    private void AddInstanceItem(LootSource source, int itemId)
    {
        if (source == null || itemId == 0)
            return;

        if (!ItemDB.UseInstance(itemId))
        {
            source.AddItem(new LootItem(itemId, 1));
            return;
        }

        int maxDurability = 0;
        int currentDurability = 0;

        if (ItemDB.HasDurability(itemId))
        {
            maxDurability = ItemDB.GetDefaultDurability(itemId);
            currentDurability = maxDurability;
        }

        ItemStack instance = new ItemStack(itemId, currentDurability, maxDurability);
        source.AddItem(new LootItem(instance));
    }
}
