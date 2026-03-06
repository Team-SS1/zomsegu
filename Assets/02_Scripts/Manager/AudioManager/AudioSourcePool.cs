using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오디오 소스 풀링 담당
/// todo: generic pool 만들고 교체 or 상속
/// </summary>
public class AudioSourcePool : IAudioSourcePool
{
    private readonly AudioSource origin;
    private readonly Transform root;

    private readonly List<IAudioInstance> pool = new();

    private readonly int maxSize;

    public AudioSourcePool(AudioSource origin, Transform root, int size, int maxSize)
    {
        this.origin = origin;
        this.root = root;
        this.maxSize = maxSize;

        for (int i = 0; i < size; i++)
        {
            AudioSource newAudioSource = Object.Instantiate(this.origin, root);
            newAudioSource.gameObject.SetActive(false);
            pool.Add(new AudioInstance(newAudioSource));
        }
    }

    public IAudioInstance Get()
    {
        foreach (IAudioInstance instance in pool)
        {
            if (!instance.IsPlaying) return instance;
        }

        IAudioInstance newInstance;

        if (pool.Count < maxSize)
        {
            var newGo = Object.Instantiate(origin, root);
            newInstance = new AudioInstance(newGo);
            pool.Add(newInstance);
        }
        else
        {
            newInstance = LowestPriorityInstance();
            newInstance.Stop();
        }

        return newInstance;
    }

    public void Release(IAudioInstance instance)
    {
        instance.Stop();
        instance.SetClip(null);
    }

    public void ReleaseAll()
    {
        foreach (var instance in pool)
        {
            Release(instance);
        }
    }

    private IAudioInstance LowestPriorityInstance()
    {
        AudioInstance lowest = pool[0] as AudioInstance;

        foreach (IAudioInstance instance in pool)
        {
            if (lowest.Priority > ((AudioInstance)instance).Priority)
            {
                lowest = instance as AudioInstance;
            }
        }

        return lowest;
    }
}
