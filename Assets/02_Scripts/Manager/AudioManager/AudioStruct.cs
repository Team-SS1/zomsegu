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