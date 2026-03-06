using AudioEnum;
using System.Collections.Generic;
using UnityEngine;

public static class AudioCooldown
{
    private static readonly Dictionary<AudioName, float> lastPlayTime = new();

    public static bool CanPlay(AudioName name, float interval)
    {
        float now = Time.unscaledTime;

        if (lastPlayTime.TryGetValue(name, out float last) &&
            now - last < interval)
            return false;

        lastPlayTime[name] = now;
        return true;
    }
}
