using AudioEnum;
using System.Collections.Generic;

/// <summary>
/// 오디오 재생, 볼륨 관리
/// </summary>
public class AudioService
{
    #region 필드
    private readonly AudioRepository repository;
    private readonly AudioMixerController mixerController;

    private readonly List<PlayingAudio> activeAudios = new();

    private readonly float spatialMinDistance;
    private readonly float spatialMaxDistance;
    #endregion

    public AudioService(
        AudioRepository repository,
        AudioMixerController mixerController,
        float minDist,
        float maxDist)
    {
        this.repository = repository;
        this.mixerController = mixerController;
        spatialMinDistance = minDist;
        spatialMaxDistance = maxDist;
    }

    public void Update()
    {
        for (int i = activeAudios.Count - 1; i >= 0; i--)
        {
            PlayingAudio active = activeAudios[i];

            if (!active.instance.IsPlaying)
            {
                active.pool?.Release(active.instance);
                activeAudios.RemoveAt(i);
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
        int clipIndex,
        bool loop,
        float pitch,
        bool useSpatial = false)
    {
        if (!repository.TryGetAudioEntry(audioName, clipIndex, out AudioEntry entry)) { return null; }

        instance.SetClip(entry.AudioClip);
        instance.SetLoop(loop);
        instance.SetPitch(pitch);
        instance.SetVolume(entry.Volume);

        instance.SetOutputAudioMixerGroup(
            audioCategory == AudioCategory.Bgm
            ? mixerController.BgmGroup
            : mixerController.SfxGroup);

        if (audioCategory == AudioCategory.Bgm)
        {
            instance.Play();
            return null;
        }

        var newPlayingAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool
        };

        activeAudios.Add(newPlayingAudio);

        ApplySpatial(instance, useSpatial);

        instance.Play();

        return newPlayingAudio;
    }

    public void PauseAll()
    {
        foreach (PlayingAudio audio in activeAudios)
        {
            audio.instance.Pause();
        }
    }

    public void UnPauseAll()
    {
        foreach (PlayingAudio audio in activeAudios)
        {
            audio.instance.UnPause();
        }
    }

    public void StopAllSfx()
    {
        foreach (PlayingAudio audio in activeAudios)
        {
            audio.pool?.Release(audio.instance);
        }

        activeAudios.Clear();
    }

    private void ApplySpatial(IAudioInstance instance, bool useSpatial)
    {
        if (useSpatial)
        {
            instance.Set3D(spatialMinDistance, spatialMaxDistance);
        }
        else
        {
            instance.Set2D();
        }
    }
}
