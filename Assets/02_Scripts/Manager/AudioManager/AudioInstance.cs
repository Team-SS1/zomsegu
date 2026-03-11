using UnityEngine;

/// <summary>
/// Unity AudioSource를 감싼 재생 인스턴스
/// </summary>
public class AudioInstance : IAudioInstance
{
    private readonly AudioSource source;
    private bool isPaused;

    public AudioInstance(AudioSource source)
    {
        this.source = source;
    }

    public bool IsPlaying => source.isPlaying;
    public bool IsPaused => isPaused;

    #region AudioSource 재생 관리
    public void Play()
    {
        if (!source.gameObject.activeSelf)
        {
            source.gameObject.SetActive(true);
        }
        source.Play();
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        if (source.isPlaying)
        {
            source.Pause();
        }
    }

    public void UnPause()
    {
        if (!isPaused) return;
        isPaused = false;

        if (!source.isPlaying)
        {
            source.UnPause();
        }
    }

    public void Stop()
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
    }
    #endregion

    #region AudioSoucre 설정
    public void SetPitch(float pitch) => source.pitch = pitch;
    public void SetVolume(float volume) => source.volume = volume;
    public void SetPosition(Vector3 position) => source.transform.position = position;

    public void SetConfig(in AudioPlaybackConfig config)
    {
        source.clip = config.clip;
        source.outputAudioMixerGroup = config.mixerGroup;
        source.loop = config.loop;

        if (config.spatial)
        {
            Set3D(config.spatialMinDistance, config.spatialMaxDistance);
        }
        else
        {
            Set2D();
        }
    }

    private void Set2D()
    {
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void Set3D(float minDistance, float maxDistance)
    {
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
    }
    #endregion
}
