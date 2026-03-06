using AudioEnum;
using System;
using System.Collections.Generic;

/// <summary>
/// AudioDatabase 관리용
/// </summary>
public class AudioRepository
{
    private readonly Dictionary<AudioName, AudioData> audioDict = new();

    public AudioRepository(AudioDatabase audioDatabase)
    {
        if (audioDatabase == null) throw new ArgumentNullException(nameof(audioDatabase));

        foreach (var audioData in audioDatabase.GetDatabase<AudioData>())
        {
            if (!Enum.TryParse(audioData.name, out AudioName audioName))
            {
                Logger.LogWarning($"{audioData.name} 이름 오류");
                continue;
            }

            audioDict[audioName] = audioData;
        }
    }

    public bool TryGetAudioData(AudioName audioName, out AudioData data)
    {
        if (!audioDict.TryGetValue(audioName, out data) || data == null || data.AudioEntries.Count == 0)
        {
            Logger.LogWarning($"{audioName} 오디오 데이터 없음");
            return false;
        }

        return true;
    }
}
