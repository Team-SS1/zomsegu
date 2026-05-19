using ItemEnum;
using PlayerEnum;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISearchWindow : UIPopup
{
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI titleTXT;
    [SerializeField] private Button closeButton;

    [Header("List")]
    [SerializeField] private Transform content;
    [SerializeField] private UISearchItemEntry entryPrefab;

    [Header("Player")]
    [SerializeField] private PlayerType targetPlayerType = PlayerType.Player_SHIN;

    private readonly List<LootSource> currentSources = new();
    private readonly List<SearchDisplayEntry> currentEntries = new();
    private readonly List<UISearchItemEntry> spawnedEntries = new();

    private int selectedIndex = -1;
    private string currentTitle = "바닥";

    private bool IsOpen => gameObject.activeSelf;
    public int SelectedIndex => selectedIndex;
    public SearchDisplayEntry SelectedEntry
    {
        get
        {
            if(selectedIndex < 0 || selectedIndex >= currentEntries.Count)
                return null;

            return currentEntries[selectedIndex];
        }
    }

    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseWindow);
    }
    protected override void DestroyInternal()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseWindow);

        base.DestroyInternal();
    }
    public void OpenWithSource(LootSource source)
    {
        currentSources.Clear();

        if(source != null)
            currentSources.Add(source);

        currentTitle = source != null ? source.displayName : "바닥"; 

        RefreshWindow(true);
    }
    public void OpenWithSources(List<LootSource> sources, string title = "바닥")
    {
        currentSources.Clear();

        if (sources != null)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                if(sources[i] != null)
                    currentSources.Add(sources[i]);
            }
        }

        currentTitle = string.IsNullOrEmpty(title) ? "바닥" : title;

        RefreshWindow(true);
    }
    public void RefreshFromCurrentSources()
    {
        if(!IsOpen) return;
        RefreshWindow(false);
    }
    public void CloseWindow()
    {
        HideTooltip();
        ClearSpawnedEntries();
        currentEntries.Clear();
        currentSources.Clear();
        selectedIndex = -1;
        currentTitle = "바닥";

        Close();
    }
    public void OnClickEntry(UISearchItemEntry entryUI) //한번 클릭시
    {
        int index = spawnedEntries.IndexOf(entryUI);
        if(index < 0) return;

        SelectIndex(index);
        ShowTooltip(entryUI);
    }
    public void OnDoubleClickEntry(UISearchItemEntry entryUI) //더블 클릭 시
    {
        int index = spawnedEntries.IndexOf(entryUI);
        if(index < 0) return;

        SelectIndex(index);
        TryPickupSelected();
    }
    public void OnClickPickupButton(UISearchItemEntry entryUI) // 가방 버튼 누를 시
    {
        int index = spawnedEntries.IndexOf(entryUI);
        if(index < 0) return;

        SelectIndex(index);
        TryPickupSelected();
    }
    public void OnHoverEntry(UISearchItemEntry entryUI) // 마우스 커서 올릴 시
    {
        ShowTooltip(entryUI);
    }
    public void OnExitEntry(UISearchItemEntry entryUI) // 마우스 커서 나갈 시
    {
        SearchDisplayEntry selected = SelectedEntry;
        if (selected == null)
        {
            HideTooltip();
            return;
        }

        int selectedUIIndex = selectedIndex;
        if (selectedUIIndex < 0 || selectedUIIndex >= spawnedEntries.Count)
        {
            HideTooltip();
            return;
        }
        ShowTooltip(spawnedEntries[selectedUIIndex]);
    }
    public bool TryPickupSelected()
    {
        SearchDisplayEntry entry = SelectedEntry;
        if (entry == null)
            return false;

        bool success = SearchLootService.TryPickupEntry(targetPlayerType, entry);

        if (!success)
        {
#if UNITY_EDITOR
            Debug.Log("탐색창 아이템 획득 실패");
#endif
            return false;
        }

        RefreshWindow(false);
        return true;
    }
    public void RefreshWindow(bool resetSelection)
    {
        BuildEntries();

        if (titleTXT != null)
            titleTXT.text = currentTitle;

        if (resetSelection)
        {
            selectedIndex = currentEntries.Count > 0 ? 0 : -1;
        }
        else
        {
            if (currentEntries.Count == 0)
                selectedIndex = -1;
            else
                selectedIndex = Mathf.Clamp(selectedIndex, 0, currentEntries.Count - 1);
        }

        ReBuildList();
        ShowTooltipBySelectedIndex();
    }
    private void BuildEntries()
    {
        currentEntries.Clear();

        if (currentSources.Count == 0)
            return;

        if(currentSources.Count == 1)
        {
            List<SearchDisplayEntry> entries = SearchEntryBuilder.Build(currentSources[0]);
            currentEntries.AddRange(entries);
            return;
        }
        List<SearchDisplayEntry> mergedEntries = SearchEntryBuilder.Build(currentSources);
        currentEntries.AddRange(mergedEntries);
    }
    private void ReBuildList()
    {
        ClearSpawnedEntries();

        for(int i = 0; i < currentEntries.Count; i++)
        {
            UISearchItemEntry entryUI = Instantiate(entryPrefab, content);
            entryUI.SetEntry(this, currentEntries[i], i == selectedIndex);
            spawnedEntries.Add(entryUI);
        }

        RefreshSelection();
    }
    private void ClearSpawnedEntries()
    {
        for(int i = 0; i < spawnedEntries.Count; i++)
        {
            if(spawnedEntries[i] != null)
                Destroy(spawnedEntries[i].gameObject);
        }

        spawnedEntries.Clear();
    }

    private void SelectIndex(int index)
    {
        if (index < 0 || index >= currentEntries.Count)
            return;

        selectedIndex = index;
        RefreshSelection();
    }
    private void RefreshSelection()
    {
        for(int i = 0; i < spawnedEntries.Count; i++)
        {
            bool isSelected = i == selectedIndex;
            spawnedEntries[i].SetSelected(isSelected);
        }
    }
    private void ShowTooltipBySelectedIndex()
    {
        if(selectedIndex < 0 || selectedIndex >= currentEntries.Count)
        {
            HideTooltip();
            return;
        }

        ShowTooltip(spawnedEntries[selectedIndex]);
    }
    private void ShowTooltip(UISearchItemEntry entryUI)
    {
        if (entryUI == null || entryUI.EntryData == null)
            return;

        SearchDisplayEntry entry = entryUI.EntryData;
        ItemStack instance = entry.IsInstanceEntry ? entry.representativeInstance : null;

        int compareItemId = EquipmentQueryService.GetEquippedItemId(targetPlayerType, EquipSlotType.Weapon);
        ItemStack compareInstance = EquipmentQueryService.GetEquippedInstance(targetPlayerType, EquipSlotType.Weapon);

        UITooltipManage tooltipManage = UIManager.Instance.GetUI<UITooltipManage>();
        tooltipManage.ShowInventoryTooltip(entryUI.Rect, entry.itemId, instance, compareItemId, compareInstance);
    }
    private void HideTooltip()
    {
        UIManager.Instance.GetUI<UITooltipManage>()?.HideAll();
    }
}
