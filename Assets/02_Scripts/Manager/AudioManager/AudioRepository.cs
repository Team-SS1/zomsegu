using AudioEnum;
using System;
using System.Collections.Generic;

/// <summary>
/// AudioDatabase 관리용
/// </summary>
public class AudioRepository
{
    private readonly Dictionary<AudioName, List<AudioEntry>> audioDict = new();
    private readonly IRandom random;

    public AudioRepository(AudioDatabase audioDatabase, IRandom random)
    {
        this.random = random ?? new UnityRandom();
        if (audioDatabase == null) throw new ArgumentNullException(nameof(audioDatabase));

        foreach (var audioData in audioDatabase.GetDatabase<AudioData>())
        {
            if (!Enum.TryParse(audioData.name, out AudioName audioName))
            {
                Logger.LogWarning($"{audioData.name} 이름 오류");
                continue;
            }

            audioDict[audioName] = audioData.AudioEntries;
        }
    }

    public bool TryGetAudioEntry(AudioName audioName, int clipIndex, out AudioEntry entry)
    {
        entry = null;

        if (!audioDict.TryGetValue(audioName, out List<AudioEntry> entries) || entries == null || entries.Count == 0)
        {
            Logger.LogWarning($"{audioName} 오디오 데이터 없음");
            return false;
        }

        if (clipIndex >= entries.Count || clipIndex < 0)
        {
            clipIndex = random.Range(0, entries.Count);
        }

        entry = entries[clipIndex];
        return true;
    }
}
