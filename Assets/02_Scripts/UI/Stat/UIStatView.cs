using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIEnum;
using Unity.VisualScripting;
public class UIStatView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI valueTXT;

    [Header("Sprite")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;

    [Header("Color")]
    [SerializeField] private Color normalColor = new Color32(0x11, 0x11, 0x11, 0xFF);
    [SerializeField] private Color upColor = new Color32(0x3D, 0xDC, 0x84, 0xFF);
    [SerializeField] private Color downColor = new Color32(0xF4, 0x43, 0x36, 0xFF);

    public void SetValue(string valueText, UIStatViewState state)
    {
        SetText(valueText);
        ApplyState(state);
    }
    public void Clear()
    {
        if(valueTXT != null) valueTXT.text = "";

        if(iconImg != null)
        {
            iconImg.sprite = normalSprite;
            iconImg.enabled = normalSprite != null;
        }
    }
    private void SetText(string valueText)
    {
        if (valueTXT == null) return;

        valueTXT.text = valueText;
    }
    private void ApplyState(UIStatViewState state)
    {
        Sprite sprite = normalSprite;
        Color targetColor = normalColor;

        switch (state)
        {
            case UIStatViewState.Normal:
                sprite = normalSprite;
                targetColor = normalColor;
                break;

            case UIStatViewState.Up:
                sprite = upSprite;
                targetColor = upColor;
                break;

            case UIStatViewState.Down:
                sprite = downSprite;
                targetColor = downColor;
                break;
        }

        if(iconImg != null)
        {
            iconImg.sprite = sprite;
            iconImg.enabled = sprite != null;
        }

        if(valueTXT != null)
            valueTXT.color = targetColor;
    }
    
}
