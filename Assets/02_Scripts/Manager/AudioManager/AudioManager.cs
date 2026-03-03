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

    #region Bgm / Sfx 재생
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBgm(AudioName audioName, int idx = 0, bool loop = true, float pitch = 1f)
    {
        PlayInternal(AudioCategory.Bgm, audioName, bgmAudioSource, idx, loop, pitch);
    }

    /// <summary>
    /// 2D 사운드 재생 (UI 등 거리 기반이 필요 없는 사운드)
    /// </summary>
    public void PlaySfx2D(AudioName audioName, int idx = 0, bool loop = false, float pitch = 1f)
    {
        AudioSource audioSource = pool2D.Get();
        PlayInternal(AudioCategory.Sfx, audioName, audioSource, idx, loop, pitch);
    }

    /// <summary>
    /// 3D 사운드 재생 (고정 위치 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Vector3 position, int idx = -1, bool loop = false, float pitch = 1f)
    {
        AudioSource audioSource = pool3D.Get();
        PlayInternal(AudioCategory.Sfx, audioName, audioSource, idx, loop, pitch, is3D: true);
        audioSource.transform.position = position;
    }

    /// <summary>
    /// 3D 사운드 재생 (따라다니는 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Transform transform, int idx = -1, bool loop = false, float pitch = 1f)
    {
        AudioSource audioSource = pool3D.Get();
        PlayInternal(AudioCategory.Sfx, audioName, audioSource, idx, loop, pitch, is3D: true);
        audioSource.transform.position = transform.position;

        active3D.Add(new ActiveAudio
        {
            audioSource = audioSource,
            follow = transform
        });
    }

    private void PlayInternal(
        AudioCategory audioCategory,
        AudioName audioName,
        AudioSource audioSource,
        int idx,
        bool loop,
        float pitch,
        bool is3D = false)
    {
        bool flowControl = (idx < 0)
            ? TryGetRandomAudioEntry(audioName, out AudioEntry entry)
            : TryGetAudioEntry(audioName, idx, out entry);
        if (!flowControl) return;

        audioSource.clip = entry.AudioClip;
        audioSource.loop = loop;
        audioSource.pitch = pitch;

        float baseVolume = (audioCategory == AudioCategory.Bgm ? bgmVolume : sfxVolume) * entry.Volume;
        audioSource.volume = baseVolume * masterVolume;

        if (is3D)
        {
            Configure3D(audioSource);
        }

        audioSource.Play();
    }
    #endregion

    #region Bgm / Sfx 정지
    public void StopBgm()
    {
        bgmAudioSource.Stop();
    }

    public void StopAllSfx()
    {
        pool2D.ReleaseAll();
        pool3D.ReleaseAll();
        active3D.Clear();
    }

    public void StopAll()
    {
        bgmAudioSource.Stop();
        pool2D.ReleaseAll();
        pool3D.ReleaseAll();
        active3D.Clear();
    }
    #endregion

    #region 볼륨 조절
    #endregion

    #region Utils
    private bool TryGetRandomAudioEntry(AudioName audioName, out AudioEntry entry)
    {
        entry = null;

        if (!bgmDict.TryGetValue(audioName, out List<AudioEntry> entries))
        {
            Logger.LogWarning($"{audioName} 오디오 데이터 없음");
            return false;
        }

        entry = entries[UnityEngine.Random.Range(0, entries.Count)];
        return true;
    }

    private bool TryGetAudioEntry(AudioName audioName, int idx, out AudioEntry entry)
    {
        entry = null;

        if (!bgmDict.TryGetValue(audioName, out List<AudioEntry> entries))
        {
            Logger.LogWarning($"{audioName} 오디오 데이터 없음");
            return false;
        }

        if (idx >= entries.Count)
        {
            Logger.LogWarning("index 벗어남");
            return false;
        }

        entry = entries[idx];
        return true;
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
