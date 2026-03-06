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
    [SerializeField] private AudioSource sourcePrefab;


    [Header("오디오 믹서")]
    [SerializeField] private AudioMixer audioMixer;

    private AudioService audioService;

    private bool isPaused = false;
    #endregion

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        var repository = new AudioRepository(audioDatabase);
        var audioRouter = new AudioMixerRouter(audioMixer);

        audioService = new AudioService(repository, audioRouter, spatialMinDistance, spatialMaxDistance);

        IAudioInstance bgmInstance = CreateBgmInstance();
        IAudioSourcePool pool = CreateAudioSourcePool();

        audioService.InitializeRuntime(bgmInstance, pool);
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
        audioService.Update();
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

    private AudioInstance CreateBgmInstance()
    {
        GameObject newGo = new("BGM_Source");
        newGo.transform.parent = transform;
        return new AudioInstance(newGo.AddComponent<AudioSource>());
    }

    private AudioSourcePool CreateAudioSourcePool()
    {
        GameObject newGo = new("AudioSource_Root");
        newGo.transform.parent = transform;
        return new AudioSourcePool(sourcePrefab, newGo.transform, sfxPoolSize, maxSfxPoolSize);
    }
    #endregion

    #region 오디오 재생
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBgm(AudioName audioName, int clipIndex = -1)
    {
        audioService.PlayBgm(audioName, clipIndex);
    }

    /// <summary>
    /// 2D 사운드 재생 (발소리 등 거리 기반이 필요 없는 사운드)
    /// </summary>
    public void PlaySfx(AudioName audioName, int clipIndex = -1)
    {
        audioService.PlaySfx(audioName, clipIndex);
    }

    /// <summary>
    /// 3D 사운드 재생 (고정 위치 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfxAt(AudioName audioName, Vector3 position, int clipIndex = -1)
    {
        audioService.PlayAt(audioName, position, clipIndex);
    }

    /// <summary>
    /// 3D 사운드 재생 (따라다니는 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfxFollow(AudioName audioName, Transform target, int clipIndex = -1)
    {
        audioService.PlayFollow(audioName, target, clipIndex);
    }
    #endregion

    #region 오디오 일시 정지
    public void PauseAll()
    {
        if (isPaused) return;
        isPaused = true;
        audioService.PauseAll();
    }

    public void ResumeAll()
    {
        if (!isPaused) return;
        isPaused = false;
        audioService.UnPauseAll();
    }
    #endregion

    #region 오디오 정지
    public void StopBgm()
    {
        audioService.StopBgm();
    }

    public void StopAllSfx()
    {
        audioService.StopAllSfx();
    }

    public void StopAll()
    {
        audioService.StopBgm();
        StopAllSfx();
    }
    #endregion

    #region 볼륨 조절
    public void SetVolume(AudioMixerGroupType type, float normalized)
    {
        audioService.SetVolume(type, normalized);
    }

    public float GetVolume(AudioMixerGroupType type)
    {
        return audioService.GetVolume(type);
    }

    public void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        audioDatabase = AssetLoader.FindAndLoadByName<AudioDatabase>("AudioDatabase");
        sourcePrefab = AssetLoader.FindAndLoadByName("AudioSource").GetComponent<AudioSource>();
        audioMixer = AssetLoader.FindAndLoadByName<AudioMixer>("AudioMixer");
    }
#endif
    #endregion
}
