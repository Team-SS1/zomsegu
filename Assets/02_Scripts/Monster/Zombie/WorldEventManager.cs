using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// World Event Central Manager
/// </summary>
public class WorldEventManager : MonoBehaviour
{
    public static WorldEventManager Instance;

    public event Action<SoundEvent> OnSoundEvent; // Monster Subscribe

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Invoke SoundEvent
    /// </summary>
    public void RaiseSoundEvent(SoundEvent soundEvent)
    {
        OnSoundEvent?.Invoke(soundEvent);
    }
}
