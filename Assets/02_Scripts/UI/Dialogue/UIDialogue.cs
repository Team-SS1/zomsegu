using DialogueEnum;
using InputEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogue : BaseUI
{
    #region 필드
    [Header("Buttons")]
    [SerializeField] BaseButton dialogueWindowBtn;

    [Header("Dialogue")]
    [SerializeField] Image portrait;
    [SerializeField] TMP_Text speaker;
    [SerializeField] DialogueTyper typer;
    [SerializeField] GameObject arrow;
    [SerializeField] float skipDelayTime = 0.3f;
    [SerializeField] float endDelayMultiplier;

    [Header("Choice")]
    [SerializeField] DialogueChoiceButton choiceBtnPrefab;
    [SerializeField] RectTransform choiceRoot;
    private float choiceBtnHeight;

    private List<DialogueData> dialogues = new();
    private List<DialogueChoiceButton> choiceBtns = new();

    private DialogueData curDialogue;
    private DialogueChoiceData curChoice;

    private int index = 0;
    private int lockIndex = 0;
    private int curChoiceIndex = -1;
    public int CurChoiceIndex
    {
        get { return curChoiceIndex; }
        set
        {
            if (value == -1)
            {
                choiceBtns.ForEach(b => b.UnselectChoice());
                curChoiceIndex = value;
                return;
            }

            if (curChoiceIndex != -1)
            {
                choiceBtns[curChoiceIndex].UnselectChoice();
            }
            curChoiceIndex = value;
            choiceBtns[curChoiceIndex].SelectChoice();
        }
    }

    private bool needChoice = false;
    public bool IsChoiceRequired => needChoice;

    private List<IEnumerator> choiceBtnsCos = new();
    private Coroutine choiceBtnsRoutine;
    private Coroutine choiceBtnRoutine;

    // dialogue mode
    private DialogueMode curMode = DialogueMode.None;
    public DialogueMode CurMode => curMode;

    // skip
    private Coroutine skipCoroutine;
    private WaitForSecondsRealtime skipDelay;

    private List<DialogueBacklog> backlogs = new();

    public event Action OnChangeMode;

    private UIDialogueTopButtonsController btnsController;
    #endregion

    #region Unity API
    private void Awake()
    {
        btnsController = GetComponentInChildren<UIDialogueTopButtonsController>(true);
        btnsController.Init(this);

        dialogueWindowBtn.onClick.AddListener(OnClickDialogueWindowBtn);

        skipDelay = new WaitForSecondsRealtime(skipDelayTime);

        choiceBtnHeight = choiceBtnPrefab.GetComponent<RectTransform>().rect.height;
    }

    private void OnDestroy()
    {
        OnChangeMode = null;
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        typer.OnEnd += OnAutoEndTyping;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        ChangeMode(DialogueMode.None);
        typer.Clear();
        typer.OnEnd -= OnAutoEndTyping;
        dialogues.Clear();
        curDialogue = null;
        for (int i = choiceBtns.Count - 1; i >= 0; i--)
        {
            Destroy(choiceBtns[i].gameObject);
        }
        choiceBtns.Clear();

        index = 0;
        lockIndex = 0;

        backlogs.Clear();

        choiceBtnsCos.Clear();
        StopActiveCoroutine(choiceBtnRoutine);
        StopActiveCoroutine(choiceBtnsRoutine);
    }
    #endregion

    private void StopActiveCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    #region 버튼 이벤트
    private void OnClickDialogueWindowBtn()
    {
        ChangeMode(DialogueMode.None);
        AdvanceOrCompleteCurrentLine();
    }
    #endregion

    #region 대화 관리
    public void StartDialogue(int id)
    {
        gameObject.SetActive(true);
        TryShowDialogue(id);
    }

    public void AdvanceOrCompleteCurrentLine()
    {
        ChangeMode(DialogueMode.None);

        if (typer.IsTyping)
        {
            typer.SkipOrComplete();

            if (!needChoice)
            {
                arrow.SetActive(true);
            }
        }
        else
        {
            TryShowNextLine();
        }

        StopActiveCoroutine(choiceBtnRoutine);
        StopActiveCoroutine(choiceBtnsRoutine);

        if (needChoice)
        {
            foreach (DialogueChoiceButton btn in choiceBtns)
            {
                btn.CompleteTyping();
            }
        }
    }

    private bool TryShowNextLine()
    {
        if (curDialogue == null || needChoice) return false;

        index++;

        if (curChoice != null)
        {
            if (!TryShowDialogue(curChoice.nextDialogueId)) return false;
            curChoice = null;
            CurChoiceIndex = -1;
            lockIndex = index;
            choiceBtns.ForEach(btn => btn.gameObject.SetActive(false));
        }

        if (index < dialogues.Count)        // 새로운 대화가 아니면 캐싱된 거 가지고 오기
        {
            ShowLine(dialogues[index]);
        }
        else
        {
            TryShowDialogue(curDialogue.nextDialogueId);
        }

        arrow.SetActive(false);
        return true;
    }

    private bool TryShowDialogue(int id)
    {
        if (id == -1)
        {
            gameObject.SetActive(false);
            return false;
        }

        if (!DialogueData.tableDic.TryGetValue(id, out curDialogue)) return false;

        if (!dialogues.Contains(curDialogue))
        {
            dialogues.Add(curDialogue);
        }

        if (curDialogue.HasChoice)          // 선택지 index 캐싱해서 선택지 전으로 못 돌아가게 하기
        {
            lockIndex = index;
            SetNeedChoice(true);
            ShowChoice(curDialogue);
        }
        else
        {
            ShowLine(curDialogue);

            DialogueBacklog backlog = new()
            {
                isPlayer = curDialogue.isPlayer,
                speaker = curDialogue.speaker,
                dialogueText = curDialogue.text
            };
            backlogs.Add(backlog);
        }

        return true;
    }

    public void SetCurChoice(DialogueChoiceData data)
    {
        curChoice = data;
        SetNeedChoice(false);

        DialogueBacklog backlog = backlogs[^1];
        backlog.choiceTexts[curChoiceIndex]
            = $"<color=#FF0000><b>{backlog.choiceTexts[curChoiceIndex]}</b></color>";
        backlogs[^1] = backlog;

        TryShowNextLine();
    }

    private void ShowChoice(DialogueData data)
    {
        speaker.text = data.speaker;
        typer.OnEnd += CreateChoiceButton;
        typer.PlayLine(data.text);

        DialogueBacklog backlog = new()
        {
            isPlayer = data.isPlayer,
            speaker = data.speaker,
            dialogueText = data.text,
            choiceTexts = new string[data.choiceIds.Count]
        };
        backlogs.Add(backlog);
    }

    private WaitForSecondsRealtime CreateChoiceButton()
    {
        DialogueData data = curDialogue;
        for (int i = 0; i < data.choiceIds.Count; i++)
        {
            if (!DialogueChoiceData.tableDic
                .TryGetValue(data.choiceIds[i], out DialogueChoiceData choiceData))
            {
                return null;
            }

            DialogueChoiceButton choiceBtn;
            if (i < choiceBtns.Count)
            {
                choiceBtn = choiceBtns[i];
            }
            else
            {
                choiceBtn = Instantiate(choiceBtnPrefab);
                choiceBtns.Add(choiceBtn);
            }
            choiceBtn.gameObject.SetActive(true);

            RectTransform rect = choiceBtn.GetComponent<RectTransform>();
            rect.SetParent(choiceRoot);
            rect.anchoredPosition = new Vector2(0, -choiceBtnHeight * i);

            choiceBtn.Init(this, choiceData, i);
            choiceBtnsCos.Add(choiceBtn.CoPlayLine());

            DialogueBacklog backlog = backlogs[^1];
            backlog.choiceTexts[i] = $"{i + 1}. {choiceData.text}";
            backlogs[^1] = backlog;
        }

        curChoiceIndex = -1;
        choiceBtnsRoutine = StartCoroutine(CoChoiceBtns());
        typer.OnEnd -= CreateChoiceButton;

        return null;
    }

    private IEnumerator CoChoiceBtns()
    {
        foreach (IEnumerator choice in choiceBtnsCos)
        {
            if (choiceBtnRoutine != null)
            {
                StopActiveCoroutine(choiceBtnRoutine);
                choiceBtnRoutine = null;
            }
            choiceBtnRoutine = StartCoroutine(choice);
            yield return choiceBtnRoutine;
        }
        choiceBtnsCos.Clear();

        StopActiveCoroutine(choiceBtnsRoutine);
    }

    public void ShowPreviousLine()
    {
        index--;
        if (index < lockIndex)
        {
            index = lockIndex;
            return;
        }
        else if (index >= dialogues.Count)
        {
            gameObject.SetActive(false);
            return;
        }

        ShowLine(dialogues[index]);
    }

    private void ShowLine(DialogueData data)
    {
        speaker.text = data.speaker;
        typer.PlayLine(data.text);
    }

    private IEnumerator SkipDialogueRoutine()
    {
        while (true)
        {
            yield return skipDelay;
            TryShowNextLine();
            typer.SkipOrComplete();
        }
    }
    #endregion

    #region Input 이벤트
    public void AllSkip()
    {
        do
        {
            AdvanceOrCompleteCurrentLine();
        }
        while (TryShowNextLine());
    }

    public bool CanSelectChoice()
    {
        if (curDialogue == null ||
            curDialogue.choiceIds == null ||
            curDialogue.choiceIds.Count <= 0)
            return false;

        return true;
    }

    public void SelectChoice(float axis)
    {
        int count = curDialogue.choiceIds.Count;

        int dir = axis > 0f ? count - 1 : 1;

        if (CurChoiceIndex == -1)   // 처음 선택
        {
            CurChoiceIndex = dir > 0f ? 0 : count - 1;
            return;
        }

        CurChoiceIndex = (curChoiceIndex + dir) % count;
    }

    public void SubmitCurChoice()
    {
        if (CurChoiceIndex < 0 || CurChoiceIndex >= choiceBtns.Count) return;
        choiceBtns[curChoiceIndex].SubmitChoice();
    }
    #endregion

    #region 모드 설정
    public void ChangeMode(DialogueMode mode)
    {
        if (curMode == mode) return;

        curMode = mode;

        ClearModeState();

        switch (curMode)
        {
            case DialogueMode.Skip:
                skipCoroutine = StartCoroutine(SkipDialogueRoutine());
                break;
            case DialogueMode.Auto:
                if (!typer.IsTyping) TryShowNextLine();
                break;
            case DialogueMode.Backlog:
                var ui = UIManager.Instance.OpenUI<UIDialogueBacklog>();
                ui.AddBacklogs(backlogs);
                typer.SkipOrComplete();
                if (!needChoice)
                {
                    arrow.SetActive(true);
                }
                break;
            default:
                break;
        }

        OnChangeMode?.Invoke();
    }

    public void ToggleMode(DialogueMode mode)
    {
        ChangeMode(curMode == mode ? DialogueMode.None : mode);
    }

    private void ClearModeState()
    {
        if (skipCoroutine != null)
        {
            StopActiveCoroutine(skipCoroutine);
            skipCoroutine = null;
        }

        UIManager.Instance?.ClosePanel<UIDialogueBacklog>();
    }

    private void SetNeedChoice(bool active)
    {
        needChoice = active;

        if (needChoice)
        {
            InputManager.Instance.UnlockInput(ActionMaps.Dialogue, Actions.Navigate);
            InputManager.Instance.UnlockInput(ActionMaps.Dialogue, Actions.Submit);
        }
        else
        {
            InputManager.Instance.LockInput(ActionMaps.Dialogue, Actions.Navigate);
            InputManager.Instance.LockInput(ActionMaps.Dialogue, Actions.Submit);
        }
    }
    #endregion

    private IEnumerator OnAutoEndTyping()
    {
        if (!needChoice)
        {
            arrow.SetActive(true);
        }

        if (curMode == DialogueMode.Auto)
        {
            yield return new WaitForSecondsRealtime(curDialogue.text.Length * endDelayMultiplier);
            TryShowNextLine();
        }
    }

    #region 유니티 전용
#if UNITY_EDITOR
    private void Reset()
    {
        dialogueWindowBtn = transform.FindChild<BaseButton>("Panel_Dialogue");

        portrait = transform.FindChild<Image>("Portrait");
        speaker = transform.FindChild<TMP_Text>("Text_Speaker");
        typer = transform.FindChild<DialogueTyper>("Text_Dialogue");
        arrow = transform.FindChild<Image>("Arrow").gameObject;

        choiceBtnPrefab = AssetLoader.FindAndLoadByName("Btn_Choice").GetComponent<DialogueChoiceButton>();
        choiceRoot = transform.FindChild<RectTransform>("ChoiceGroup");
    }

    private void OnValidate()
    {
        skipDelay = new WaitForSecondsRealtime(skipDelayTime);
    }
#endif
    #endregion
}
