using AudioEnum;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public sealed class SfxManager
{
    /// <summary>
    /// 오디오 중첩 방지를 위한 쿨다운 관리 클래스
    /// </summary>
    private class AudioCooldown
    {
        private readonly Dictionary<AudioName, float> lastPlayTime = new();

        public bool CanPlay(AudioName name, float interval, float now)
        {
            if (lastPlayTime.TryGetValue(name, out float last) &&
                now - last < interval)
                return false;

            lastPlayTime[name] = now;
            return true;
        }
    }

    private readonly AudioRepository repository;
    private readonly AudioMixerRouter audioRouter;
    private readonly AudioSourcePool pool;
    private readonly AudioCooldown cooldown = new();

    private float spatialMinDistance;
    private float spatialMaxDistance;

    public SfxManager(AudioRepository repository, AudioMixerRouter audioRouter, AudioSourcePool pool)
    {
        this.repository = repository;
        this.audioRouter = audioRouter;
        this.pool = pool;
    }

    public void SetSpatialDistance(float min, float max)
    {
        spatialMinDistance = Mathf.Max(0f, min);
        spatialMaxDistance = Mathf.Max(spatialMinDistance, max);
        pool.SetSpatialDistance(spatialMinDistance, spatialMaxDistance);
    }

    public void Play(AudioName audioName, SfxPlayRequest request)
    {
        PlayInternalAsync(audioName, request, default).Forget();
    }

    public UniTask PlayAsync(AudioName audioName, SfxPlayRequest request, CancellationToken ct)
    {
        return PlayInternalAsync(audioName, request, ct);
    }

    private async UniTask PlayInternalAsync(AudioName audioName, SfxPlayRequest request, CancellationToken ct)
    {
        if (!TryGetPlayback(audioName, request, ct, out AudioPoolObject po, out AudioData data, out AudioVariation variation)) return;

        AudioSourceConfig config = CreateSourceConfig(data, variation, request);
        ConfigureSource(po.Source, config, request.Position);

        request.Play(po, data.Priority);

        await WaitAsync(po, config.IsLoop, ct);
    }

    private bool TryGetPlayback(
        AudioName audioName,
        SfxPlayRequest request,
        CancellationToken ct,
        out AudioPoolObject po,
        out AudioData data,
        out AudioVariation variation)
    {
        po = null;
        data = null;
        variation = null;

        if (ct.IsCancellationRequested) return false;
        if (request.NeedsCancelToken && !ct.CanBeCanceled) return false;
        if (!repository.TryGetAudioData(audioName, out data)) return false;
        if (data.AudioCategory == AudioCategory.Bgm) return false;

        variation = data.GetVariation(request.ClipIndex);
        if (variation?.AudioClip == null) return false;

        if (request.UsesCooldown && !cooldown.CanPlay(audioName, data.Cooldown, Time.unscaledTime)) return false;

        po = pool.Get();
        return true;
    }

    private AudioSourceConfig CreateSourceConfig(AudioData data, AudioVariation variation, SfxPlayRequest request)
    {
        return new AudioSourceConfig(
            variation.AudioClip,
            audioRouter.GetMixerGroup(data.AudioCategory),
            request.IsLoop,
            request.UseSpatial,
            variation.Volume * data.RandomVolume,
            data.RandomPitch,
            GetSpatialMinDistance(request.SpatialSettings),
            GetSpatialMaxDistance(request.SpatialSettings),
            request.SpatialSettings.RolloffMode);
    }

    private void ConfigureSource(
        AudioSource source,
        AudioSourceConfig config,
        Vector3 position)
    {
        source.clip = config.Clip;
        source.outputAudioMixerGroup = config.MixerGroup;
        source.loop = config.IsLoop;
        source.volume = config.Volume;
        source.pitch = config.Pitch;
        source.transform.position = position;

        if (!config.UseSpatial)
        {
            source.spatialBlend = 0f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            return;
        }

        source.spatialBlend = 1f;
        source.minDistance = config.SpatialMinDistance;
        source.maxDistance = config.SpatialMaxDistance;
        source.rolloffMode = config.RolloffMode;
    }

    private float GetSpatialMinDistance(AudioSpatialSettings settings)
    {
        return settings.OverrideDistance ? settings.MinDistance : spatialMinDistance;
    }

    private float GetSpatialMaxDistance(AudioSpatialSettings settings)
    {
        return settings.OverrideDistance ? settings.MaxDistance : spatialMaxDistance;
    }

    private async UniTask WaitAsync(AudioPoolObject po, bool isLoop, CancellationToken ct)
    {
        int playId = po.PlayId;

        try
        {
            if (isLoop)
            {
                await UniTask.WaitUntilCanceled(ct);
                return;
            }

            float time = po.GetDuration();
            while (time > 0f)
            {
                ct.ThrowIfCancellationRequested();
                if (!po.IsPaused) time -= Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (po.PlayId == playId)
            {
                pool.Release(po);
            }
        }
    }

    public void Pause()
    {
        pool.Pause();
    }

    public void Resume()
    {
        pool.UnPause();
    }

    public void StopAll()
    {
        pool.ReleaseAll();
    }
}
