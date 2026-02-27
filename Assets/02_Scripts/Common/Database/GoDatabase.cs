using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObject 데이터베이스
/// </summary>
[CreateAssetMenu(fileName = "GoDatabase", menuName = "SO/Database/GO Database")]
public class GoDatabase : ScriptableObject
{
    [SerializeField] private List<GameObject> _list = new();
    protected List<GameObject> List => _list;
    public int Count => _list.Count;

    public List<GameObject> GetDatabase()
    {
        List<GameObject> newList = new();

        foreach (GameObject go in _list)
        {
            if (go != null)
            {
                newList.Add(go);
            }
        }

        return newList;
    }

    public List<T> GetDatabaseComponent<T>()
    {
        List<T> newList = new();

        foreach (GameObject go in _list)
        {
            if (go != null)
            {
                if (go.GetComponent<T>() != null)
                {
                    newList.Add(go.GetComponent<T>());
                }
            }
        }
        return newList;
    }
}
