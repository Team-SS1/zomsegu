using AudioEnum;
using UnityEngine;
using UnityEngine.Audio;

public readonly struct SfxPlayRequest
{
    ///<summary>AudioData variation index. -1이면 random variation을 사용한다.</summary>
    public int ClipIndex { get; }

    ///<summary>true면 CancellationToken으로 종료되는 loop SFX로 재생한다.</summary>
    public bool IsLoop { get; }

    ///<summary>3D SFX를 재생할 world position. 2D SFX에서는 사용하지 않는다.</summary>
    public Vector3 Position { get; }

    ///<summary>Loop follow SFX가 따라갈 Transform.</summary>
    public Transform Parent { get; }

    ///<summary>3D SFX의 spatial 거리와 rolloff 설정.</summary>
    public AudioSpatialSettings SpatialSettings { get; }

    private SfxPlayRequest(
        int clipIndex,
        bool isLoop,
        Vector3 position,
        Transform parent,
        AudioSpatialSettings spatialSettings)
    {
        ClipIndex = clipIndex;
        IsLoop = isLoop;
        Position = position;
        Parent = parent;
        SpatialSettings = spatialSettings;
    }

    ///<summary>true면 AudioSource를 3D spatial SFX로 설정한다.</summary>
    public bool UseSpatial => SpatialSettings.Enabled;

    ///<summary>true면 재생에 취소 가능한 CancellationToken이 필요하다.</summary>
    public bool NeedsCancelToken => IsLoop;

    ///<summary>true면 AudioData cooldown 검사를 적용한다.</summary>
    public bool UsesCooldown => !IsLoop;

    public static SfxPlayRequest OneShot(int clipIndex = -1)
    {
        return new SfxPlayRequest(clipIndex, isLoop: false, Vector3.zero, null, AudioSpatialSettings.None);
    }

    public static SfxPlayRequest At(Vector3 position, int clipIndex = -1)
    {
        return At(position, AudioSpatialSettings.Default3D, clipIndex);
    }

    public static SfxPlayRequest At(Vector3 position, AudioSpatialSettings spatialSettings, int clipIndex = -1)
    {
        return new SfxPlayRequest(clipIndex, isLoop: false, position, null, spatialSettings);
    }

    public static SfxPlayRequest Loop(int clipIndex = -1)
    {
        return new SfxPlayRequest(clipIndex, isLoop: true, Vector3.zero, null, AudioSpatialSettings.None);
    }

    public static SfxPlayRequest LoopAt(Vector3 position, int clipIndex = -1)
    {
        return LoopAt(position, AudioSpatialSettings.Default3D, clipIndex);
    }

    public static SfxPlayRequest LoopAt(Vector3 position, AudioSpatialSettings spatialSettings, int clipIndex = -1)
    {
        return new SfxPlayRequest(clipIndex, isLoop: true, position, null, spatialSettings);
    }

    public static SfxPlayRequest LoopFollow(Transform parent, int clipIndex = -1)
    {
        return LoopFollow(parent, AudioSpatialSettings.Default3D, clipIndex);
    }

    public static SfxPlayRequest LoopFollow(Transform parent, AudioSpatialSettings spatialSettings, int clipIndex = -1)
    {
        return new SfxPlayRequest(clipIndex, isLoop: true, Vector3.zero, parent, spatialSettings);
    }

    public void Play(AudioPoolObject po, AudioPriority priority)
    {
        if (Parent != null && IsLoop)
        {
            po.PlayLoop(priority, Parent);
            return;
        }

        po.Play(priority);
    }
}

public readonly struct AudioSpatialSettings
{
    ///<summary>true면 3D spatial SFX, false면 2D SFX로 재생한다.</summary>
    public bool Enabled { get; }

    ///<summary>true면 AudioManager의 전역 spatial 거리 대신 Min/MaxDistance를 사용한다.</summary>
    public bool OverrideDistance { get; }

    ///<summary>AudioSource.minDistance에 적용할 거리.</summary>
    public float MinDistance { get; }

    ///<summary>AudioSource.maxDistance에 적용할 거리.</summary>
    public float MaxDistance { get; }

    ///<summary>3D SFX 감쇠 방식.</summary>
    public AudioRolloffMode RolloffMode { get; }

    private AudioSpatialSettings(
        bool enabled,
        bool overrideDistance,
        float minDistance,
        float maxDistance,
        AudioRolloffMode rolloffMode)
    {
        Enabled = enabled;
        OverrideDistance = overrideDistance;
        MinDistance = minDistance;
        MaxDistance = maxDistance;
        RolloffMode = rolloffMode;
    }

    ///<summary>2D SFX용 spatial 비활성 설정.</summary>
    public static AudioSpatialSettings None => new(false, false, 0f, 0f, AudioRolloffMode.Logarithmic);

    ///<summary>AudioManager 전역 spatial 거리를 쓰는 기본 3D 설정.</summary>
    public static AudioSpatialSettings Default3D => new(true, false, 0f, 0f, AudioRolloffMode.Linear);

    public static AudioSpatialSettings Custom(
        float minDistance,
        float maxDistance,
        AudioRolloffMode rolloffMode = AudioRolloffMode.Linear)
    {
        float min = Mathf.Max(0f, minDistance);
        float max = Mathf.Max(min, maxDistance);
        return new AudioSpatialSettings(true, true, min, max, rolloffMode);
    }
}

public readonly struct AudioSourceConfig
{
    ///<summary>실제 재생할 AudioClip.</summary>
    public AudioClip Clip { get; }

    ///<summary>AudioData category에 따라 연결된 AudioMixerGroup.</summary>
    public AudioMixerGroup MixerGroup { get; }

    ///<summary>AudioSource.loop에 적용할 값.</summary>
    public bool IsLoop { get; }

    ///<summary>true면 3D spatial 설정을 AudioSource에 적용한다.</summary>
    public bool UseSpatial { get; }

    ///<summary>variation volume과 AudioData random volume을 반영한 최종 volume.</summary>
    public float Volume { get; }

    ///<summary>AudioData random pitch를 반영한 최종 pitch.</summary>
    public float Pitch { get; }

    ///<summary>spatial SFX의 최소 감쇠 거리.</summary>
    public float SpatialMinDistance { get; }

    ///<summary>spatial SFX의 최대 감쇠 거리.</summary>
    public float SpatialMaxDistance { get; }

    ///<summary>spatial SFX의 감쇠 방식.</summary>
    public AudioRolloffMode RolloffMode { get; }

    public AudioSourceConfig(
        AudioClip clip,
        AudioMixerGroup mixerGroup,
        bool isLoop,
        bool useSpatial,
        float volume,
        float pitch,
        float spatialMinDistance,
        float spatialMaxDistance,
        AudioRolloffMode rolloffMode)
    {
        Clip = clip;
        MixerGroup = mixerGroup;
        IsLoop = isLoop;
        UseSpatial = useSpatial;
        Volume = volume;
        Pitch = pitch;
        SpatialMinDistance = spatialMinDistance;
        SpatialMaxDistance = spatialMaxDistance;
        RolloffMode = rolloffMode;
    }
}
