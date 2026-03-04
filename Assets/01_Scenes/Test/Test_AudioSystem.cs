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

    [SerializeField] Transform followTarget;

    bool toggleBgm = false;

    AudioManager mg;

    #region Unity API
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

        masterSlider.onValueChanged.AddListener(OnMasterValueChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmValueChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxValueChanged);
    }

    private void OnDestroy()
    {
        if (deleteMove)
        {
            PlayerPrefs.DeleteAll();
        }
    }
    #endregion

    #region Example - Play Bgm/Sfx
    private void PlayBgm()
    {
        mg.PlayBgm(
            AudioEnum.AudioName.Test_Bgm,   // AudioData SO 이름
            clipIndex: 0,                   // AudioData의 리스트 index, -1일 경우 random(기본값)
            loop: false,                    // 루프 기본값 BGM : true / SFX : false
            pitch: 1f                       // pitch
            );
    }

    private void PlayTestSfx2D()
    {
        mg.PlaySfx2D(AudioEnum.AudioName.Test_Sfx);
    }

    private void PlayTestSfx3D_Position()
    {
        // 특정 위치에서 작동하는 sfx
        mg.PlaySfx3D(AudioEnum.AudioName.Test_Sfx, Vector3.zero);
    }

    private void PlayTestSfx3D_Transform()
    {
        // 특정 대상을 따라가는 sfx
        mg.PlaySfx3D(AudioEnum.AudioName.Test_Sfx, followTarget);
    }
    #endregion

    #region Example - Audio Mixer
    private void OnMasterValueChanged(float value)
    {
        mg.SetVolume(AudioEnum.AudioMixerGroupType.Master, value);
    }

    private void OnBgmValueChanged(float value)
    {
        mg.SetVolume(AudioEnum.AudioMixerGroupType.Bgm, value);
    }

    private void OnSfxValueChanged(float value)
    {
        mg.SetVolume(AudioEnum.AudioMixerGroupType.Sfx, value);
    }
    #endregion
}
