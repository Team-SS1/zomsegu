using AudioEnum;
using UnityEngine;
using UnityEngine.Audio;

public interface IAudioSourcePool
{
    public AudioPoolObject Get();
    public void Release(AudioPoolObject po);
    public void ReleaseAll();
    public void Pause();
    public void UnPause();
}

public interface IAudioInstance
{
    public bool IsPlaying { get; }
    public bool IsPaused { get; }

    public void Play();
    public void Pause();
    public void UnPause();
    public void Stop();

    public void SetConfig(in AudioPlaybackConfig config);
    public void SetVolume(float volume);
    public void SetPitch(float pitch);
    public void SetPosition(Vector3 position);
}

public interface IAudioRouter
{
    public AudioMixerGroup GetMixerGroup(AudioMixerGroupType type);
    public void SetVolume(AudioMixerGroupType type, float normalized);
    public float GetVolume01(AudioMixerGroupType type);
}