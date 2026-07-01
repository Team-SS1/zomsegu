using PlayerEnum;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    [Header("Punch")]
    [SerializeField] private Animator punchAnimator;
    [SerializeField] private SpriteRenderer punchSprite;

    [Header("Bat")]
    [SerializeField] private Animator batAnimator;
    [SerializeField] private SpriteRenderer batSprite;

    [Header("Blunt")]
    [SerializeField] private Animator bluntAnimator;
    [SerializeField] private SpriteRenderer bluntSprite;

    [Header("Hammer")]
    [SerializeField] private Animator hammerAnimator;
    [SerializeField] private SpriteRenderer hammerSprite;

    [Header("Knife")]
    [SerializeField] private Animator knifeAnimator;
    [SerializeField] private SpriteRenderer knifeSprite;

    public Animator CurrentAnimator { get; private set; }
    public SpriteRenderer CurrentSprite { get; private set; }

    private void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null) return;

        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }

    public void ChangeSprite(SpriteType type)
    {
        // 전부 숨김
        SetAlpha(punchSprite, 0f);
        SetAlpha(batSprite, 0f);
        SetAlpha(bluntSprite, 0f);
        SetAlpha(hammerSprite, 0f);
        SetAlpha(knifeSprite, 0f);

        switch (type)
        {
            case SpriteType.Punch:
                SetAlpha(punchSprite, 1f);
                CurrentAnimator = punchAnimator;
                CurrentSprite = punchSprite;
                break;

            case SpriteType.Bat:
                SetAlpha(batSprite, 1f);
                CurrentAnimator = batAnimator;
                CurrentSprite = batSprite;
                break;

            case SpriteType.Blunt:
                SetAlpha(bluntSprite, 1f);
                CurrentAnimator = bluntAnimator;
                CurrentSprite = bluntSprite;
                break;

            case SpriteType.Hammer:
                SetAlpha(hammerSprite, 1f);
                CurrentAnimator = hammerAnimator;
                CurrentSprite = hammerSprite;
                break;

            case SpriteType.Knife:
                SetAlpha(knifeSprite, 1f);
                CurrentAnimator = knifeAnimator;
                CurrentSprite = knifeSprite;
                break;
        }
    }

    public void SetVisible(bool visible)
    {
        if (!visible)
        {
            HideAllSprites();
            return;
        }

        if (CurrentSprite != null)
            SetAlpha(CurrentSprite, 1f);
    }

    public void HideAllSprites()
    {
        SetAlpha(punchSprite, 0f);
        SetAlpha(batSprite, 0f);
        SetAlpha(bluntSprite, 0f);
        SetAlpha(hammerSprite, 0f);
        SetAlpha(knifeSprite, 0f);
    }
}