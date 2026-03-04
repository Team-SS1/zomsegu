using AudioEnum;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerController
{
    private readonly AudioMixer mixer;
    private readonly Dictionary<AudioMixerGroupType, AudioMixerGroup> mixerGroups = new();

    public AudioMixerGroup BgmGroup => mixerGroups[AudioMixerGroupType.Bgm];
    public AudioMixerGroup SfxGroup => mixerGroups[AudioMixerGroupType.Sfx];

    public AudioMixerController(AudioMixer mixer)
    {
        if (mixer == null) throw new ArgumentNullException(nameof(mixer));

        this.mixer = mixer;

        foreach (AudioMixerGroupType mixerGroup in Enum.GetValues(typeof(AudioMixerGroupType)))
        {
            mixerGroups[mixerGroup] = mixer.FindMatchingGroups(mixerGroup.ToString())[0];
            SetVolume(mixerGroup, PlayerPrefs.GetFloat(mixerGroup.ToString(), GameConstants.DefaultVolume));
        }
    }

    public void SetVolume(AudioMixerGroupType type, float normalized)
    {
        string param = type.ToString();
        float volume = Mathf.Log10(Mathf.Clamp(normalized, 0.0001f, 1f)) * 20f; // db로 변환
        mixer.SetFloat(param, volume);
        SaveVolume(param, volume);
    }

    public float GetVolume01(AudioMixerGroupType type)
    {
        string param = type.ToString();
        if (!mixer.GetFloat(param, out float volume)) return 1f;
        return Mathf.Pow(10f, volume / 20f);
    }

    private void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
}
