using System.Collections.Generic;

public static class SearchEntryBuilder // 탐색창 UI에 보일 아이템칸 리스트 생성기
{
    public static List<SearchDisplayEntry> Build(LootSource source)
    {
        List<SearchDisplayEntry> result = new List<SearchDisplayEntry>();

        if(source == null || source.lootItems == null)
            return result;

        Dictionary<int, SearchDisplayEntry> stackMap = new Dictionary<int, SearchDisplayEntry>(); // itemId가 같은 아이템 합치는 용

        for(int i = 0; i < source.lootItems.Count; i++)
        {
            LootItem item = source.lootItems[i];
            if(item == null || item.itemId == 0)
                continue;

            bool isStackable = ItemDB.IsStackable(item.itemId);
            bool shouldMerge = source.mergeStackableForDisplay && isStackable;

            if (shouldMerge)
            {
                if(!stackMap.TryGetValue(item.itemId, out SearchDisplayEntry entry))
                {
                    entry = new SearchDisplayEntry();
                    entry.source = source;
                    entry.itemId = item.itemId;
                    stackMap[item.itemId] = entry;
                    result.Add(entry);
                }

                entry.AddUnit(item);
            }
            else
            {
                SearchDisplayEntry entry = new SearchDisplayEntry();
                entry.source = source;
                entry.itemId = item.itemId;
                entry.representativeInstance = item.instance;
                entry.AddUnit(item);
                result.Add(entry);
            }
        }
        SearchEntrySorter.Sort(result);
        return result;
    }
    public static List<SearchDisplayEntry> Build(List<LootSource> sources)
    {
        List<SearchDisplayEntry> result = new List<SearchDisplayEntry>();

        if (sources == null)
            return result;

        for (int i = 0; i < sources.Count; i++)
        {
            LootSource source = sources[i];
            List<SearchDisplayEntry> entries = Build(source);

            for(int j = 0; j < entries.Count; j++)
            {
                result.Add(entries[j]);
            }
        }

        SearchEntrySorter.Sort(result);
        return result;
    }
}
