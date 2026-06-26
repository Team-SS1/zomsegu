using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SFX용 AudioSource pool
/// </summary>
public class AudioSourcePool
{
    private readonly AudioSource origin;
    private readonly Transform root;

    private readonly List<AudioPoolObject> activePool = new();
    private readonly List<AudioPoolObject> deactivePool = new();

    private readonly int maxSize;

    public AudioSourcePool(AudioSource origin, Transform root, int size, int maxSize)
    {
        this.origin = origin;
        this.root = root;
        this.maxSize = Mathf.Max(1, maxSize);

        int initialSize = Mathf.Clamp(size, 1, this.maxSize);
        for (int i = 0; i < initialSize; i++)
        {
            CreatePoolObject();
        }
    }

    public AudioPoolObject Get()
    {
        if (activePool.Count >= maxSize)
        {
            Release(LowestPriorityInstance());
        }

        if (deactivePool.Count == 0)
        {
            CreatePoolObject();
        }

        AudioPoolObject po = deactivePool[0];
        deactivePool.RemoveAt(0);
        activePool.Add(po);
        po.gameObject.SetActive(true);
        return po;
    }

    public void Release(AudioPoolObject po)
    {
        if (po == null || !po.gameObject.activeSelf) return;
        po.gameObject.SetActive(false);
    }

    public void ReleaseAll()
    {
        for (int i = activePool.Count - 1; i >= 0; i--)
        {
            Release(activePool[i]);
        }
    }

    public void Pause()
    {
        for (int i = 0; i < activePool.Count; i++)
        {
            activePool[i].Pause();
        }
    }

    public void UnPause()
    {
        for (int i = 0; i < activePool.Count; i++)
        {
            activePool[i].Resume();
        }
    }

    public void SetSpatialDistance(float minDistance, float maxDistance)
    {
        for (int i = 0; i < activePool.Count; i++)
        {
            AudioSource source = activePool[i].Source;
            if (source == null || source.spatialBlend <= 0f) continue;

            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
        }
    }

    private void CreatePoolObject()
    {
        AudioSource newAudioSource = Object.Instantiate(origin, root);
        newAudioSource.gameObject.name = $"{activePool.Count + deactivePool.Count}";

        AudioPoolObject po = newAudioSource.GetComponent<AudioPoolObject>();
        po.Create(newAudioSource);
        po.OnEnd += OnEnd;
        po.gameObject.SetActive(false);
        deactivePool.Add(po);
    }

    private void OnEnd(AudioPoolObject po)
    {
        if (!activePool.Remove(po)) return;
        if (!deactivePool.Contains(po))
        {
            deactivePool.Add(po);
        }
    }

    private AudioPoolObject LowestPriorityInstance()
    {
        AudioPoolObject lowest = activePool[0];

        for (int i = 1; i < activePool.Count; i++)
        {
            if (lowest.Priority > activePool[i].Priority)
            {
                lowest = activePool[i];
            }
        }

        return lowest;
    }
}
