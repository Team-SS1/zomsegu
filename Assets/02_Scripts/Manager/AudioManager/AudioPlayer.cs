using AudioEnum;
using System.Collections.Generic;
using UnityEngine;

public class ActiveAudio
{
    public AudioSource audioSource;
    public Transform follow;
    public AudioSourcePool pool;
}

public class AudioPlayer : MonoBehaviour
{
    #region 필드
    [Header("데이터베이스")]
    [SerializeField] private AudioDatabase audioDatabase;
    private AudioDatabaseAdapter adapter;

    [Header("거리 기반 세팅")]
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    // 볼륨
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float masterVolume = 1f;

    private static readonly string BgmVolumeKey = "BgmVolume";
    private static readonly string SfxVolumeKey = "SfxVolume";
    private static readonly string MasterVolumeKey = "MasterVolume";

    private List<ActiveAudio> actives = new();
    public List<ActiveAudio> Actives => actives;
    #endregion

    #region Unity API
    private void Awake()
    {
        adapter = new(audioDatabase);
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
    #endregion

    public ActiveAudio PlayInternal(
        AudioCategory audioCategory,
        AudioName audioName,
        AudioSource audioSource,
        AudioSourcePool pool,
        int idx,
        bool loop,
        float pitch,
        bool is3D = false)
    {
        if (!adapter.TryGetAudioEntry(audioCategory, audioName, idx, out AudioEntry entry)) { return null; }

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
            pool = pool
        };
        actives.Add(newActiveAudio);

        if (is3D)
        {
            Configure3D(audioSource);
        }

        audioSource.Play();

        return newActiveAudio;
    }

    private void Configure3D(AudioSource source)
    {
        source.spatialBlend = 1f; // 3D 사운드
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        audioDatabase = AssetLoader.FindAndLoadByName<AudioDatabase>("AudioDatabase");
    }
#endif
    #endregion
}
