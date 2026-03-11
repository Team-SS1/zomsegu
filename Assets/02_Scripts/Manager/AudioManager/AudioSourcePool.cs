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

    private readonly List<AudioPoolObject> activePool = new();
    private readonly List<AudioPoolObject> deactivePool = new();

    private readonly int initSize;
    private readonly int maxSize;

    public AudioSourcePool(AudioSource origin, Transform root, int size, int maxSize)
    {
        this.origin = origin;
        this.root = root;
        initSize = size;
        this.maxSize = maxSize;

        for (int i = 0; i < size; i++)
        {
            CreatePoolObject();
        }
    }

    private void CreatePoolObject()
    {
        AudioSource newAudioSource = Object.Instantiate(origin, root);
        newAudioSource.gameObject.name = $"{activePool.Count + deactivePool.Count}";
        AudioPoolObject newPo = newAudioSource.GetComponent<AudioPoolObject>();
        newPo.Create(new AudioInstance(newAudioSource));
        newPo.gameObject.SetActive(false);
        newPo.OnEnd += OnEnd;
        deactivePool.Add(newPo);
    }

    private void OnEnd(AudioPoolObject poolObject)
    {
        activePool.Remove(poolObject);
        deactivePool.Add(poolObject);
    }

    public AudioPoolObject Get()
    {
        AudioPoolObject newPo;

        // active에 여유가 있을 때
        if (activePool.Count < maxSize)
        {
            if (deactivePool.Count == 0)
            {
                CreatePoolObject();
            }
            newPo = deactivePool[0];
            activePool.Add(newPo);
            deactivePool.Remove(newPo);
            return newPo;
        }

        newPo = LowestPriorityInstance();

        return newPo;
    }

    public void Release(AudioPoolObject po)
    {
        po.gameObject.SetActive(false);
    }

    public void ReleaseAll()
    {
        foreach (var po in activePool)
        {
            Release(po);
        }
    }

    private AudioPoolObject LowestPriorityInstance()
    {
        AudioPoolObject lowest = activePool[0];

        foreach (AudioPoolObject po in activePool)
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
        foreach (AudioPoolObject po in activePool)
        {
            po.Instance.Pause();
        }
    }

    public void UnPause()
    {
        foreach (AudioPoolObject po in activePool)
        {
            po.Instance.UnPause();
        }
    }
}
