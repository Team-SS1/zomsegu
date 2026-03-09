using AudioEnum;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerRouter : IAudioRouter
{
    private readonly AudioMixer mixer;
    private readonly Dictionary<AudioMixerGroupType, AudioMixerGroup> mixerGroups = new();

    public AudioMixerRouter(AudioMixer mixer)
    {
        if (mixer == null) throw new ArgumentNullException(nameof(mixer));

        this.mixer = mixer;

        foreach (AudioMixerGroupType mixerGroup in Enum.GetValues(typeof(AudioMixerGroupType)))
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups(mixerGroup.ToString());
            if (groups.Length == 0) throw new Exception($"MixerGroup 없음: {mixerGroup}");
            mixerGroups[mixerGroup] = groups[0];
        }
    }

    public AudioMixerGroup GetMixerGroup(AudioMixerGroupType type) => mixerGroups[type];

    public void SetVolume(AudioMixerGroupType type, float normalized)
    {
        string param = type.ToString();
        float db = Mathf.Log10(Mathf.Clamp(normalized, 0.0001f, 1f)) * 20f; // db로 변환
        mixer.SetFloat(param, db);
    }

    public float GetVolume01(AudioMixerGroupType type)
    {
        string param = type.ToString();

        if (!mixer.GetFloat(param, out float db))
        {
            Logger.LogWarning($"파라미터 못찾음 {param}");
            return 1f;
        }

        // -80 이하일 경우 0으로 간주
        if (db <= -80f) return 0f;

        return Mathf.Pow(10f, db / 20f);
    }
}
