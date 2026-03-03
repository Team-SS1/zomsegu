using UnityEngine;
using UnityEngine.UI;

public class Test_AudioSystem : MonoBehaviour
{
    [SerializeField] Button sfxButton;
    [SerializeField] Button bgmButton;
    [SerializeField] Button stopButton;

    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

    bool toggleBgm = false;

    private void Start()
    {
        AudioManager mg = AudioManager.Instance;

        sfxButton.onClick.AddListener(() => mg.PlaySfx2D(AudioEnum.AudioName.Test_Sfx));
        bgmButton.onClick.AddListener(() =>
        {
            toggleBgm = !toggleBgm;
            if (toggleBgm)
            {
                mg.PlayBgm(AudioEnum.AudioName.Test_Bgm);
            }
            else
            {
                mg.StopBgm();
            }
        });
        stopButton.onClick.AddListener(() => mg.StopAll());

        masterSlider.onValueChanged.AddListener(mg.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(mg.SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(mg.SetSfxVolume);
    }
}
