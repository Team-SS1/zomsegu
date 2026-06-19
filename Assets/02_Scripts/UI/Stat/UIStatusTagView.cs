using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIEnum;
public class UIStatusTagView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImg;

    [Header("Sprite")]
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite dangerSprite;
    [SerializeField] private Sprite severeSprite;
    [SerializeField] private Sprite deadSprite;

    public void SetState(UIStatusTagState state)
    {
        Sprite targetSprite = GetSprite(state);

        if (iconImg == null)
            return;

        iconImg.sprite = targetSprite;
        iconImg.enabled = targetSprite != null;
    }
    public void Clear()
    {
        SetState(UIStatusTagState.Inactive);
    }
    private Sprite GetSprite(UIStatusTagState state)
    {
        switch (state)
        {
            case UIStatusTagState.Active:
                return activeSprite != null ? activeSprite : inactiveSprite;
            case UIStatusTagState.Danger:
                return dangerSprite != null ? dangerSprite : activeSprite;
            case UIStatusTagState.Severe:
                return severeSprite != null ? severeSprite : dangerSprite;
            case UIStatusTagState.Dead:
                return deadSprite != null ? deadSprite : severeSprite;
            case UIStatusTagState.Inactive:
            default:
                return inactiveSprite;
        }
    }

}
