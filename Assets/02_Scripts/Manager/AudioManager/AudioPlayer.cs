using AudioEnum;
using System.Collections.Generic;

/// <summary>
/// 오디오 재생, 볼륨 관리
/// </summary>
public class AudioPlayer
{
    #region 필드
    private AudioDatabaseAdapter adapter;

    // 볼륨
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private float masterVolume = 1f;

    private static readonly string BgmVolumeKey = "BgmVolume";
    private static readonly string SfxVolumeKey = "SfxVolume";
    private static readonly string MasterVolumeKey = "MasterVolume";

    private List<ActiveAudio> actives = new();
    public List<ActiveAudio> Actives => actives;

    private readonly float minDistance;
    private readonly float maxDistance;
    #endregion

    public AudioPlayer(
        AudioDatabaseAdapter adapter,
        float minDistance,
        float maxDistance)
    {
        this.adapter = adapter;
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }

    public void Tick()
    {
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            ActiveAudio active = actives[i];

            if (!active.handle.IsPlaying)
            {
                active.pool?.Release(active.handle);
                actives.RemoveAt(i);
                continue;
            }

            if (active.follow != null)
            {
                active.handle.SetPosition(active.follow.position);
            }
        }
    }

    public ActiveAudio Play(
        AudioCategory audioCategory,
        AudioName audioName,
        IAudioHandle handle,
        IAudioSourcePool pool,
        int idx,
        bool loop,
        float pitch,
        bool is3D = false)
    {
        if (!adapter.TryGetAudioEntry(audioCategory, audioName, idx, out AudioEntry entry)) { return null; }

        handle.SetClip(entry.AudioClip);
        handle.SetLoop(loop);
        handle.SetPitch(pitch);

        float baseVolume = (audioCategory == AudioCategory.Bgm ? bgmVolume : sfxVolume) * entry.Volume;
        handle.SetVolume(baseVolume * masterVolume);

        if (audioCategory == AudioCategory.Bgm)
        {
            handle.Play();
            return null;
        }

        var newActiveAudio = new ActiveAudio
        {
            handle = handle,
            pool = pool
        };
        actives.Add(newActiveAudio);

        if (is3D)
        {
            handle.Set3D(minDistance, maxDistance);
        }
        else
        {
            handle.Set2D();
        }

        handle.Play();

        return newActiveAudio;
    }
}
