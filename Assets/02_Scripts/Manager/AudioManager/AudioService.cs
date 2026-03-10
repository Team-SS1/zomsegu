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
    private readonly AudioCooldown cooldown = new();

    private readonly List<ActiveVoice> activeVoices = new();

    private IAudioInstance bgmInstance;     // BGM
    private IAudioSourcePool pool;          // SFX 풀

    // todo: 확정되면 상수로 빼기
    private readonly float spatialMinDistance;
    private readonly float spatialMaxDistance;

    private bool isPaused = false;
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
    }
    #endregion

    public void Update()
    {
        if (isPaused) return;

        for (int i = activeVoices.Count - 1; i >= 0; i--)
        {
            ActiveVoice active = activeVoices[i];

            if (!active.instance.IsPlaying)
            {
                pool.Release(active.instance);
                activeVoices.RemoveAt(i);
                continue;
            }

            if (active.follow != null)
            {
                active.instance.SetPosition(active.follow.position);
            }
        }
    }

    #region 오디오 재생
    public void PlayBgm(AudioName audioName, int clipIndex)
    {
        if (!repository.TryGetAudioData(audioName, out AudioData data)) return;

        AudioVariation variation = data.GetVariation(clipIndex);

        if (!cooldown.CanPlay(audioName, data.Cooldown, Time.unscaledTime)) return;

        bgmInstance.Stop();

        AudioPlaybackConfig config = new(
            clip: variation.AudioClip,
            mixerGroup: audioRouter.GetMixerGroup(GetRoute(AudioCategory.Bgm)),
            loop: data.Loop,
            spatial: false,
            priority: data.Priority);

        bgmInstance.SetConfig(config);
        bgmInstance.SetVolume(variation.Volume * data.RandomVolume);
        bgmInstance.SetPitch(data.RandomPitch);
        bgmInstance.Play();
    }

    public void PlaySfx(AudioName audioName, int clipIndex, Vector3 position = default, Transform target = null)
    {
        if (!repository.TryGetAudioData(audioName, out AudioData data)) return;

        if (!cooldown.CanPlay(audioName, data.Cooldown, Time.unscaledTime)) return;

        IAudioInstance instance = pool.Get();

        AudioVariation variation = data.GetVariation(clipIndex);

        AudioPlaybackConfig config = new(
            clip: variation.AudioClip,
            mixerGroup: audioRouter.GetMixerGroup(GetRoute(data.AudioCategory)),
            loop: data.Loop,
            spatial: data.Spatial,
            priority: data.Priority,
            spatialMinDistance: spatialMinDistance,
            spatialMaxDistance: spatialMaxDistance);

        instance.SetConfig(config);
        instance.SetVolume(variation.Volume * data.RandomVolume);
        instance.SetPitch(data.RandomPitch);
        instance.SetPosition(position);

        instance.Play();

        activeVoices.Add(new ActiveVoice
        {
            instance = instance,
            follow = target,
            priority = data.Priority
        });
    }
    #endregion

    #region 오디오 일시정지
    public void PauseAll()
    {
        if (isPaused) return;
        isPaused = true;

        bgmInstance.Pause();
        foreach (ActiveVoice audio in activeVoices)
        {
            audio.instance.Pause();
        }
    }

    public void UnPauseAll()
    {
        if (!isPaused) return;
        isPaused = false;

        bgmInstance.UnPause();
        foreach (ActiveVoice audio in activeVoices)
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
        foreach (ActiveVoice audio in activeVoices)
        {
            pool.Release(audio.instance);
        }

        activeVoices.Clear();
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

    #region Utils
    private AudioMixerGroupType GetRoute(AudioCategory category)
    {
        return category switch
        {
            AudioCategory.Bgm => AudioMixerGroupType.Bgm,
            AudioCategory.UI => AudioMixerGroupType.UI,
            AudioCategory.Gameplay => AudioMixerGroupType.Gameplay,
            AudioCategory.Ambient => AudioMixerGroupType.Ambient,
            _ => AudioMixerGroupType.Gameplay
        };
    }
    #endregion
}
