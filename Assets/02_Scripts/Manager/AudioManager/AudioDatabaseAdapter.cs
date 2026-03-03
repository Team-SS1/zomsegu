using AudioEnum;
using System;
using System.Collections.Generic;

/// <summary>
/// AudioDatabase 관리 및 조회용 어댑터
/// </summary>
public class AudioDatabaseAdapter
{
    private readonly Dictionary<AudioName, List<AudioEntry>> bgmDict = new();
    private readonly Dictionary<AudioName, List<AudioEntry>> sfxDict = new();
    private readonly IRandom random;

    public AudioDatabaseAdapter(AudioDatabase audioDatabase, IRandom random)
    {
        this.random = random ?? new UnityRandom();
        if (audioDatabase == null) throw new ArgumentNullException("AudioDatabase 없음");

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
            idx = random.Range(0, entries.Count);
        }

        entry = entries[idx];
        return true;
    }
}
