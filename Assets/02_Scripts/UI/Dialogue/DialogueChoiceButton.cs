using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueChoiceButton : ToggleButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float horizontalPadding = 16f;

    private RectTransform rect;
    private DialogueChoiceData data;

    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        rect = GetComponent<RectTransform>();
    }

    public void Init(DialogueChoiceData data, int no)
    {
        this.data = data;
        text.text = $"{no}. {data.text}";

        // text 길이에 맞게 width 조정하기
        float width = text.preferredWidth + horizontalPadding;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    protected override void OnClickInternal()
    {
        ConfirmChoice();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectChoice();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UnselectChoice();
    }

    // hover 또는 select 시 폰트 수정
    public void SelectChoice()
    {
        text.fontStyle |= TMPro.FontStyles.Bold;
        SetState(true);
    }

    public void UnselectChoice()
    {
        text.fontStyle &= ~TMPro.FontStyles.Bold;
        SetState(false);
    }

    public void ConfirmChoice()
    {

    }
}
