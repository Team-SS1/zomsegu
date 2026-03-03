public interface IAudioSourcePool
{
    public IAudioHandle Get();
    public void Release(IAudioHandle audioSource);
    public void ReleaseAll();
}

public interface IAudioHandle
{
    public bool IsPlaying { get; }

    public void Play();
    public void Stop();

    public void SetClip(object clip);
    public void SetLoop(bool loop);
    public void SetPitch(float pitch);
    public void SetVolume(float volume);
    public void SetPosition(UnityEngine.Vector3 position);
    public void Set2D();
    public void Set3D(float minDistance, float maxDistance);
}