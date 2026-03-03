using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오디오 소스 풀링 담당
/// todo: generic pool 만들고 교체 or 상속
/// </summary>
public class AudioSourcePool
{
    private readonly AudioSource origin;
    private readonly Transform root;
    private readonly List<AudioSource> pool = new();
    private int index = 0;

    public AudioSourcePool(GameObject origin, Transform root, int poolSize)
    {
        this.origin = origin.GetComponent<AudioSource>();
        this.root = root;

        for (int i = 0; i < poolSize; i++)
        {
            var newAudioSource = GameObject.Instantiate(this.origin, root);
            newAudioSource.gameObject.SetActive(false);
            pool.Add(newAudioSource);
        }
    }

    public AudioSource Get()
    {
        // index가 풀 사이즈보다 커지면 다시 처음부터 순회
        if (index >= pool.Count)
        {
            index %= pool.Count;
        }

        // 현재 인덱스의 오디오 소스가 활성화되어 있다면
        // 1. 새로 생성 후 풀에 추가
        // 2. 해당 index로 이동
        if (pool[index].enabled)
        {
            var newAudioSource = GameObject.Instantiate(origin, root);
            pool.Add(newAudioSource);
            index = pool.Count - 1;
        }

        return pool[index++];
    }

    public void Release(AudioSource audioSource)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
    }

    public void ReleaseAll()
    {
        foreach (var audioSource in pool)
        {
            Release(audioSource);
        }
    }
}
