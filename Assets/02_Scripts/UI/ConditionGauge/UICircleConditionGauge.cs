using System.Collections;
using TMPro;
using UIEnum;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private float blinkInterval = 0.5f; // 깜빡임 간격 (초)
    [SerializeField] private int dangerThreshold = 3; // 이 수치 이하일 때 깜빡임 시작

    private Coroutine blinkCoroutine;
    private BlinkMode currentBlinkMode = BlinkMode.None;

    private int currentValue;
    private int maxValue;

    private void OnDisable()
    {
        StopBlink();
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

        if (currentValue <= 0) // 아예 0 이하면 배경 깜빡임
        {
            AppyZeroState();
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
    }
    private void ApplyAlertState() // 경고 상태
    {
        SetImageEnabled(bgNormal, true);
        SetImageEnabled(fillNormal, true);
        SetImageEnabled(bgAlert, false);
        SetImageEnabled(fillAlert, false);

        currentBlinkMode = BlinkMode.FillAlert;
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }
    private void AppyZeroState() // 0 이하면 배경 깜빡임
    {
        SetImageEnabled(bgNormal, true);
        SetImageEnabled(fillNormal, false);
        SetImageEnabled(bgAlert, false);
        SetImageEnabled(fillAlert, false);

        currentBlinkMode = BlinkMode.BgAlert;
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }
    private IEnumerator BlinkRoutine()
    {
        bool showAlert = false;

        while (true)
        {
            showAlert = !showAlert;

            if (currentBlinkMode == BlinkMode.FillAlert)
            {
                SetImageEnabled(bgNormal, true);
                SetImageEnabled(bgAlert, false);

                SetImageEnabled(fillNormal, !showAlert);
                SetImageEnabled(fillAlert, showAlert);
            }
            else if (currentBlinkMode == BlinkMode.BgAlert)
            {
                SetImageEnabled(bgNormal, !showAlert);
                SetImageEnabled(bgAlert, showAlert);

                SetImageEnabled(fillNormal, false);
                SetImageEnabled(fillAlert, false);
            }
            yield return new WaitForSecondsRealtime(blinkInterval);
        }
    }
    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        currentBlinkMode = BlinkMode.None;
    }
    private void SetImageEnabled(Image image, bool enabled)
    {
        if (image != null)
            image.enabled = enabled;
    }
}
