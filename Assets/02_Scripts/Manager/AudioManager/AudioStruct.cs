using AudioEnum;
using UnityEngine;
using UnityEngine.Audio;

public readonly struct AudioPlayOptions
{
    public readonly int clipIndex;
    public readonly bool loop;
    public readonly float pitch;
    public readonly bool useSpatial;

    public AudioPlayOptions(int clipIndex, bool loop, float pitch, bool useSpatial = false)
    {
        this.clipIndex = clipIndex;
        this.loop = loop;
        this.pitch = pitch;
        this.useSpatial = useSpatial;
    }
}

public readonly struct AudioPlaybackConfig
{
    public readonly AudioClip clip;
    public readonly AudioMixerGroup mixerGroup;
    public readonly bool loop;
    public readonly bool spatial;
    public readonly AudioPriority priority;

    // todo: 확정되면 const로 빼기
    public readonly float spatialMinDistance;
    public readonly float spatialMaxDistance;

    public AudioPlaybackConfig(
        AudioClip clip,
        AudioMixerGroup mixerGroup,
        bool loop,
        bool spatial,
        AudioPriority priority,
        float spatialMinDistance = 0f,
        float spatialMaxDistance = 0f)
    {
        this.clip = clip;
        this.mixerGroup = mixerGroup;
        this.loop = loop;
        this.spatial = spatial;
        this.priority = priority;
        this.spatialMinDistance = spatialMinDistance;
        this.spatialMaxDistance = spatialMaxDistance;
    }
}