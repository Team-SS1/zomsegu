using UnityEngine;
using UnityEngine.UI;

public class Test_AudioSystem : MonoBehaviour
{
    [SerializeField] bool deleteMove = true;

    [SerializeField] Button sfxButton;
    [SerializeField] Button bgmButton;
    [SerializeField] Button stopButton;

    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

    bool toggleBgm = false;

    AudioManager mg;

    private void OnEnable()
    {
        if (mg == null) return;

        masterSlider.SetValueWithoutNotify(mg.GetVolume(AudioEnum.AudioMixerGroupType.Master));
        bgmSlider.SetValueWithoutNotify(mg.GetVolume(AudioEnum.AudioMixerGroupType.Bgm));
        sfxSlider.SetValueWithoutNotify(mg.GetVolume(AudioEnum.AudioMixerGroupType.Sfx));
    }

    private void Start()
    {
        mg = AudioManager.Instance;

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

        masterSlider.value = mg.GetVolume(AudioEnum.AudioMixerGroupType.Master);
        bgmSlider.value = mg.GetVolume(AudioEnum.AudioMixerGroupType.Bgm);
        sfxSlider.value = mg.GetVolume(AudioEnum.AudioMixerGroupType.Sfx);

        masterSlider.onValueChanged.AddListener((float value)
            => mg.SetVolume(AudioEnum.AudioMixerGroupType.Master, value));
        bgmSlider.onValueChanged.AddListener((float value)
            => mg.SetVolume(AudioEnum.AudioMixerGroupType.Bgm, value));
        sfxSlider.onValueChanged.AddListener((float value)
            => mg.SetVolume(AudioEnum.AudioMixerGroupType.Sfx, value));
    }

    private void OnDestroy()
    {
        if (deleteMove)
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
