using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class UIShockGauge : MonoBehaviour
{
    [Header("Segment")]
    [SerializeField] private Image[] segmentImages; // 7칸 이미지들 각각 다 다름

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI valueText;

    [Header("Color")]
    [SerializeField] private Color normalTextColor = new Color32(0x11, 0x11, 0x11, 0xFF);
    [SerializeField] private Color alertTextColor = new Color32(0xF4, 0x43, 0x36, 0xFF);

    [Header("Blink")]
    [SerializeField] private float alertThreshold = 3; // 이 수치 이하일 때 깜빡임 시작
    [SerializeField] private float blinkDuration = 0.6f; // 깜빡임 간격 (초)

    private Tween blinkTween;
    private int currentValue;
    private int maxValue;

    private void Awake()
    {
        ResetSegmentsImmediate();
    }
    private void OnDisable()
    {
        StopBlink();
        ResetSegmentsImmediate();
    }
    public void SetValue(int currentValue, int maxValue)
    {  
        this.maxValue = Mathf.Max(0, maxValue);
        this.currentValue = Mathf.Clamp(currentValue, 0, this.maxValue);

        RefreshText();
        RefreshSegments();
        ApplyBlinkState();
    }
    private void RefreshText()
    {
        if(valueText == null) return;

        valueText.text = $"{currentValue}/{maxValue}";
        valueText.color = currentValue >= alertThreshold ? alertTextColor : normalTextColor;
    }
    private void RefreshSegments()
    {
        if (segmentImages == null || segmentImages.Length == 0) return;

        for(int i = 0; i < segmentImages.Length; i++)
        {
            if (segmentImages[i] == null) continue;

            bool active = i < currentValue;
            segmentImages[i].enabled = active;

            Color color = segmentImages[i].color;
            color.a = 1f;
            segmentImages[i].color = color;
        }
    }
    private void ApplyBlinkState()
    {
        StopBlink();

        if (currentValue < alertThreshold)
            return;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        for(int i = 0; i < segmentImages.Length; i++)
        {
            if (segmentImages[i] == null) continue;
            if(!segmentImages[i].enabled) continue;

            seq.Join(segmentImages[i].DOFade(0.35f, blinkDuration));
        }

        seq.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

        blinkTween = seq;
    }
    private void StopBlink()
    {
        if (blinkTween != null && blinkTween.IsActive())
        {
            blinkTween.Kill();
            blinkTween = null;
        }

        if(segmentImages == null) return;

        for(int i = 0; i < segmentImages.Length; i++)
        {
            if (segmentImages[i] == null) continue;

            segmentImages[i].DOKill();

            Color color = segmentImages[i].color;
            color.a = 1f;
            segmentImages[i].color = color;
        }
    }
    private void ResetSegmentsImmediate()
    {
        if(segmentImages == null) return;

        for(int i = 0; i < segmentImages.Length; i++)
        {
            if (segmentImages[i] == null) continue;

            segmentImages[i].enabled = false;

            Color color = segmentImages[i].color;
            color.a = 1f;
            segmentImages[i].color = color;
        }
    }
}
