using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// JSON 파일을 메모리에 올리고
/// JSON을 C# 객체(List<T>)로 변환해서
/// 객체들을 Dictionary에 저장하는 작업
/// </summary>
public static class DataTableLoader
{
    private static readonly List<Action<Action<string>>> _loadActions = new()
    {
        log => LoadTable<PlayerStat, int>("JsonFiles/PlayerStat", PlayerStat.tableDic, x => x.PlayerID, log),
        //log => LoadTable<MonsterStat, int>("JsonFiles/MonsterStat", MonsterStat.tableDic, x => x.MonsterID, log),
        log => LoadTable<CommonItemData, int>("JsonFiles/CommonItemData", CommonItemData.tableDic, x => x.ItemID, log),
        log => LoadTable<WeaponStat, int>("JsonFiles/WeaponStat", WeaponStat.tableDic, x => x.ItemID, log),
        log => LoadTable<ArmorStat, int>("JsonFiles/ArmorStat", ArmorStat.tableDic, x => x.ItemID, log),
        log => LoadTable<ConsumableStat, int>("JsonFiles/ConsumableStat", ConsumableStat.tableDic, x => x.ItemID, log),
        log => LoadTable<AccessoryStat, int>("JsonFiles/AccessoryStat", AccessoryStat.tableDic, x => x.ItemID, log),
    };

    /// <summary>
    /// 등록된 모든 테이블을 로드한다.
    /// </summary>
    public static void LoadTables(Action<string> log = null)
    {
        log ??= Debug.Log;

        for (int i = 0; i < _loadActions.Count; i++)
            _loadActions[i](log);
    }

    /// <summary>
    /// Resources 경로에서 json(TextAsset)을 로드하고, List<T>로 Deserialize 후,
    /// keySelector로 Dictionary에 채워 넣는다.
    /// </summary>
    public static bool LoadTable<T, TKey>(
    string resourcesPath,
    Dictionary<TKey, T> targetDic,
    Func<T, TKey> keySelector,
    Action<string> log = null
)
    {
        log ??= Debug.Log;

        TextAsset jsonFile = Resources.Load<TextAsset>(resourcesPath);
        if (jsonFile == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[DataTableLoader] {resourcesPath}.json not found!");
#endif
            return false;
        }

        List<T> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<T>>(jsonFile.text);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataTableLoader] Deserialize failed. Path={resourcesPath}, Type={typeof(T).Name}\n{e}");
            return false;
        }

        if (list == null)
        {
            Debug.LogError($"[DataTableLoader] Deserialize returned null. Path={resourcesPath}, Type={typeof(T).Name}");
            return false;
        }

        targetDic.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            var row = list[i];
            if (row == null) continue;

            TKey key = keySelector(row);

            if (EqualityComparer<TKey>.Default.Equals(key, default))
            {
#if UNITY_EDITOR
                Debug.LogError($"[DataTableLoader] Invalid key(default). Path={resourcesPath}, Type={typeof(T).Name}, Index={i}");
#endif
                return false;
            }

            if (targetDic.ContainsKey(key))
            {
#if UNITY_EDITOR
                Debug.LogError($"[DataTableLoader] Duplicate key. Path={resourcesPath}, Type={typeof(T).Name}, Key={key}");
#endif
                return false;
            }

            targetDic[key] = row;
        }

#if UNITY_EDITOR
        log($"[DataTableLoader] Loaded: {typeof(T).Name} ({targetDic.Count} rows) from {resourcesPath}");
#endif
        return true;
    }
}
