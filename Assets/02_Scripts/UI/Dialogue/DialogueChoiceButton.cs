using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueChoiceButton : ToggleButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float horizontalPadding = 16f;

    // components - unity
    private RectTransform rect;

    // components
    private UIDialogue ui;
    private DialogueTyper typer;

    // data
    private DialogueChoiceData data;
    private int no;

    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        rect = GetComponent<RectTransform>();
        typer = GetComponentInChildren<DialogueTyper>(true);
    }

    public void Init(UIDialogue ui, DialogueChoiceData data, int no)
    {
        gameObject.SetActive(false);

        this.ui = ui;
        this.data = data;
        this.no = no;
        text.text = $"{no + 1}. {data.text}";

        // text 길이에 맞게 width 조정하기
        float width = text.preferredWidth + horizontalPadding * 2;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    #region 타이핑
    public IEnumerator CoPlayLine()
    {
        gameObject.SetActive(true);
        typer.PlayLine();
        yield return new WaitForSecondsRealtime(data.text.Length * 0.03f);
    }

    public void CompleteTyping()
    {
        gameObject.SetActive(true);
        typer.SkipOrComplete();
    }
    #endregion

    protected override void OnClickInternal()
    {
        SubmitChoice();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.CurChoiceIndex = no;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.CurChoiceIndex = -1;
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

    public void SubmitChoice()
    {
        var popup = UIManager.Instance.OpenUI<UIConfirmPopup>();
        popup?.Register($"{no + 1}. {data.text}를 선택합니다.", () =>
        {
            ui.CurChoiceIndex = no;
            ui.SetCurChoice(data);
        });
    }
}
