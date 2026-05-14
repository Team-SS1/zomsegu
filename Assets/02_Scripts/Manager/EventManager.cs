using System;
using System.Collections.Generic;
using UnityEngine;
using EventEnum;

public class EventManager : GlobalSingleton<EventManager>
{
    private Dictionary<EventKey, Delegate> eventDictionary;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    public void Init()
    {
        eventDictionary ??= new Dictionary<EventKey, Delegate>();
    }

    public static void Subscribe(EventKey key, Action listener)
    {
        if (Instance == null || listener == null) return;

        if (Instance.eventDictionary.TryGetValue(key, out var del))
            Instance.eventDictionary[key] = Delegate.Combine(del, listener);
        else
            Instance.eventDictionary.Add(key, listener);
    }

    public static void Subscribe<T>(EventKey key, Action<T> listener)
    {
        if (Instance == null || listener == null) return;

        if (Instance.eventDictionary.TryGetValue(key, out var del))
            Instance.eventDictionary[key] = Delegate.Combine(del, listener);
        else
            Instance.eventDictionary.Add(key, listener);
    }

    public static void UnSubscribe(EventKey key, Action listener)
    {
        if (Instance == null || listener == null) return;

        if (Instance.eventDictionary.TryGetValue(key, out var del))
        {
            var current = Delegate.Remove(del, listener);

            if (current == null)
                Instance.eventDictionary.Remove(key);
            else
                Instance.eventDictionary[key] = current;
        }
    }

    public static void UnSubscribe<T>(EventKey key, Action<T> listener)
    {
        if (Instance == null || listener == null) return;

        if (Instance.eventDictionary.TryGetValue(key, out var del))
        {
            var current = Delegate.Remove(del, listener);

            if (current == null)
                Instance.eventDictionary.Remove(key);
            else
                Instance.eventDictionary[key] = current;
        }
    }

    public static void TriggerEvent(EventKey key)
    {
        if (Instance == null) return;

        if (Instance.eventDictionary.TryGetValue(key, out var del))
        {
            if (del is Action callback)
                callback.Invoke();
        }
    }

    public static void TriggerEvent<T>(EventKey key, T arg)
    {
        if (Instance == null)
        {
            Debug.LogWarning($"[EventManager] Instance is null. key = {key}");
            return;
        }

        if (Instance.eventDictionary == null)
        {
            Debug.LogError($"[EventManager] eventDictionary is null. key = {key}");
            return;
        }

        if (Instance.eventDictionary.TryGetValue(key, out var del))
        {
            Debug.Log($"[EventManager] TriggerEvent<{typeof(T).Name}> : {key}");

            if (del is Action<T> callback)
            {
                try
                {
                    callback.Invoke(arg);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventManager] Callback exception on key = {key}\n{e}");
                }
            }
            else
            {
                Debug.LogWarning($"[EventManager] Delegate type mismatch. key = {key}, actual = {del?.GetType().Name}");
            }
        }
        else
        {
            Debug.LogWarning($"[EventManager] No listener for key = {key}");
        }
    }

    // 사용 방법 매개변수 미존재
    //EventManager.Subscribe(EventKey.OnGameStart, OnGameStart);
    //EventManager.TriggerEvent(EventKey.OnGameStart);

    //void OnGameStart()
    //{
    //    Debug.Log("Start");
    //}

    //사용 방법 매개변수 존재
    //EventManager.Subscribe<int>(EventKey.OnDamage, OnDamage);
    //EventManager.TriggerEvent<int>(EventKey.OnDamage, 10);

    //void OnDamage(int damage)
    //{
    //    Debug.Log(damage);
    //}
}