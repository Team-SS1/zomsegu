using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject 데이터베이스
/// </summary>
[CreateAssetMenu(fileName = "SoDatabase", menuName = "SO/Database/SO Database")]
public class SoDatabase : ScriptableObject
{
    [SerializeField] private List<ScriptableObject> list = new();
    protected List<ScriptableObject> List => list;
    public int Count => list.Count;

    public List<T> GetDatabase<T>() where T : ScriptableObject
    {
        List<T> newList = new();

        foreach (ScriptableObject so in list)
        {
            T t = so as T;
            if (t != null)
            {
                newList.Add(t);
            }
        }

        return newList;
    }

    #region 에디터 전용
#if UNITY_EDITOR
    protected virtual void Reset()
    {
    }
#endif
    #endregion
}
