using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerController
{
    private const string MASTER = "Master";
    private const string BGM = "Bgm";
    private const string SFX = "Sfx";

    private readonly AudioMixer mixer;

    private readonly AudioMixerGroup masterGroup;
    private readonly AudioMixerGroup bgmGroup;
    private readonly AudioMixerGroup sFXGroup;

    public AudioMixerGroup MasterGroup => masterGroup;
    public AudioMixerGroup BgmGroup => bgmGroup;
    public AudioMixerGroup SfxGroup => sFXGroup;

    public AudioMixerController(AudioMixer mixer)
    {
        if (mixer == null) throw new ArgumentNullException(nameof(mixer));

        this.mixer = mixer;

        masterGroup = mixer.FindMatchingGroups(MASTER)[0];
        bgmGroup = mixer.FindMatchingGroups(BGM)[0];
        sFXGroup = mixer.FindMatchingGroups(SFX)[0];
    }

    public void SetMaster(float normalized) => SetVolume(MASTER, normalized);
    public void SetBgm(float normalized) => SetVolume(BGM, normalized);
    public void SetSfx(float normalized) => SetVolume(SFX, normalized);

    private void SetVolume(string param, float normalized)
    {
        float volume = Mathf.Log10(Mathf.Clamp(normalized, 0.0001f, 1f)) * 20f; // db로 변환
        mixer.SetFloat(param, volume);
    }

    public float GetVolume01(string param)
    {
        if (!mixer.GetFloat(param, out float volume)) return 1f;
        return Mathf.Pow(10f, volume / 20f);
    }
}
