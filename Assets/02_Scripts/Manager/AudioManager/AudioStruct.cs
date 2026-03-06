using AudioEnum;

public readonly struct AudioRoute
{
    public readonly AudioMixerGroupType mixerGroupType;
    public readonly bool allowSpatial;

    public AudioRoute(AudioMixerGroupType mixerGroupType, bool allowSpatial)
    {
        this.mixerGroupType = mixerGroupType;
        this.allowSpatial = allowSpatial;
    }
}

public readonly struct AudioPlayOptions
{
    public readonly int clipIndex;
    public readonly bool loop;
    public readonly float pitch;

    public AudioPlayOptions(int clipIndex, bool loop, float pitch)
    {
        this.clipIndex = clipIndex;
        this.loop = loop;
        this.pitch = pitch;
    }
}