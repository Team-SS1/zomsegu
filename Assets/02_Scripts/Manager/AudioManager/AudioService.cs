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

    private readonly Dictionary<AudioCategory, AudioRoute> routes;

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

        // 오디오 믹서 규칙
        routes = new()
        {
            { AudioCategory.Bgm, new AudioRoute(AudioMixerGroupType.Bgm, allowSpatial:false) },
            { AudioCategory.UI, new AudioRoute(AudioMixerGroupType.Sfx, allowSpatial:false) },
            { AudioCategory.Sfx, new AudioRoute(AudioMixerGroupType.Sfx, allowSpatial:true) },
        };
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

    #region 오디오 재생
    public void PlayBgm(AudioName audioName, in AudioPlayOptions options)
    {
        PlayCore(AudioCategory.Bgm, audioName, bgmInstance, options);
    }

    private bool PlayCore(AudioCategory audioCategory, AudioName audioName, IAudioInstance instance, in AudioPlayOptions options)
    {
        if (!repository.TryGetAudioEntry(audioName, options.clipIndex, out AudioEntry entry))
        {
            return false;
        }

        instance.SetClip(entry.AudioClip);
        instance.SetLoop(options.loop);
        instance.SetPitch(options.pitch);
        instance.SetVolume(entry.Volume);

        AudioRoute route = routes[audioCategory];
        instance.SetOutputAudioMixerGroup(audioRouter.GetMixerGroup(route.mixerGroupType));
        ApplySpatial(instance, route.allowSpatial);

        instance.Play();

        return true;
    }

    public void PlaySfx(AudioCategory audioCategory, AudioName audioName, in AudioPlayOptions options)
    {
        IAudioInstance instance = pool.Get();
        PlayCore(audioCategory, audioName, instance, options);

        var newPlayingAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool
        };

        activeAudios.Add(newPlayingAudio);
    }

    public void Play(AudioCategory audioCategory, AudioName audioName, Vector3 position, in AudioPlayOptions options)
    {
        IAudioInstance instance = pool.Get();
        instance.SetPosition(position);
        PlayCore(audioCategory, audioName, instance, options);

        var newPlayingAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool
        };

        activeAudios.Add(newPlayingAudio);
    }

    public void Play(
        AudioCategory audioCategory,
        AudioName audioName,
        Transform target,
        in AudioPlayOptions options)
    {
        IAudioInstance instance = pool.Get();
        instance.SetPosition(target.position);
        PlayCore(audioCategory, audioName, instance, options);

        var newPlayingAudio = new PlayingAudio
        {
            instance = instance,
            pool = pool,
            follow = target
        };

        activeAudios.Add(newPlayingAudio);
    }
    #endregion

    #region 오디오 일시정지
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
    #endregion

    #region 오디오 정지
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
    #endregion

    #region 오디오 세팅
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
    #endregion

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
