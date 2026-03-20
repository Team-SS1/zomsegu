using DialogueEnum;
using InputEnum;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public bool NeedChoice => needChoice;

    // dialogue mode
    private DialogueMode curMode = DialogueMode.None;
    public DialogueMode CurMode => curMode;

    // skip
    private Coroutine skipCoroutine;
    private WaitForSecondsRealtime skipDelay;

    private List<DialogueBacklog> backlogs = new();
    private DialogueBacklog curBacklog;

    public event Action OnChangeMode;

    private UIDialogueTopButtonController btnController;
    #endregion

    #region Unity API
    private void Awake()
    {
        btnController = GetComponentInChildren<UIDialogueTopButtonController>(true);
        btnController.Init(this);

        dialogueWindowBtn.onClick.AddListener(OnClickDialogueWindowBtn);

        skipDelay = new WaitForSecondsRealtime(skipDelayTime);

        choiceBtnHeight = choiceBtnPrefab.GetComponent<RectTransform>().rect.height;
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        typer.OnEnd += OnEndTyping;

        InputManager mg = InputManager.Instance;
        if (mg == null) return;
        mg.RemoveMaps(ActionMaps.Gameplay);
        mg.RemoveMaps(ActionMaps.UI);
        mg.AddMaps(ActionMaps.Dialogue);
    }

    private void Start()
    {
        InputManager mg = InputManager.Instance;
        mg.BindInput(ActionMaps.Dialogue, Actions.Next, OnNext);
        mg.BindInput(ActionMaps.Dialogue, Actions.Previous, OnPrev);
        mg.BindInput(ActionMaps.Dialogue, Actions.Skip, OnSkip);
        mg.BindInput(ActionMaps.Dialogue, Actions.AllSkip, OnAllSkip);
        mg.BindInput(ActionMaps.Dialogue, Actions.Navigate, OnNavigate);
        mg.BindInput(ActionMaps.Dialogue, Actions.Submit, OnSubmit);
        mg.LockInput(ActionMaps.Dialogue, Actions.Submit);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        ChangeMode(DialogueMode.None);
        typer.Clear();
        typer.OnEnd -= OnEndTyping;
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

        InputManager mg = InputManager.Instance;
        if (mg == null) return;
        mg.AddMaps(ActionMaps.Gameplay);
        mg.AddMaps(ActionMaps.UI);
        mg.RemoveMaps(ActionMaps.Dialogue);
    }
    #endregion

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

    private void AdvanceOrCompleteCurrentLine()
    {
        ChangeMode(DialogueMode.None);

        if (typer.IsTyping)
        {
            typer.SkipOrComplete();
            if (!needChoice)
            {
                arrow.SetActive(true);
            }
            return;
        }

        TryShowNextLine();
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

            curBacklog = new DialogueBacklog
            {
                isPlayer = curDialogue.isPlayer,
                speaker = curDialogue.speaker,
                dialogueText = curDialogue.text
            };
            backlogs.Add(curBacklog);
        }

        return true;
    }

    public void SetCurChoice(DialogueChoiceData data)
    {
        curChoice = data;
        SetNeedChoice(false);
        curBacklog.choiceTexts[curChoiceIndex]
            = $"<color=#FF0000><b>{curBacklog.choiceTexts[curChoiceIndex]}</b></color>";
        backlogs.Add(curBacklog);
        TryShowNextLine();
    }

    private void ShowChoice(DialogueData data)
    {
        speaker.text = data.speaker;
        typer.OnEnd += CreateChoiceButton;
        typer.PlayLine(data.text);

        curBacklog = new DialogueBacklog
        {
            isPlayer = data.isPlayer,
            speaker = data.speaker,
            dialogueText = data.text,
            choiceTexts = new string[data.choiceIds.Count]
        };
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
            if (choiceBtns.Count < i)
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
            curBacklog.choiceTexts[i] = $"{i + 1}. {choiceData.text}";
        }

        curChoiceIndex = -1;

        typer.OnEnd -= CreateChoiceButton;

        return null;
    }

    private void ShowPreviousLine()
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
    private void OnNext(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            AdvanceOrCompleteCurrentLine();
        }
    }

    private void OnPrev(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ChangeMode(DialogueMode.None);
            if (typer.IsTyping) typer.SkipOrComplete();
            ShowPreviousLine();
        }
    }

    private void OnSkip(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ChangeMode(DialogueMode.Skip);
        }
        else if (context.canceled)
        {
            ChangeMode(DialogueMode.None);
        }
    }

    private void OnAllSkip(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UIManager.Instance.OpenPopup<UIConfirmPopup>().Open("현재 대화를 \n전체스킵하시겠습니까?\n", AllSkip);
        }
    }

    private void AllSkip()
    {
        do
        {
            AdvanceOrCompleteCurrentLine();
        }
        while (TryShowNextLine());
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (curDialogue == null || curDialogue.choiceIds == null) return;
        int count = curDialogue.choiceIds.Count;
        if (count <= 0) return;

        if (context.started)
        {
            bool isAuto = curMode == DialogueMode.Auto;
            AdvanceOrCompleteCurrentLine();

            float axis = context.ReadValue<float>();
            if (Mathf.Approximately(axis, 0f)) return;

            int dir = axis > 0f ? count - 1 : 1;

            if (CurChoiceIndex == -1)   // 처음 선택
            {
                CurChoiceIndex = dir > 0f ? 0 : count - 1;
                return;
            }

            CurChoiceIndex = (curChoiceIndex + dir) % count;

            if (isAuto) ChangeMode(DialogueMode.Auto);
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (CurChoiceIndex < 0 || CurChoiceIndex >= choiceBtns.Count) return;
            choiceBtns[curChoiceIndex].SubmitChoice();
        }
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
                var ui = UIManager.Instance.GetOrCreateUI<UIDialogueBacklog>(true);
                ui.AddBacklogs(backlogs);
                backlogs.Clear();
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
            StopCoroutine(skipCoroutine);
            skipCoroutine = null;
        }
    }

    private void SetNeedChoice(bool active)
    {
        needChoice = active;

        if (needChoice)
        {
            InputManager.Instance.UnlockInput(ActionMaps.Dialogue, Actions.Submit);
        }
        else
        {
            InputManager.Instance.LockInput(ActionMaps.Dialogue, Actions.Submit);
        }
    }
    #endregion

    private IEnumerator OnEndTyping()
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
