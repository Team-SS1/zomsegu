using AudioEnum;
using UnityEngine;

public class ActiveVoice
{
    public IAudioInstance instance;
    public IAudioSourcePool pool;
    public Transform follow;

    public AudioPriority priority;
}