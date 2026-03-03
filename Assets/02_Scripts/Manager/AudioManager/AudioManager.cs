using AudioEnum;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : GlobalSingleton<AudioManager>
{
    #region 필드
    [Header("데이터베이스")]
    [SerializeField] private AudioDatabase audioDatabase;

    private readonly Dictionary<AudioName, List<AudioEntry>> bgmDict = new();
    private readonly Dictionary<AudioName, List<AudioEntry>> sfxDict = new();

    [Header("거리 기반 세팅")]
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    [Header("풀 세팅")]
    [SerializeField] private int sfxPoolSize = 10;

    private AudioSource bgmAudioSource; // BGM
    private AudioSourcePool pool2D;     // SFX - UI
    private AudioSourcePool pool3D;     // SFX - 거리 기반

    private List<ActiveAudio> active3D = new();

    private class ActiveAudio
    {
        public AudioSource audioSource;
        public Transform follow;
    }

    #endregion

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        InitializeDict();
        SetAudioSources();
    }

    private void Update()
    {
        for (int i = active3D.Count - 1; i >= 0; i--)
        {
            ActiveAudio active = active3D[i];

            if (!active.audioSource.isPlaying)
            {
                pool3D.Release(active.audioSource);
                active3D.RemoveAt(i);
                continue;
            }

            if (active.follow != null)
            {
                active.audioSource.transform.position = active.follow.position;
            }
        }
    }
    #endregion

    #region 초기화
    /// <summary>
    /// DI용 초기화 메서드
    /// </summary>
    /// <param name="database"></param>
    public void Initialize(AudioDatabase database)
    {
        audioDatabase = database;
    }

    public void InitializeDict()
    {
        foreach (var audioData in audioDatabase.GetDatabase<AudioData>())
        {
            if (!Enum.TryParse(audioData.name, out AudioName audioName))
            {
                Logger.LogWarning($"{audioData.name} 이름 오류");
                continue;
            }

            switch (audioData.AudioCategory)
            {
                case AudioCategory.Bgm:
                    bgmDict.Add(audioName, audioData.AudioEntries);
                    break;
                case AudioCategory.Sfx:
                    sfxDict.Add(audioName, audioData.AudioEntries);
                    break;
            }
        }
    }

    private void SetAudioSources()
    {
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        pool2D = new(new(), transform, sfxPoolSize);
        pool3D = new(new(), transform, sfxPoolSize);
    }
    #endregion

    #region 오디오 재생 / 정지
    /// <summary>
    /// BGM 재생
    /// </summary>
    /// <param name="audioName"></param>
    public void PlayBgm(AudioName audioName)
    {
        if (!TryGetRandomAudioEntry(audioName, out AudioEntry entry)) return;

        bgmAudioSource.clip = entry.AudioClip;
        bgmAudioSource.volume = entry.Volume;
        bgmAudioSource.Play();
    }

    /// <summary>
    /// 2D 사운드 재생 (UI 등 거리 기반이 필요 없는 사운드)
    /// </summary>
    /// <param name="audioName"></param>
    public void PlaySfx2D(AudioName audioName)
    {
        if (!TryGetRandomAudioEntry(audioName, out AudioEntry entry)) return;

        AudioSource audioSource = pool2D.Get();
        audioSource.clip = entry.AudioClip;
        audioSource.volume = entry.Volume;
        audioSource.Play();
    }

    public void PlaySfx3D(AudioName audioName, Vector3 position)
    {
        if (!TryGetRandomAudioEntry(audioName, out AudioEntry entry)) return;

        AudioSource audioSource = Set3DAudioSource(entry);
        audioSource.transform.position = position;
    }


    public void PlaySfx3D(AudioName audioName, Transform transform)
    {
        if (!TryGetRandomAudioEntry(audioName, out AudioEntry entry)) return;

        AudioSource audioSource = Set3DAudioSource(entry);
        audioSource.transform.position = transform.position;

        active3D.Add(new ActiveAudio
        {
            audioSource = audioSource,
            follow = transform
        });
    }

    #endregion

    #region Utils
    private bool TryGetRandomAudioEntry(AudioName audioName, out AudioEntry entry)
    {
        if (!bgmDict.TryGetValue(audioName, out List<AudioEntry> entries))
        {
            Logger.Log($"{audioName} 오디오 데이터 없음");
            entry = null;
            return false;
        }
        entry = entries[UnityEngine.Random.Range(0, entries.Count)];
        return true;
    }

    private AudioSource Set3DAudioSource(AudioEntry entry)
    {
        AudioSource audioSource = pool3D.Get();
        audioSource.clip = entry.AudioClip;
        audioSource.volume = entry.Volume;
        Configure3D(audioSource);
        audioSource.Play();

        return audioSource;
    }

    private void Configure3D(AudioSource source)
    {
        source.spatialBlend = 1f; // 3D 사운드
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
    }
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        audioDatabase = AssetLoader.FindAndLoadByName<AudioDatabase>("AudioDatabase");
    }
#endif
    #endregion
}
