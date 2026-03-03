using AudioEnum;
using System.Collections.Generic;

/// <summary>
/// 오디오 재생, 볼륨 관리
/// </summary>
public class AudioService
{
    #region 필드
    private AudioRepository repository;

    // 볼륨
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float masterVolume = 1f;

    private static readonly string BgmVolumeKey = "BgmVolume";
    private static readonly string SfxVolumeKey = "SfxVolume";
    private static readonly string MasterVolumeKey = "MasterVolume";

    private List<PlayingAudio> actives = new();
    public List<PlayingAudio> Actives => actives;

    private readonly float spatialMinDistance;
    private readonly float spatialMaxDistance;
    #endregion

    public AudioService(
        AudioRepository repository,
        float minDistance,
        float maxDistance)
    {
        this.repository = repository;
        this.spatialMinDistance = minDistance;
        this.spatialMaxDistance = maxDistance;
    }

    public void Tick()
    {
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            PlayingAudio active = actives[i];

            if (!active.instance.IsPlaying)
            {
                active.pool?.Release(active.instance);
                actives.RemoveAt(i);
                continue;
            }

            if (active.follow != null)
            {
                active.instance.SetPosition(active.follow.position);
            }
        }
    }

    public PlayingAudio Play(
        AudioCategory audioCategory,
        AudioName audioName,
        IAudioInstance instance,
        IAudioSourcePool pool,
        int idx,
        bool loop,
        float pitch,
        bool is3D = false)
    {
        if (!repository.TryGetAudioEntry(audioCategory, audioName, idx, out AudioEntry entry)) { return null; }

        instance.SetClip(entry.AudioClip);
        instance.SetLoop(loop);
        instance.SetPitch(pitch);

        float baseVolume = (audioCategory == AudioCategory.Bgm ? bgmVolume : sfxVolume) * entry.Volume;
        instance.SetVolume(baseVolume * masterVolume);

        if (audioCategory == AudioCategory.Bgm)
        {
            instance.Play();
            return null;
        }

        var newActiveAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool
        };
        actives.Add(newActiveAudio);

        if (is3D)
        {
            instance.Set3D(spatialMinDistance, spatialMaxDistance);
        }
        else
        {
            instance.Set2D();
        }

        instance.Play();

        return newActiveAudio;
    }
}
