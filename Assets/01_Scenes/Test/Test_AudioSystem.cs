using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Test_AudioSystem : MonoBehaviour
{
    [SerializeField] bool deleteMove = true;

    [Header("오디오 믹서 Slider")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

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

        masterSlider.value = mg.GetVolume(AudioEnum.AudioMixerGroupType.Master);
        bgmSlider.value = mg.GetVolume(AudioEnum.AudioMixerGroupType.Bgm);
        sfxSlider.value = mg.GetVolume(AudioEnum.AudioMixerGroupType.Sfx);

        AddSliderListener();
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
    public void Example_PlayBgm()
    {
        mg.PlayBgm(
            AudioEnum.AudioName.Test_Bgm,   // AudioData SO 이름
            clipIndex: 0,                   // AudioData의 리스트 index, -1일 경우 random(기본값)
            loop: false,                    // 루프 기본값 BGM : true / SFX : false
            pitch: 1f                       // pitch
            );
    }

    public void Example_PlayTestSfx2D()
    {
        mg.PlaySfx(AudioEnum.AudioName.Test_Sfx);
    }

    public void Example_PlayTestSfx3D_Position()
    {
        // 특정 위치에서 작동하는 sfx
        mg.PlaySfx(AudioEnum.AudioName.Test_Sfx, Vector3.zero);
    }

    public void Example_PlayTestSfx3D_Transform()
    {
        // 특정 대상을 따라가는 sfx
        mg.PlaySfx(AudioEnum.AudioName.Test_Sfx, followTarget);
    }

    public void Example_Pause()
    {
        mg.PauseAll();
    }

    public void Example_UnPause()
    {
        mg.UnPauseAll();
    }

    public void Example_StopBgm()
    {
        mg.StopBgm();
    }

    public void Example_StopAll()
    {
        mg.StopAll();
    }
    #endregion

    #region Example - Audio Mixer
    private void AddSliderListener()
    {
        masterSlider.onValueChanged.AddListener(Example_OnMasterValueChanged);
        bgmSlider.onValueChanged.AddListener(Example_OnBgmValueChanged);
        sfxSlider.onValueChanged.AddListener(Example_OnSfxValueChanged);
    }

    private void Example_OnMasterValueChanged(float value)
    {
        mg.SetVolume(AudioEnum.AudioMixerGroupType.Master, value);
    }

    private void Example_OnBgmValueChanged(float value)
    {
        mg.SetVolume(AudioEnum.AudioMixerGroupType.Bgm, value);
    }

    private void Example_OnSfxValueChanged(float value)
    {
        mg.SetVolume(AudioEnum.AudioMixerGroupType.Sfx, value);
    }
    #endregion

    #region Example - Follow Movable 3D SFX
    [Header("설정 - 이동 가능 타겟 SFX")]
    [SerializeField] Transform followTarget;
    [SerializeField] float startPos = -10f;
    [SerializeField] float duration = 3f;
    [SerializeField] float repeatingTime = 1f;

    Tween tween;
    Coroutine coroutine;

    public void Example_Movable3DSFX()
    {
        tween?.Kill();
        followTarget.position = (Vector3)new Vector2(startPos, 0);
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(Repeat3DSfx());
        tween = followTarget.DOMoveX(-startPos, duration).OnComplete(() => StopCoroutine(coroutine));
    }

    private IEnumerator Repeat3DSfx()
    {
        while (true)
        {
            Example_PlayTestSfx3D_Transform();
            yield return new WaitForSeconds(repeatingTime);
        }
    }
    #endregion
}
