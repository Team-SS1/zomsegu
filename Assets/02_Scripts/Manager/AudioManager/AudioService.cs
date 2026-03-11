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

    private IAudioInstance[] bgmInstances = new IAudioInstance[2];  // cross fade 용
    private IAudioSourcePool pool;                                  // SFX 풀
    private IAudioInstance curBgm => bgmInstances[CurIndex];

    // todo: 확정되면 상수로 빼기
    private float spatialMinDistance;
    private float spatialMaxDistance;

    private bool isPaused = false;

    // Fade in / out
    private float[] bgmVolumes = new float[2];
    private bool isFading = false;
    private float fadeElapsed = 0f;
    private float fadeDuration = 0f;
    private int fadeIndex = 0;
    private int CurIndex => fadeIndex % 2;
    private int PrevIndex => (fadeIndex + 1) % 2;
    #endregion

    #region 초기화
    public AudioService(AudioRepository repository, IAudioRouter audioRouter)
    {
        this.repository = repository;
        this.audioRouter = audioRouter;
    }

    public void InitializeRuntime(
        IAudioInstance bgmInstanceA,
        IAudioInstance bgmInstanceB,
        IAudioSourcePool pool)
    {
        bgmInstances[0] = bgmInstanceA;
        bgmInstances[1] = bgmInstanceB;
        this.pool = pool;
    }

    public void SetSpatialDistance(float min, float max)
    {
        spatialMinDistance = min;
        spatialMaxDistance = max;
    }
    #endregion

    public void Update()
    {
        // 정지
        if (isPaused) return;

        // todo: active voice 컴포넌트로 빼서 여기서 관리하게 하지 않기
        // 풀 정리 후 옮기기
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

        // bgm fading
        if (!isFading) return;

        fadeElapsed += Time.deltaTime;
        float t = fadeElapsed / fadeDuration;
        float outVol = Mathf.Lerp(bgmVolumes[PrevIndex], 0f, t);
        float inVol = Mathf.Lerp(0f, bgmVolumes[CurIndex], t);
        bgmInstances[PrevIndex]?.SetVolume(outVol);
        bgmInstances[CurIndex].SetVolume(inVol);

        if (t >= 1f || outVol == 0f)
        {
            bgmInstances[PrevIndex]?.SetVolume(0f);
            bgmInstances[PrevIndex]?.Stop();
            bgmInstances[CurIndex].SetVolume(bgmVolumes[CurIndex]);
            isFading = false;
        }
    }

    #region 오디오 재생
    public void PlayBgm(AudioName audioName, int clipIndex)
    {
        if (!repository.TryGetAudioData(audioName, out AudioData data)) return;

        if (data.AudioCategory != AudioCategory.Bgm) { return; }

        AudioVariation variation = data.GetVariation(clipIndex);

        if (!cooldown.CanPlay(audioName, data.Cooldown, Time.unscaledTime)) return;

        AudioPlaybackConfig config = new(
            clip: variation.AudioClip,
            mixerGroup: audioRouter.GetMixerGroup(GetRoute(AudioCategory.Bgm)),
            loop: data.Loop,
            spatial: false,
            priority: data.Priority);

        fadeIndex++;
        bgmVolumes[CurIndex] = variation.Volume * data.RandomVolume;

        curBgm.SetConfig(config);
        curBgm.SetVolume(0f);
        curBgm.SetPitch(data.RandomPitch);
        curBgm.Play();

        fadeDuration = 1f;
        fadeElapsed = 0f;
        isFading = true;

        if (!bgmInstances[PrevIndex].IsPlaying)
        {
            curBgm.SetVolume(bgmVolumes[CurIndex]);
            isFading = false;
        }
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

        curBgm?.Pause();
        foreach (ActiveVoice audio in activeVoices)
        {
            audio.instance.Pause();
        }
    }

    public void UnPauseAll()
    {
        if (!isPaused) return;
        isPaused = false;

        curBgm?.UnPause();
        foreach (ActiveVoice audio in activeVoices)
        {
            audio.instance.UnPause();
        }
    }
    #endregion

    #region 오디오 정지
    public void StopBgm()
    {
        curBgm?.Stop();
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
