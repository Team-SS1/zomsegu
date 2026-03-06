using AudioEnum;
using UnityEngine;
using UnityEngine.Audio;

public interface IAudioSourcePool
{
    public IAudioInstance Get();
    public void Release(IAudioInstance audioSource);
    public void ReleaseAll();
}

public interface IAudioInstance
{
    public bool IsPlaying { get; }

    public void Play();
    public void Pause();
    public void UnPause();
    public void Stop();

    public void SetClip(AudioClip clip);
    public void SetLoop(bool loop);
    public void SetPitch(float pitch);
    public void SetVolume(float volume);
    public void SetPosition(Vector3 position);
    public void SetOutputAudioMixerGroup(AudioMixerGroup audioMixerGroup);
    public void SetPriority(AudioPriority priority);

    public void Set2D();
    public void Set3D(float minDistance, float maxDistance);
}

public interface IAudioRouter
{
    public AudioMixerGroup GetMixerGroup(AudioMixerGroupType type);
    public void SetVolume(AudioMixerGroupType type, float normalized);
    public float GetVolume01(AudioMixerGroupType type);
}