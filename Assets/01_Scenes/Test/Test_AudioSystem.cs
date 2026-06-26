using AudioEnum;
using DG.Tweening;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Test_AudioSystem : MonoBehaviour
{
    [SerializeField] bool deleteMove = true;

    [Header("오디오 믹서 Slider")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

    [Header("Cooldown")]
    [SerializeField] bool lockUpdate = true;

    [Header("SFX Request 예제")]
    [SerializeField] Transform sfxPoint;
    [SerializeField] Transform followTarget;
    [SerializeField] float customMinDistance = 2f;
    [SerializeField] float customMaxDistance = 12f;

    AudioManager mg;
    CancellationTokenSource loopSfxCts;

    #region Unity API
    private void OnEnable()
    {
        if (mg == null) return;

        masterSlider.SetValueWithoutNotify(mg.GetVolume(AudioMixerGroupType.Master));
        bgmSlider.SetValueWithoutNotify(mg.GetVolume(AudioMixerGroupType.Bgm));
        sfxSlider.SetValueWithoutNotify(mg.GetVolume(AudioMixerGroupType.Sfx));
    }

    private void Start()
    {
        mg = AudioManager.Instance;

        masterSlider.value = mg.GetVolume(AudioMixerGroupType.Master);
        bgmSlider.value = mg.GetVolume(AudioMixerGroupType.Bgm);
        sfxSlider.value = mg.GetVolume(AudioMixerGroupType.Sfx);

        AddSliderListener();
    }

    private void Update()
    {
        if (!lockUpdate)
        {
            Example_PlayTestSfx2D();
        }
    }

    private void OnDestroy()
    {
        Example_StopLoopSfxByCancellationToken();

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
            AudioName.Test_Bgm, // AudioData SO 이름
            clipIndex: 0        // AudioData의 variation index. -1이면 random.
            );
    }

    public void Example_PlayBgm_Fade()
    {
        mg.PlayBgm(AudioName.Test_Bgm, fadeDuration: 1f);
    }

    public void Example_PlayTestSfx2D()
    {
        mg.PlaySfx(AudioName.Test_Sfx);
    }

    public void Example_PlayTestSfx2D_ClipIndex()
    {
        mg.PlaySfx(AudioName.Test_Sfx, clipIndex: 0);
    }

    public void Example_PlayTestSfx3D_Position()
    {
        mg.PlaySfxAt(AudioName.Test_Sfx, GetSfxPosition());
    }

    public void Example_PlayTestSfx3D_Transform()
    {
        mg.PlaySfxFollow(AudioName.Test_Sfx, GetFollowTarget());
    }

    public void Example_PlayTestSfx3D_Transform(Transform target)
    {
        mg.PlaySfxFollow(AudioName.Test_Sfx, target);
    }

    public void Example_PlayTestSfx3D_CustomDistance()
    {
        SfxPlayRequest request = SfxPlayRequest.At(
            GetSfxPosition(),
            AudioSpatialSettings.Custom(customMinDistance, customMaxDistance));

        mg.PlaySfx(AudioName.Test_Sfx, request);
    }

    public void Example_PlayLoopSfxByCancellationToken()
    {
        PlayLoop(SfxPlayRequest.Loop());
    }

    public void Example_PlayLoopSfx3D_Position()
    {
        PlayLoop(SfxPlayRequest.LoopAt(GetSfxPosition()));
    }

    public void Example_PlayLoopSfx3D_Follow()
    {
        PlayLoop(SfxPlayRequest.LoopFollow(GetFollowTarget()));
    }

    public void Example_PlayLoopSfx3D_CustomDistance()
    {
        SfxPlayRequest request = SfxPlayRequest.LoopAt(
            GetSfxPosition(),
            AudioSpatialSettings.Custom(customMinDistance, customMaxDistance));

        PlayLoop(request);
    }

    public void Example_StopLoopSfxByCancellationToken()
    {
        StopLoopSfx(loopSfxCts);
        loopSfxCts = null;
    }

    private void PlayLoop(SfxPlayRequest request)
    {
        StopLoopSfx(loopSfxCts);
        loopSfxCts = new CancellationTokenSource();

        // Loop SFX는 token이 취소될 때까지 재생된다.
        _ = mg.PlaySfxAsync(AudioName.Test_Sfx, request, loopSfxCts.Token);
    }

    private void StopLoopSfx(CancellationTokenSource cts)
    {
        if (cts == null) return;

        cts.Cancel();
        cts.Dispose();
    }

    private Vector3 GetSfxPosition()
    {
        return sfxPoint != null ? sfxPoint.position : transform.position;
    }

    private Transform GetFollowTarget()
    {
        return followTarget != null ? followTarget : transform;
    }

    public void Example_Pause()
    {
        mg.PauseAll();
    }

    public void Example_UnPause()
    {
        mg.ResumeAll();
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
        mg.SetVolume(AudioMixerGroupType.Master, value);
    }

    private void Example_OnBgmValueChanged(float value)
    {
        mg.SetVolume(AudioMixerGroupType.Bgm, value);
    }

    private void Example_OnSfxValueChanged(float value)
    {
        mg.SetVolume(AudioMixerGroupType.Sfx, value);
    }
    #endregion

    #region Example - Follow Movable 3D SFX
    [Header("설정 - 이동 가능 타겟 SFX")]
    [SerializeField] GameObject prefab;
    [SerializeField] float startPos = -10f;
    [SerializeField] float duration = 3f;
    [SerializeField] float repeatingTime = 1f;

    public void Example_Movable3DSFX()
    {
        Transform target = Instantiate(prefab).transform;
        target.position = (Vector3)new Vector2(startPos, 0);
        Coroutine coroutine = StartCoroutine(Repeat3DSfx(target));
        target.DOMoveX(-startPos, duration).OnComplete(() =>
        {
            StopCoroutine(coroutine);
            Destroy(target.gameObject);
        });
    }

    private IEnumerator Repeat3DSfx(Transform target)
    {
        while (true)
        {
            mg.PlaySfxFollow(AudioName.Test_Sfx, target);
            yield return new WaitForSeconds(repeatingTime);
        }
    }
    #endregion
}
