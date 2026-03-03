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
    [SerializeField] private AudioSource origin;

    private AudioSource bgmAudioSource; // BGM
    private AudioSourcePool pool2D;     // SFX - UI
    private AudioSourcePool pool3D;     // SFX - 거리 기반

    private List<ActiveAudio> actives = new();

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float masterVolume = 1f;

    private static readonly string BgmVolumeKey = "BgmVolume";
    private static readonly string SfxVolumeKey = "SfxVolume";
    private static readonly string MasterVolumeKey = "MasterVolume";

    private class ActiveAudio
    {
        public AudioSource audioSource;
        public Transform follow;
        public AudioSourcePool pool;
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
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            ActiveAudio active = actives[i];

            if (!active.audioSource.isPlaying)
            {
                active.pool.Release(active.audioSource);
                actives.RemoveAt(i);
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
                    bgmDict[audioName] = audioData.AudioEntries;
                    break;
                case AudioCategory.Sfx:
                    sfxDict[audioName] = audioData.AudioEntries;
                    break;
            }
        }
    }

    private void SetAudioSources()
    {
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        pool2D = new(origin, transform, sfxPoolSize);
        pool3D = new(origin, transform, sfxPoolSize);
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
        audioSource.transform.position = position;
        PlayInternal(AudioCategory.Sfx, audioName, audioSource, idx, loop, pitch, is3D: true);
    }

    /// <summary>
    /// 3D 사운드 재생 (따라다니는 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Transform transform, int idx = -1, bool loop = false, float pitch = 1f)
    {
        AudioSource audioSource = pool3D.Get();
        audioSource.transform.position = transform.position;
        ActiveAudio activeAudio = PlayInternal(AudioCategory.Sfx, audioName, audioSource, idx, loop, pitch, is3D: true);

        if (activeAudio != null)
        {
            activeAudio.follow = transform;
        }
    }

    private ActiveAudio PlayInternal(
        AudioCategory audioCategory,
        AudioName audioName,
        AudioSource audioSource,
        int idx,
        bool loop,
        float pitch,
        bool is3D = false)
    {
        if (!TryGetAudioEntry(audioCategory, audioName, idx, out AudioEntry entry)) { return null; }

        audioSource.clip = entry.AudioClip;
        audioSource.loop = loop;
        audioSource.pitch = pitch;

        float baseVolume = (audioCategory == AudioCategory.Bgm ? bgmVolume : sfxVolume) * entry.Volume;
        audioSource.volume = baseVolume * masterVolume;

        if (audioCategory == AudioCategory.Bgm)
        {
            audioSource.Play();
            return null;
        }

        var newActiveAudio = new ActiveAudio
        {
            audioSource = audioSource,
            pool = is3D ? pool3D : pool2D
        };
        actives.Add(newActiveAudio);

        if (is3D)
        {
            Configure3D(audioSource);
        }

        audioSource.Play();

        return newActiveAudio;
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
        actives.Clear();
    }

    public void StopAll()
    {
        bgmAudioSource.Stop();
        pool2D.ReleaseAll();
        pool3D.ReleaseAll();
        actives.Clear();
    }
    #endregion

    #region 볼륨 조절
    #endregion

    #region Utils
    private bool TryGetAudioEntry(AudioCategory audioCategory, AudioName audioName, int idx, out AudioEntry entry)
    {
        entry = null;

        Dictionary<AudioName, List<AudioEntry>> dict = (audioCategory == AudioCategory.Bgm) ? bgmDict : sfxDict;
        if (!dict.TryGetValue(audioName, out List<AudioEntry> entries) || entries == null || entries.Count == 0)
        {
            Logger.LogWarning($"{audioName} 오디오 데이터 없음");
            return false;
        }

        if (idx >= entries.Count || idx < 0)
        {
            idx = UnityEngine.Random.Range(0, entries.Count);
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
