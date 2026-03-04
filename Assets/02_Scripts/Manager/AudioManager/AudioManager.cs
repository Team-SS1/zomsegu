using AudioEnum;
using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// AudioDatabase로 BGM/SFX 관리
/// 2D(UI) / 3D(거리 기반) SFX를 풀링해 놓은 AudioSource로 제공
/// </summary>
public class AudioManager : GlobalSingleton<AudioManager>
{
    #region 필드
    [Header("데이터베이스")]
    [SerializeField] private AudioDatabase audioDatabase;

    [Header("거리 기반 세팅")]
    [SerializeField] private float spatialMinDistance = 3f;
    [SerializeField] private float spatialMaxDistance = 15f;

    [Header("풀 세팅")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private int maxSfxPoolSize = 20;
    [SerializeField] private AudioSource origin;

    private IAudioInstance bgmAudioInstance;    // BGM
    private IAudioSourcePool pool;              // SFX 풀

    [Header("오디오 믹서")]
    [SerializeField] private AudioMixer audioMixer;

    private AudioService audioService;
    private AudioMixerController audioMixerController;
    #endregion

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        var repository = new AudioRepository(audioDatabase, new UnityRandom());
        audioMixerController = new AudioMixerController(audioMixer);
        audioService = new AudioService(repository, audioMixerController, spatialMinDistance, spatialMaxDistance);

        SetAudioSources();

        bgmAudioInstance.SetOutputAudioMixerGroup(audioMixerController.BgmGroup);
    }

    private void Start()
    {
        foreach (AudioMixerGroupType mixerGroup in Enum.GetValues(typeof(AudioMixerGroupType)))
        {
            SetVolume(mixerGroup, PlayerPrefs.GetFloat(mixerGroup.ToString(), GameConstants.DefaultVolume));
        }
    }

    private void Update()
    {
        audioService.Tick();
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

    private void SetAudioSources()
    {
        bgmAudioInstance = new AudioInstance(gameObject.AddComponent<AudioSource>());
        pool = new AudioSourcePool(origin, transform, sfxPoolSize);
    }
    #endregion

    #region Bgm / Sfx 재생
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBgm(AudioName audioName, int idx = 0, bool loop = true, float pitch = 1f)
    {
        bgmAudioInstance.Stop();
        audioService.Play(AudioCategory.Bgm, audioName, bgmAudioInstance, null, idx, loop, pitch);
    }

    /// <summary>
    /// 2D 사운드 재생 (UI 등 거리 기반이 필요 없는 사운드)
    /// </summary>
    public void PlaySfx2D(AudioName audioName, int idx = 0, bool loop = false, float pitch = 1f)
    {
        IAudioInstance instance = pool.Get();
        audioService.Play(AudioCategory.Sfx, audioName, instance, pool, idx, loop, pitch);
    }

    /// <summary>
    /// 3D 사운드 재생 (고정 위치 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Vector3 position, int idx = -1, bool loop = false, float pitch = 1f)
    {
        IAudioInstance instance = pool.Get();
        instance.SetPosition(position);
        audioService.Play(AudioCategory.Sfx, audioName, instance, pool, idx, loop, pitch, is3D: true);
    }

    /// <summary>
    /// 3D 사운드 재생 (따라다니는 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Transform transform, int idx = -1, bool loop = false, float pitch = 1f)
    {
        IAudioInstance instance = pool.Get();
        instance.SetPosition(transform.position);
        PlayingAudio activeAudio = audioService
            .Play(AudioCategory.Sfx, audioName, instance, pool, idx, loop, pitch, is3D: true);

        if (activeAudio != null)
        {
            activeAudio.follow = transform;
        }
    }
    #endregion

    #region Bgm / Sfx 정지
    public void StopBgm()
    {
        bgmAudioInstance.Stop();
    }

    public void StopAllSfx()
    {
        pool.ReleaseAll();
        audioService.Actives.Clear();
    }

    public void StopAll()
    {
        bgmAudioInstance.Stop();
        pool.ReleaseAll();
        audioService.Actives.Clear();
    }
    #endregion

    #region 볼륨 조절
    public void SetVolume(AudioMixerGroupType type, float normalized) => audioMixerController.SetVolume(type, normalized);
    public float GetVolume(AudioMixerGroupType type) => audioMixerController.GetVolume01(type);
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        audioDatabase = AssetLoader.FindAndLoadByName<AudioDatabase>("AudioDatabase");
        origin = AssetLoader.FindAndLoadByName("AudioSource").GetComponent<AudioSource>();
        audioMixer = AssetLoader.FindAndLoadByName<AudioMixer>("AudioMixer");
    }
#endif
    #endregion
}
