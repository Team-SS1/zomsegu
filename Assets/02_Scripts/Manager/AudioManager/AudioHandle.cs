using UnityEngine;

/// <summary>
/// Unity AudioSource를 감싼 재생 핸들
/// </summary>
public class AudioHandle : IAudioHandle
{
    private readonly AudioSource source;

    public AudioHandle(AudioSource source)
    {
        this.source = source;
    }

    public bool IsPlaying => source.isPlaying;

    public void Play()
    {
        if (!source.gameObject.activeSelf)
        {
            source.gameObject.SetActive(true);
        }
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
        source.gameObject.SetActive(false);
    }

    public void SetClip(object clip) => source.clip = (AudioClip)clip;
    public void SetLoop(bool loop) => source.loop = loop;
    public void SetPitch(float pitch) => source.pitch = pitch;
    public void SetVolume(float volume) => source.volume = volume;
    public void SetPosition(Vector3 position) => source.transform.position = position;

    public void Set2D()
    {
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    public void Set3D(float minDistance, float maxDistance)
    {
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
    }
}
