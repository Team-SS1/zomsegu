using System.Collections;
using TMPro;
using UIEnum;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UICircleConditionGauge : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image bgNormal;
    [SerializeField] private Image bgAlert;
    [SerializeField] private Image fillNormal;
    [SerializeField] private Image fillAlert;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI valueText;

    [Header("Color")]
    [SerializeField] private Color normalTextColor = new Color32(0x11, 0x11, 0x11, 0xFF);
    [SerializeField] private Color alertTextColor = new Color32(0xF4, 0x43, 0x36, 0xFF);

    [Header("Blink")]
    [SerializeField] private float blinkInterval = 0.6f; // 깜빡임 간격 (초)
    [SerializeField] private int dangerThreshold = 3; // 이 수치 이하일 때 깜빡임 시작

    private Tween blinkTween;

    private int currentValue;
    private int maxValue;

    private void Awake()
    {
        ResetVisualImmediate();
    }
    private void OnDisable()
    {
        StopBlink();
        ResetVisualImmediate();
    }
    public void SetValue(int currentValue, int maxValue)
    {
        this.currentValue = Mathf.Max(0, currentValue);
        this.maxValue = Mathf.Max(0, maxValue);

        float ratio = maxValue <= 0 ? 0f : Mathf.Clamp01((float)this.currentValue / this.maxValue);

        if(valueText != null)
        {
            valueText.text = $"{this.currentValue}/{this.maxValue}";
            valueText.color = this.currentValue <= dangerThreshold ? alertTextColor : normalTextColor;
        }

        if(fillNormal != null)
            fillNormal.fillAmount = ratio;

        if(fillAlert != null)
            fillAlert.fillAmount = ratio;

        ApplyVisualState();
    }
    private void ApplyVisualState()
    {
        StopBlink();
        ResetVisualImmediate();

        if (currentValue <= 0) // 아예 0 이하면 배경 깜빡임
        {
            ApplyZeroState();
            return;
        }
        else if (currentValue <= dangerThreshold) // dangerThreshold 이하면 경고 상태로 깜빡임
        {
            ApplyAlertState();
            return;
        }
        else // 그 외에는 정상 상태
        {
            ApplyNormalState();
        }
    }
    private void ApplyNormalState() // 정상 상태
    {
        SetImageEnabled(bgNormal, true);
        SetImageEnabled(fillNormal, true);
        SetImageEnabled(bgAlert, false);
        SetImageEnabled(fillAlert, false);

        SetImageAlpha(bgNormal, 1f);
        SetImageAlpha(bgAlert, 0f);
        SetImageAlpha(fillNormal, 1f);
        SetImageAlpha(fillAlert, 0f);
    }
    private void ApplyAlertState() // 경고 상태
    {
        SetImageEnabled(bgNormal, true);
        SetImageEnabled(fillNormal, true);
        SetImageEnabled(bgAlert, false);
        SetImageEnabled(fillAlert, true);

        SetImageAlpha(bgNormal, 1f);
        SetImageAlpha(bgAlert, 0f);
        SetImageAlpha(fillNormal, 1f);
        SetImageAlpha(fillAlert, 0f);

        if(fillAlert != null)
        {
            blinkTween = fillAlert
                .DOFade(1f, blinkInterval)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }
    }
    private void ApplyZeroState() // 0 이하면 배경 깜빡임
    {
        SetImageEnabled(bgNormal, true);
        SetImageEnabled(fillNormal, true);
        SetImageEnabled(bgAlert, true);
        SetImageEnabled(fillAlert, false);

        SetImageAlpha(bgNormal, 1f);
        SetImageAlpha(bgAlert, 0f);
        SetImageAlpha(fillNormal, 0f);
        SetImageAlpha(fillAlert, 0f);

        if(bgAlert != null)
        {
            blinkTween = bgAlert
                .DOFade(1f, blinkInterval)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }
    }
    private void StopBlink()
    {
        if(blinkTween != null)
        {
            blinkTween.Kill();
            blinkTween = null;
        }
    }
    private void ResetVisualImmediate()
    {
        if(bgNormal != null)
        {
            bgNormal.DOKill();
            SetImageEnabled(bgNormal, true);
            SetImageAlpha(bgNormal, 1f);
        }
        if(bgAlert != null)
        {
            bgAlert.DOKill();
            SetImageEnabled(bgAlert, true);
            SetImageAlpha(bgAlert, 0f);
        }
        if (fillNormal != null)
        {
            fillNormal.DOKill();
            SetImageEnabled(fillNormal, true);
            SetImageAlpha(fillNormal, 1f);
        }
        if (fillAlert != null)
        {
            fillAlert.DOKill();
            SetImageEnabled(fillAlert, true);
            SetImageAlpha(fillAlert, 0f);
        }
    }
    private void SetImageEnabled(Image image, bool enabled)
    {
        if (image != null)
            image.enabled = enabled;
    }
    private void SetImageAlpha(Image image, float alpha)
    {
        if(image == null) return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
