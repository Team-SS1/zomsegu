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

    private readonly List<AudioPoolObject> pool = new();

    private readonly int maxSize;

    public AudioSourcePool(AudioSource origin, Transform root, int size, int maxSize)
    {
        this.origin = origin;
        this.root = root;
        this.maxSize = maxSize;

        for (int i = 0; i < size; i++)
        {
            CreatePoolObject();
        }
    }

    private void CreatePoolObject()
    {
        AudioSource newAudioSource = Object.Instantiate(origin, root);
        AudioPoolObject newPo = newAudioSource.GetComponent<AudioPoolObject>();
        newPo.Create(new AudioInstance(newAudioSource));
        newPo.gameObject.SetActive(false);
        pool.Add(newPo);
    }

    public AudioPoolObject Get()
    {
        foreach (AudioPoolObject po in pool)
        {
            if (!po.Instance.IsPlaying) { return po; }
        }

        AudioPoolObject newPo;

        if (pool.Count < maxSize)
        {
            CreatePoolObject();
            newPo = pool[^1];
        }
        else
        {
            newPo = LowestPriorityInstance();
            newPo.gameObject.SetActive(false);
        }

        return newPo;
    }

    public void Release(AudioPoolObject po)
    {
        po.gameObject.SetActive(false);
    }

    public void ReleaseAll()
    {
        foreach (var po in pool)
        {
            Release(po);
        }
    }

    private AudioPoolObject LowestPriorityInstance()
    {
        AudioPoolObject lowest = pool[0];

        foreach (AudioPoolObject po in pool)
        {
            if (lowest.Priority > po.Priority)
            {
                lowest = po;
            }
        }

        return lowest;
    }

    public void Pause()
    {
        foreach (AudioPoolObject po in pool)
        {
            po.Instance.Pause();
        }
    }

    public void UnPause()
    {
        foreach (AudioPoolObject po in pool)
        {
            po.Instance.UnPause();
        }
    }
}
