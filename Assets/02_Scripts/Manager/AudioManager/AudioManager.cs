using AudioEnum;
using UnityEngine;

[RequireComponent(typeof(AudioPlayer))]
public class AudioManager : GlobalSingleton<AudioManager>
{
    #region 필드
    [Header("풀 세팅")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private int maxSfxPoolSize = 20;
    [SerializeField] private AudioSource origin;

    private AudioSource bgmAudioSource; // BGM
    private AudioSourcePool pool2D;     // SFX - UI
    private AudioSourcePool pool3D;     // SFX - 거리 기반

    private AudioPlayer audioPlayer;
    #endregion

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        SetAudioSources();
        audioPlayer = GetComponent<AudioPlayer>();
    }
    #endregion

    #region 초기화
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
        audioPlayer.PlayInternal(AudioCategory.Bgm, audioName, bgmAudioSource, null, idx, loop, pitch);
    }

    /// <summary>
    /// 2D 사운드 재생 (UI 등 거리 기반이 필요 없는 사운드)
    /// </summary>
    public void PlaySfx2D(AudioName audioName, int idx = 0, bool loop = false, float pitch = 1f)
    {
        AudioSource audioSource = pool2D.Get();
        audioPlayer.PlayInternal(AudioCategory.Sfx, audioName, audioSource, pool2D, idx, loop, pitch);
    }

    /// <summary>
    /// 3D 사운드 재생 (고정 위치 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Vector3 position, int idx = -1, bool loop = false, float pitch = 1f)
    {
        AudioSource audioSource = pool3D.Get();
        audioSource.transform.position = position;
        audioPlayer.PlayInternal(AudioCategory.Sfx, audioName, audioSource, pool3D, idx, loop, pitch, is3D: true);
    }

    /// <summary>
    /// 3D 사운드 재생 (따라다니는 사운드 재생용)
    /// 기본적으로 랜덤 클립 재생(index == -1), idx로 특정 클립 재생 가능
    /// </summary>
    public void PlaySfx3D(AudioName audioName, Transform transform, int idx = -1, bool loop = false, float pitch = 1f)
    {
        AudioSource audioSource = pool3D.Get();
        audioSource.transform.position = transform.position;
        ActiveAudio activeAudio = audioPlayer
            .PlayInternal(AudioCategory.Sfx, audioName, audioSource, pool3D, idx, loop, pitch, is3D: true);

        if (activeAudio != null)
        {
            activeAudio.follow = transform;
        }
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
        audioPlayer.Actives.Clear();
    }

    public void StopAll()
    {
        bgmAudioSource.Stop();
        pool2D.ReleaseAll();
        pool3D.ReleaseAll();
        audioPlayer.Actives.Clear();
    }
    #endregion

    #region 볼륨 조절
    #endregion
}
