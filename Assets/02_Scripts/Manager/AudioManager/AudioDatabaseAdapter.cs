using AudioEnum;
using System;
using System.Collections.Generic;

public class AudioDatabaseAdapter
{
    private readonly Dictionary<AudioName, List<AudioEntry>> bgmDict = new();
    private readonly Dictionary<AudioName, List<AudioEntry>> sfxDict = new();

    public AudioDatabaseAdapter(AudioDatabase audioDatabase)
    {
        foreach (var audioData in audioDatabase.GetDatabase<AudioData>())
        {
            if (!Enum.TryParse(audioData.name, out AudioName audioName))
            {
                Logger.LogWarning($"{audioData.name} 이름 오류");
                continue;
            }

            switch (audioData.AudioCategory)
            {
                case AudioCategory.Bgm:
                    bgmDict[audioName] = audioData.AudioEntries;
                    break;
                case AudioCategory.Sfx:
                    sfxDict[audioName] = audioData.AudioEntries;
                    break;
            }
        }
    }

    public bool TryGetAudioEntry(AudioCategory audioCategory, AudioName audioName, int idx, out AudioEntry entry)
    {
        entry = null;

        Dictionary<AudioName, List<AudioEntry>> dict = (audioCategory == AudioCategory.Bgm) ? bgmDict : sfxDict;
        if (!dict.TryGetValue(audioName, out List<AudioEntry> entries) || entries == null || entries.Count == 0)
        {
            Logger.LogWarning($"{audioName} 오디오 데이터 없음");
            return false;
        }

        if (idx >= entries.Count || idx < 0)
        {
            idx = UnityEngine.Random.Range(0, entries.Count);
        }

        entry = entries[idx];
        return true;
    }
}
