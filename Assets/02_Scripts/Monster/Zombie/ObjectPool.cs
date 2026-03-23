using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component, IPoolable
{
    private Queue<T> pool = new();
    private T prefab;
    private Transform root;

    public ObjectPool(T prefab, int initialSize, Transform root)
    {
        this.prefab = prefab;
        this.root = root;

        for (int i = 0; i < initialSize; i++)
        {
            var obj = CreateNew();
            pool.Enqueue(obj);
        }
    }

    private T CreateNew()
    {
        var obj = GameObject.Instantiate(prefab, root);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get()
    {
        var obj = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        obj.gameObject.SetActive(true);
        obj.OnSpawn();
        return obj;
    }

    public void Return(T obj)
    {
        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}