using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject 데이터베이스
/// </summary>
[CreateAssetMenu(fileName = "SoDatabase", menuName = "SO/Database/SO Database")]
public class SoDatabase : ScriptableObject
{
    [SerializeField] private List<ScriptableObject> _list = new();
    protected List<ScriptableObject> List => _list;
    public int Count => _list.Count;

    public List<T> GetDatabase<T>() where T : ScriptableObject
    {
        List<T> newList = new();

        foreach (ScriptableObject so in _list)
        {
            T t = so as T;
            if (t != null)
            {
                newList.Add(t);
            }
        }

        return newList;
    }
}
