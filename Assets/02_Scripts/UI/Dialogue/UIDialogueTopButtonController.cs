using DialogueEnum;
using TMPro;
using UnityEngine;

public class UIDialogueTopButtonController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] UIDialogueTopButton[] topButtons;

    [Header("Extra")]
    [SerializeField] TMP_Text autoPlaying;

    private UIDialogue dialogue;
    private DialogueMode CurMode => dialogue.CurMode;

    public void Init(UIDialogue ui)
    {
        dialogue = ui;
        ui.OnChangeMode += CheckMode;

        topButtons[0].onClick.AddListener(OnClickSkipBtn);
        topButtons[1].onClick.AddListener(OnClickAutoBtn);
        topButtons[2].onClick.AddListener(OnClickBacklogBtn);
        topButtons[3].onClick.AddListener(OnClickOptionBtn);
    }

    private void OnClickSkipBtn()
    {
        dialogue.ToggleMode(DialogueMode.Skip);
    }

    private void OnClickAutoBtn()
    {
        dialogue.ToggleMode(DialogueMode.Auto);
    }

    private void OnClickBacklogBtn()
    {
        dialogue.ChangeMode(DialogueMode.Backlog);
    }

    private void OnClickOptionBtn()
    {
        // todo: 옵션 ui 띄우기
    }

    private void CheckMode()
    {
        topButtons[0].SetState(CurMode == DialogueMode.Skip);
        topButtons[1].SetState(CurMode == DialogueMode.Auto);
        topButtons[2].SetState(CurMode == DialogueMode.Backlog);

        autoPlaying.gameObject.SetActive(CurMode == DialogueMode.Auto);
        autoPlaying.text = dialogue.NeedChoice ? "자동진행 일시정지" : "자동진행 중...";
    }

    #region 유니티 전용
#if UNITY_EDITOR
    private void Reset()
    {
        topButtons = new UIDialogueTopButton[4];
        topButtons[0] = transform.FindChild<UIDialogueTopButton>("Btn_Skip");
        topButtons[1] = transform.FindChild<UIDialogueTopButton>("Btn_Auto");
        topButtons[2] = transform.FindChild<UIDialogueTopButton>("Btn_Backlog");
        topButtons[3] = transform.FindChild<UIDialogueTopButton>("Btn_Option");

        autoPlaying = transform.FindChild<TMP_Text>("Text_AutoPlaying");
    }
#endif
    #endregion
}
