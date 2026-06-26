using AudioEnum;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// AudioDatabase 기반 BGM/SFX 재생 진입점
/// </summary>
public class AudioManager : GlobalSingleton<AudioManager>
{
    [Header("데이터베이스")]
    [SerializeField] private AudioDatabase audioDatabase;

    [Header("거리 기반 설정")]
    [SerializeField] private float spatialMinDistance = 3f;
    [SerializeField] private float spatialMaxDistance = 15f;

    [Header("SFX 풀 설정")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private int maxSfxPoolSize = 20;
    [SerializeField] private AudioSource sourcePrefab;

    [Header("오디오 믹서")]
    [SerializeField] private AudioMixer audioMixer;

    private AudioMixerRouter audioRouter;
    private BgmManager bgmManager;
    private SfxManager sfxManager;

    // ----- Unity API -----
    protected override void Awake()
    {
        base.Awake();

        AudioRepository repository = new(audioDatabase);

        audioRouter = new AudioMixerRouter(audioMixer);
        bgmManager = new BgmManager(repository, audioRouter, CreateBgmSource(), CreateBgmSource());
        sfxManager = new SfxManager(repository, audioRouter, CreateAudioSourcePool());
        SetSpatialDistance(spatialMinDistance, spatialMaxDistance);
    }

    private void Start()
    {
        foreach (AudioMixerGroupType mixerGroup in Enum.GetValues(typeof(AudioMixerGroupType)))
        {
            SetVolume(mixerGroup, PlayerPrefs.GetFloat(mixerGroup.ToString(), GameConstants.DefaultVolume));
        }
    }

    protected override void OnDestroy()
    {
        bgmManager?.Stop();
        base.OnDestroy();
    }

    // ----- 초기화 -----
    private AudioSource CreateBgmSource()
    {
        GameObject newGo = new("BGM_Source");
        newGo.transform.parent = transform;
        return newGo.AddComponent<AudioSource>();
    }

    private AudioSourcePool CreateAudioSourcePool()
    {
        GameObject newGo = new("AudioSource_Root");
        newGo.transform.parent = transform;
        return new AudioSourcePool(sourcePrefab, newGo.transform, sfxPoolSize, maxSfxPoolSize);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllSfx();
    }

    // ----- 오디오 재생 -----
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBgm(AudioName audioName, int clipIndex = -1, float fadeDuration = 0.5f)
    {
        bgmManager.Play(audioName, clipIndex, fadeDuration);
    }

    /// <summary>
    /// 2D SFX 재생
    /// </summary>
    public void PlaySfx(AudioName audioName, int clipIndex = -1)
    {
        PlaySfx(audioName, SfxPlayRequest.OneShot(clipIndex));
    }

    /// <summary>
    /// 요청 값 기반 SFX 재생
    /// </summary>
    public void PlaySfx(AudioName audioName, SfxPlayRequest request)
    {
        sfxManager.Play(audioName, request);
    }

    /// <summary>
    /// 취소 가능한 2D SFX 재생
    /// </summary>
    public UniTask PlaySfxAsync(AudioName audioName, CancellationToken ct, int clipIndex = -1)
    {
        return PlaySfxAsync(audioName, SfxPlayRequest.OneShot(clipIndex), ct);
    }

    /// <summary>
    /// 요청 값 기반 취소 가능한 SFX 재생
    /// </summary>
    public UniTask PlaySfxAsync(AudioName audioName, SfxPlayRequest request, CancellationToken ct)
    {
        return sfxManager.PlayAsync(audioName, request, ct);
    }

    /// <summary>
    /// 고정 위치 3D SFX 재생
    /// </summary>
    public void PlaySfxAt(AudioName audioName, Vector3 position, int clipIndex = -1)
    {
        PlaySfx(audioName, SfxPlayRequest.At(position, clipIndex));
    }

    /// <summary>
    /// 취소 가능한 고정 위치 3D SFX 재생
    /// </summary>
    public UniTask PlaySfxAtAsync(AudioName audioName, Vector3 position, CancellationToken ct, int clipIndex = -1)
    {
        return PlaySfxAsync(audioName, SfxPlayRequest.At(position, clipIndex), ct);
    }

    /// <summary>
    /// 대상 Transform을 따라가는 3D SFX 재생
    /// </summary>
    public void PlaySfxFollow(AudioName audioName, Transform target, int clipIndex = -1)
    {
        if (target == null) return;
        PlaySfxAt(audioName, target.position, clipIndex);
    }

    /// <summary>
    /// 취소 가능한 대상 추적 3D SFX 재생
    /// </summary>
    public UniTask PlaySfxFollowAsync(AudioName audioName, Transform target, CancellationToken ct, int clipIndex = -1)
    {
        if (target == null) return UniTask.CompletedTask;
        return PlaySfxAtAsync(audioName, target.position, ct, clipIndex);
    }

    /// <summary>
    /// 취소 가능한 2D Loop SFX 재생
    /// </summary>
    public UniTask PlaySfxLoopAsync(AudioName audioName, CancellationToken ct, int clipIndex = -1)
    {
        return PlaySfxAsync(audioName, SfxPlayRequest.Loop(clipIndex), ct);
    }

    /// <summary>
    /// 취소 가능한 고정 위치 3D Loop SFX 재생
    /// </summary>
    public UniTask PlaySfxLoopAtAsync(AudioName audioName, Vector3 position, CancellationToken ct, int clipIndex = -1)
    {
        return PlaySfxAsync(audioName, SfxPlayRequest.LoopAt(position, clipIndex), ct);
    }

    /// <summary>
    /// 취소 가능한 대상 추적 3D Loop SFX 재생
    /// </summary>
    public UniTask PlaySfxLoopFollowAsync(AudioName audioName, Transform target, CancellationToken ct, int clipIndex = -1)
    {
        if (target == null) return UniTask.CompletedTask;
        return PlaySfxAsync(audioName, SfxPlayRequest.LoopFollow(target, clipIndex), ct);
    }

    // ----- 오디오 일시 정지 -----
    public void PauseAll()
    {
        bgmManager.Pause();
        sfxManager.Pause();
    }

    public void ResumeAll()
    {
        bgmManager.Resume();
        sfxManager.Resume();
    }

    // ----- 오디오 정지 -----
    public void StopBgm()
    {
        bgmManager.Stop();
    }

    public void StopAllSfx()
    {
        sfxManager.StopAll();
    }

    public void StopAll()
    {
        StopBgm();
        StopAllSfx();
    }

    // ----- 볼륨 조절 -----
    public void SetVolume(AudioMixerGroupType type, float normalized)
    {
        audioRouter.SetVolume(type, normalized);
    }

    public float GetVolume(AudioMixerGroupType type)
    {
        return audioRouter.GetVolume01(type);
    }

    public void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public float SpatialMinDistance => spatialMinDistance;
    public float SpatialMaxDistance => spatialMaxDistance;

    public void SetSpatialDistance(float min, float max)
    {
        spatialMinDistance = Mathf.Max(0f, min);
        spatialMaxDistance = Mathf.Max(spatialMinDistance, max);
        sfxManager?.SetSpatialDistance(spatialMinDistance, spatialMaxDistance);
    }

#if UNITY_EDITOR
    [Header("공간 Gizmo")]
    [SerializeField] private bool alwaysDrawSpatialGizmo;
    [SerializeField] private Color spatialMinDistanceGizmoColor = new(0.2f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color spatialMaxDistanceGizmoColor = new(1f, 0.8f, 0.1f, 0.8f);

    public bool AlwaysDrawSpatialGizmo => alwaysDrawSpatialGizmo;
    public Color SpatialMinDistanceGizmoColor => spatialMinDistanceGizmoColor;
    public Color SpatialMaxDistanceGizmoColor => spatialMaxDistanceGizmoColor;

    private void Reset()
    {
        audioDatabase = AssetLoader.FindAndLoadByName<AudioDatabase>("AudioDatabase");
        sourcePrefab = AssetLoader.FindAndLoadByName("AudioSource").GetComponent<AudioSource>();
        audioMixer = AssetLoader.FindAndLoadByName<AudioMixer>("AudioMixer");
    }

    private void OnValidate()
    {
        SetSpatialDistance(spatialMinDistance, spatialMaxDistance);
    }
#endif
}
