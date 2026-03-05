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

    public void SetClip(object clip);
    public void SetLoop(bool loop);
    public void SetPitch(float pitch);
    public void SetVolume(float volume);
    public void SetPosition(UnityEngine.Vector3 position);
    public void SetOutputAudioMixerGroup(AudioMixerGroup audioMixerGroup);
    public void Set2D();
    public void Set3D(float minDistance, float maxDistance);
}