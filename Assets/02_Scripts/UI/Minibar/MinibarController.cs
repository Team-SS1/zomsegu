using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MiniBarController : MonoBehaviour
{ 
    [Header("UI")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button toggleBtn;
    [SerializeField] private Image toggleImg;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closeSprite;

    [Header("Position")]
    [SerializeField] private Vector2 openPos;
    [SerializeField] private Vector2 closePos;

    [Header("Tween")]
    [SerializeField] private float openDuration = 0.25f; //열리는 시간
    [SerializeField] private float closeDuration = 0.2f; //닫히는 시간

    [SerializeField] private bool startOpen = true;

    private bool isOpen;
    private Tween tween;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        isOpen = startOpen;
        panel.anchoredPosition = isOpen ? openPos : closePos;
        toggleBtn.onClick.AddListener(Toggle);
    }
    private void OnDestroy()
    {
        tween?.Kill();
    }
    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        tween?.Kill();
        tween = panel.DOAnchorPos(openPos, openDuration).SetLink(gameObject);

        ToggleImg();
    }
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        tween?.Kill();
        tween = panel.DOAnchorPos(closePos, closeDuration).SetLink(gameObject);

        ToggleImg();
    }
    private void ToggleImg()
    {
        if (toggleImg == null) return;
        toggleImg.sprite = isOpen ? openSprite : closeSprite;
    }
}
