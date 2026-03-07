using AudioEnum;
using System.Collections.Generic;

public class AudioCooldown
{
    private readonly Dictionary<AudioName, float> lastPlayTime = new();

    public bool CanPlay(AudioName name, float interval, float now)
    {
        if (lastPlayTime.TryGetValue(name, out float last) &&
            now - last < interval)
            return false;

        lastPlayTime[name] = now;
        return true;
    }
}
