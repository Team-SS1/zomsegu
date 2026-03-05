using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Unity AudioSource를 감싼 재생 인스턴스
/// </summary>
public class AudioInstance : IAudioInstance
{
    private readonly AudioSource source;

    public AudioInstance(AudioSource source)
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

    public void SetClip(object o)
    {
        if (o is AudioClip clip)
        {
            source.clip = clip;
        }
    }

    public void SetLoop(bool loop) => source.loop = loop;
    public void SetPitch(float pitch) => source.pitch = pitch;
    public void SetVolume(float volume) => source.volume = volume;
    public void SetPosition(Vector3 position) => source.transform.position = position;

    public void SetOutputAudioMixerGroup(AudioMixerGroup audioMixerGroup)
    {
        source.outputAudioMixerGroup = audioMixerGroup;
    }

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
