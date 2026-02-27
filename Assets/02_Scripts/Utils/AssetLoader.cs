using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// 유니티 에디터 전용 에셋 로더
/// </summary>
public static class AssetLoader
{
    /// <summary>
    /// GO 찾기
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject FindAndLoadByName(string name)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Prefab {name}");

        if (guids.Length == 0)
        {
            Logger.Log($"{name} 프리팹 못 찾음");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    /// <summary>
    /// T 타입의 특정 오브젝트 찾기
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T FindAndLoadByName<T>(string name) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} {name}");

        if (guids.Length == 0)
        {
            Logger.Log($"{name} 오브젝트 못 찾음");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public static List<T> FindAndLoadAllByType<T>() where T : ScriptableObject
    {
        List<T> list = new();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");

        if (guids.Length == 0)
        {
            Logger.Log($"{nameof(T)} SO 못 찾음");
            return null;
        }

        string path;
        foreach (string guid in guids)
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
            list.Add(AssetDatabase.LoadAssetAtPath<T>(path));
        }

        return list;
    }
}
#endif