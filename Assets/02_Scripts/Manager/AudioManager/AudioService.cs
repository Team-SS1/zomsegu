using AudioEnum;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오디오 재생, 볼륨 관리
/// </summary>
public class AudioService
{
    #region 필드
    private readonly AudioRepository repository;
    private readonly IAudioRouter audioRouter;

    private readonly List<PlayingAudio> activeAudios = new();

    private IAudioInstance bgmInstance;     // BGM
    private IAudioSourcePool pool;          // SFX 풀

    // todo: 확정되면 상수로 빼기
    private readonly float spatialMinDistance;
    private readonly float spatialMaxDistance;
    #endregion

    #region 초기화
    public AudioService(
        AudioRepository repository,
        IAudioRouter audioRouter,
        float minDist,
        float maxDist)
    {
        this.repository = repository;
        this.audioRouter = audioRouter;
        spatialMinDistance = minDist;
        spatialMaxDistance = maxDist;
    }

    public void InitializeRuntime(IAudioInstance bgmInstance, IAudioSourcePool pool)
    {
        this.bgmInstance = bgmInstance;
        this.pool = pool;
        bgmInstance.SetOutputAudioMixerGroup(audioRouter.GetMixerGroup(AudioMixerGroupType.Bgm));
    }
    #endregion

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

    public void Play(
        AudioCategory audioCategory,
        AudioName audioName,
        int clipIndex,
        bool loop,
        float pitch)
    {
        IAudioInstance instance = (audioCategory) switch
        {
            AudioCategory.Bgm => bgmInstance,
            _ => pool.Get()
        };

        if (!SetInstance(audioName, clipIndex, loop, pitch, instance)) { return; }

        if (audioCategory == AudioCategory.Bgm)
        {
            instance.Play();
            return;
        }

        instance.SetOutputAudioMixerGroup(router.GetAudioMixerGroup(AudioMixerGroupType.Sfx));

        var newPlayingAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool
        };

        activeAudios.Add(newPlayingAudio);

        ApplySpatial(instance, useSpatial: false);

        instance.Play();
    }

    public void Play(
        AudioCategory audioCategory,
        AudioName audioName,
        Vector3 position,
        int clipIndex,
        bool loop,
        float pitch)
    {
        IAudioInstance instance = (audioCategory) switch
        {
            AudioCategory.Bgm => bgmInstance,
            _ => pool.Get()
        };

        if (!SetInstance(audioName, clipIndex, loop, pitch, instance)) { return; }

        if (audioCategory == AudioCategory.Bgm)
        {
            instance.Play();
            return;
        }

        instance.SetPosition(position);
        instance.SetOutputAudioMixerGroup(router.GetAudioMixerGroup(AudioMixerGroupType.Sfx));

        var newPlayingAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool
        };

        activeAudios.Add(newPlayingAudio);

        ApplySpatial(instance, useSpatial: true);

        instance.Play();
    }

    public void Play(
        AudioCategory audioCategory,
        AudioName audioName,
        Transform transform,
        int clipIndex,
        bool loop,
        float pitch)
    {
        IAudioInstance instance = (audioCategory) switch
        {
            AudioCategory.Bgm => bgmInstance,
            _ => pool.Get()
        };

        if (!SetInstance(audioName, clipIndex, loop, pitch, instance)) { return; }

        if (audioCategory == AudioCategory.Bgm)
        {
            instance.Play();
            return;
        }

        instance.SetPosition(transform.position);
        instance.SetOutputAudioMixerGroup(router.GetAudioMixerGroup(AudioMixerGroupType.Sfx));

        var newPlayingAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool,
            follow = transform
        };

        activeAudios.Add(newPlayingAudio);

        ApplySpatial(instance, useSpatial: true);

        instance.Play();
    }

    private bool SetInstance(AudioName audioName, int clipIndex, bool loop, float pitch, IAudioInstance instance)
    {
        if (!repository.TryGetAudioEntry(audioName, clipIndex, out AudioEntry entry))
        {
            return false;
        }

        instance.SetClip(entry.AudioClip);
        instance.SetLoop(loop);
        instance.SetPitch(pitch);
        instance.SetVolume(entry.Volume);

        return true;
    }

    public void PauseAll()
    {
        bgmInstance.Pause();
        foreach (PlayingAudio audio in activeAudios)
        {
            audio.instance.Pause();
        }
    }

    public void UnPauseAll()
    {
        bgmInstance.UnPause();
        foreach (PlayingAudio audio in activeAudios)
        {
            audio.instance.UnPause();
        }
    }

    public void StopBgm()
    {
        bgmInstance.Stop();
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

    #region 볼륨 조절
    public void SetVolume(AudioMixerGroupType type, float normalized)
    {
        audioRouter.SetVolume(type, normalized);
    }

    public float GetVolume(AudioMixerGroupType type)
    {
        return audioRouter.GetVolume01(type);
    }
    #endregion
}
