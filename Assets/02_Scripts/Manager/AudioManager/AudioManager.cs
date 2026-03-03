using AudioEnum;
using UnityEngine;

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
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    [Header("풀 세팅")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private int maxSfxPoolSize = 20;
    [SerializeField] private AudioSource origin;

    private IAudioHandle bgmAudioHandle; // BGM
    private IAudioSourcePool pool2D;     // SFX - UI
    private IAudioSourcePool pool3D;     // SFX - 거리 기반

    private AudioPlayer audioPlayer;
    #endregion

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        var adapter = new AudioDatabaseAdapter(audioDatabase, new UnityRandom());
        audioPlayer = new AudioPlayer(adapter, minDistance, maxDistance);

        SetAudioSources();
    }

    private void Update()
    {
        audioPlayer.Tick();
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
        bgmAudioHandle = new AudioHandle(gameObject.AddComponent<AudioSource>());
        pool2D = new AudioSourcePool(origin, transform, sfxPoolSize);
        pool3D = new AudioSourcePool(origin, transform, sfxPoolSize);
    }
    #endregion

    #region Bgm / Sfx 재생
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBgm(AudioName audioName, int idx = 0, bool loop = true, float pitch = 1f)
    {
        audioPlayer.Play(AudioCategory.Bgm, audioName, bgmAudioHandle, null, idx, loop, pitch);
    }

    /// <summary>
    /// 2D 사운드 재생 (UI 등 거리 기반이 필요 없는 사운드)
    /// </summary>
    public void PlaySfx2D(AudioName audioName, int idx = 0, bool loop = false, float pitch = 1f)
    {
        IAudioHandle handle = pool2D.Get();
        audioPlayer.Play(AudioCategory.Sfx, audioName, handle, pool2D, idx, loop, pitch);
    }

    /// <summary>
    /// 3D 사운드 재생 (고정 위치 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Vector3 position, int idx = -1, bool loop = false, float pitch = 1f)
    {
        IAudioHandle handle = pool3D.Get();
        handle.SetPosition(position);
        audioPlayer.Play(AudioCategory.Sfx, audioName, handle, pool3D, idx, loop, pitch, is3D: true);
    }

    /// <summary>
    /// 3D 사운드 재생 (따라다니는 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Transform transform, int idx = -1, bool loop = false, float pitch = 1f)
    {
        IAudioHandle handle = pool3D.Get();
        handle.SetPosition(transform.position);
        ActiveAudio activeAudio = audioPlayer
            .Play(AudioCategory.Sfx, audioName, handle, pool3D, idx, loop, pitch, is3D: true);

        if (activeAudio != null)
        {
            activeAudio.follow = transform;
        }
    }
    #endregion

    #region Bgm / Sfx 정지
    public void StopBgm()
    {
        bgmAudioHandle.Stop();
    }

    public void StopAllSfx()
    {
        pool2D.ReleaseAll();
        pool3D.ReleaseAll();
        audioPlayer.Actives.Clear();
    }

    public void StopAll()
    {
        bgmAudioHandle.Stop();
        pool2D.ReleaseAll();
        pool3D.ReleaseAll();
        audioPlayer.Actives.Clear();
    }
    #endregion

    #region 볼륨 조절
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        audioDatabase = AssetLoader.FindAndLoadByName<AudioDatabase>("AudioDatabase");
        origin = AssetLoader.FindAndLoadByName("AudioSource").GetComponent<AudioSource>();
    }
#endif
    #endregion
}
