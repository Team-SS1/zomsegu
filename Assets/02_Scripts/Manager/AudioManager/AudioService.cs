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
        for (int i = activeVoices.Count - 1; i >= 0; i--)
        {
            ActiveVoice active = activeVoices[i];

            if (!active.instance.IsPlaying)
            {
                active.pool?.Release(active.instance);
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
        bgmInstance.Stop();
        TryPlay(audioName, bgmInstance, clipIndex, out ActiveVoice playingAudio);
    }

    private bool TryPlay(AudioName audioName, IAudioInstance instance, int clipIndex, out ActiveVoice playingAudio)
    {
        playingAudio = null;

        if (!repository.TryGetAudioData(audioName, out AudioData data))
        {
            return false;
        }

        AudioVariation entry = data.GetEntry(clipIndex);

        if (!AudioCooldown.CanPlay(audioName, data.Cooldown))
        {
            return false;
        }

        AudioPlaybackConfig config = new(
            clip: entry.AudioClip,
            mixerGroup: audioRouter.GetMixerGroup(GetRoute(data.AudioCategory)),
            loop: data.Loop,
            spatial: data.Spatial,
            priority: data.Priority,
            spatialMinDistance: spatialMinDistance,
            spatialMaxDistance: spatialMaxDistance);

        instance.SetConfig(config);

        instance.SetVolume(entry.Volume * data.RandomVolume);
        instance.SetPitch(data.RandomPitch);

        instance.Play();

        if (data.AudioCategory != AudioCategory.Bgm)
        {
            playingAudio = new ActiveVoice
            {
                instance = instance,
                pool = pool,
                priority = data.Priority
            };
        }

        return true;
    }

    public void PlaySfx(AudioName audioName, int clipIndex)
    {
        IAudioInstance instance = pool.Get();
        if (!TryPlay(audioName, instance, clipIndex, out ActiveVoice playingAudio))
        {
            pool.Release(instance);
            return;
        }

        activeVoices.Add(playingAudio);
    }

    public void PlayAt(AudioName audioName, Vector3 position, int clipIndex)
    {
        IAudioInstance instance = pool.Get();
        instance.SetPosition(position);
        if (!TryPlay(audioName, instance, clipIndex, out ActiveVoice playingAudio))
        {
            pool.Release(instance);
            return;
        }

        activeVoices.Add(playingAudio);
    }

    public void PlayFollow(
        AudioName audioName,
        Transform target,
        int clipIndex)
    {
        IAudioInstance instance = pool.Get();
        instance.SetPosition(target.position);
        if (!TryPlay(audioName, instance, clipIndex, out ActiveVoice playingAudio))
        {
            pool.Release(instance);
            return;
        }

        playingAudio.follow = target;
        activeVoices.Add(playingAudio);
    }
    #endregion

    #region 오디오 일시정지
    public void PauseAll()
    {
        bgmInstance.Pause();
        foreach (ActiveVoice audio in activeVoices)
        {
            audio.instance.Pause();
        }
    }

    public void UnPauseAll()
    {
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
            audio.pool?.Release(audio.instance);
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
