using AudioEnum;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class BgmManager
{
    private readonly AudioRepository repository;
    private readonly AudioMixerRouter audioRouter;
    private readonly AudioSource[] bgmSources = new AudioSource[2];
    private readonly float[] bgmVolumes = new float[2];

    private bool isPaused;
    private int fadeIndex;

    private int CurIndex => fadeIndex % 2;
    private int PrevIndex => (fadeIndex + 1) % 2;

    public BgmManager(AudioRepository repository, AudioMixerRouter audioRouter, AudioSource bgmSourceA, AudioSource bgmSourceB)
    {
        this.repository = repository;
        this.audioRouter = audioRouter;
        bgmSources[0] = bgmSourceA;
        bgmSources[1] = bgmSourceB;
    }

    public void Play(AudioName audioName, int clipIndex, float fadeDuration)
    {
        if (!repository.TryGetAudioData(audioName, out AudioData data)) return;
        if (data.AudioCategory != AudioCategory.Bgm) return;

        AudioVariation variation = data.GetVariation(clipIndex);
        if (variation?.AudioClip == null) return;

        fadeIndex++;
        float curTargetVolume = variation.Volume * data.RandomVolume;

        AudioSource curSource = bgmSources[CurIndex];
        AudioSource prevSource = bgmSources[PrevIndex];

        curSource.clip = variation.AudioClip;
        curSource.outputAudioMixerGroup = audioRouter.GetMixerGroup(data.AudioCategory);
        curSource.loop = true;
        curSource.spatialBlend = 0f;
        curSource.volume = 0f;
        bgmVolumes[CurIndex] = 0f;
        curSource.pitch = data.RandomPitch;
        curSource.Play();

        if (!prevSource.isPlaying || fadeDuration <= 0f)
        {
            StopSource(prevSource);
            curSource.volume = curTargetVolume;
            bgmVolumes[CurIndex] = curTargetVolume;
            return;
        }

        FadeAsync(fadeDuration, fadeIndex, CurIndex, PrevIndex, curTargetVolume).Forget();
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        bgmSources[0].Pause();
        bgmSources[1].Pause();
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        bgmSources[0].UnPause();
        bgmSources[1].UnPause();
    }

    public void Stop()
    {
        fadeIndex++;
        StopSource(bgmSources[0]);
        StopSource(bgmSources[1]);
    }

    private async UniTask FadeAsync(float duration, int fadeId, int curIndex, int prevIndex, float curTargetVolume)
    {
        float elapsed = 0f;
        float prevStartVolume = bgmVolumes[prevIndex];

        while (fadeId == fadeIndex && elapsed < duration)
        {
            if (isPaused)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                continue;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.0001f));
            float prevVolume = Mathf.Lerp(prevStartVolume, 0f, t);
            float curVolume = Mathf.Lerp(0f, curTargetVolume, t);

            bgmSources[prevIndex].volume = prevVolume;
            bgmVolumes[prevIndex] = prevVolume;
            bgmSources[curIndex].volume = curVolume;
            bgmVolumes[curIndex] = curVolume;

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        if (fadeId != fadeIndex) return;

        bgmVolumes[prevIndex] = 0f;
        StopSource(bgmSources[prevIndex]);
        bgmSources[curIndex].volume = curTargetVolume;
        bgmVolumes[curIndex] = curTargetVolume;
    }

    private static void StopSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
    }
}
