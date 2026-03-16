using DialogueEnum;
using InputEnum;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIDialogue : MonoBehaviour
{
    #region 필드
    [Header("Buttons")]
    [SerializeField] BaseButton skipBtn;
    [SerializeField] BaseButton autoBtn;
    [SerializeField] BaseButton backlogBtn;
    [SerializeField] BaseButton optionBtn;
    [SerializeField] BaseButton dialogueWindowBtn;

    [Header("Dialogue")]
    [SerializeField] Image portrait;
    [SerializeField] TMP_Text characterName;
    [SerializeField] DialogueTyper typer;
    [SerializeField] float skipDelayTime = 0.3f;

    [Header("Extra")]
    [SerializeField] GameObject autoPlaying;

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
    private int CurChoiceIndex
    {
        get { return curChoiceIndex; }
        set
        {
            if (curChoiceIndex != -1)
            {
                choiceBtns[curChoiceIndex].UnselectChoice();
            }
            curChoiceIndex = value;
            choiceBtns[curChoiceIndex].SelectChoice();
        }
    }

    private bool needChoice = false;

    // dialogue mode
    private DialogueMode curMode = DialogueMode.None;

    // skip
    private Coroutine skipCoroutine;
    private WaitForSecondsRealtime skipDelay;
    #endregion

    #region Unity API
    private void Awake()
    {
        skipBtn.onClick.AddListener(OnClickSkipBtn);
        autoBtn.onClick.AddListener(OnClickAutoBtn);
        backlogBtn.onClick.AddListener(OnClickBacklogBtn);
        optionBtn.onClick.AddListener(OnClickOptionBtn);
        dialogueWindowBtn.onClick.AddListener(OnClickDialogueWindowBtn);

        skipDelay = new WaitForSecondsRealtime(skipDelayTime);

        choiceBtnHeight = choiceBtnPrefab.GetComponent<RectTransform>().rect.height;
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;

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
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        ChangeMode(DialogueMode.None);
        typer.Clear();
        dialogues.Clear();
        curDialogue = null;
        for (int i = choiceBtns.Count - 1; i >= 0; i--)
        {
            Destroy(choiceBtns[i].gameObject);
        }
        choiceBtns.Clear();

        index = 0;
        lockIndex = 0;

        InputManager mg = InputManager.Instance;
        if (mg == null) return;
        mg.AddMaps(ActionMaps.Gameplay);
        mg.AddMaps(ActionMaps.UI);
        mg.RemoveMaps(ActionMaps.Dialogue);
    }
    #endregion

    #region 버튼 이벤트
    private void OnClickSkipBtn()
    {
        ChangeMode(curMode != DialogueMode.Skip ? DialogueMode.Skip : DialogueMode.None);
    }

    private void OnClickAutoBtn()
    {
        ChangeMode(curMode != DialogueMode.Auto ? DialogueMode.Auto : DialogueMode.None);
    }

    private void OnClickBacklogBtn()
    {
        // todo: 백로그 ui 띄우기
    }

    private void OnClickOptionBtn()
    {
        // todo: 옵션 ui 띄우기
    }

    private void OnClickDialogueWindowBtn()
    {
        AdvanceOrCompleteCurrentLine();
        EventSystem.current?.SetSelectedGameObject(null);   // 버튼 캐싱 삭제
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
            return;
        }

        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (needChoice) return;

        index++;

        if (curChoice != null)
        {
            TryShowDialogue(curChoice.nextDialogueId);
            curChoice = null;
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
    }

    private void TryShowDialogue(int id)
    {
        if (id == -1)
        {
            gameObject.SetActive(false);
            return;
        }

        DialogueData.tableDic.TryGetValue(id, out curDialogue);

        if (!dialogues.Contains(curDialogue))
        {
            dialogues.Add(curDialogue);
        }

        if (curDialogue.HasChoice)          // 선택지 index 캐싱해서 선택지 전으로 못 돌아가게 하기
        {
            lockIndex = index;
            needChoice = true;
            ShowChoice(curDialogue);
        }
        else
        {
            ShowLine(curDialogue);
        }
    }

    private void ShowChoice(DialogueData data)
    {
        characterName.text = data.characterName;
        typer.OnEnd += CreateChoiceButton;
        typer.PlayLine(data.text);
    }

    private void CreateChoiceButton()
    {
        DialogueData data = curDialogue;
        for (int i = 0; i < data.choiceIds.Count; i++)
        {
            DialogueChoiceData.tableDic.TryGetValue(data.choiceIds[i], out DialogueChoiceData choiceData);

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

            choiceBtn.Init(choiceData, i + 1);
        }

        curChoiceIndex = -1;

        typer.OnEnd -= CreateChoiceButton;
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
        characterName.text = data.characterName;
        typer.PlayLine(data.text);
    }

    private IEnumerator SkipDialogueRoutine()
    {
        while (true)
        {
            yield return skipDelay;
            ShowNextLine();
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
            Logger.Log("대화 전체 스킵");
        }
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        int count = curDialogue.choiceIds.Count;
        if (count <= 0) return;

        if (context.started)
        {
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
        }
    }
    #endregion

    #region 모드 설정
    private void ChangeMode(DialogueMode mode)
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
                autoPlaying.SetActive(true);
                if (!typer.IsTyping) ShowNextLine();
                typer.OnEnd += ShowNextLine;
                break;
            default:
                break;
        }
    }

    private void ClearModeState()
    {
        if (skipCoroutine != null)
        {
            StopCoroutine(skipCoroutine);
            skipCoroutine = null;
        }

        autoPlaying.SetActive(false);

        skipBtn.ResetState();
        autoBtn.ResetState();

        typer.OnEnd -= ShowNextLine;
    }
    #endregion

    #region 유니티 전용
#if UNITY_EDITOR
    private void Reset()
    {
        skipBtn = transform.FindChild<BaseButton>("Btn_Skip");
        autoBtn = transform.FindChild<BaseButton>("Btn_Auto");
        backlogBtn = transform.FindChild<BaseButton>("Btn_Backlog");
        optionBtn = transform.FindChild<BaseButton>("Btn_Option");
        dialogueWindowBtn = transform.FindChild<BaseButton>("Panel_Dialogue");

        portrait = transform.FindChild<Image>("Portrait");
        characterName = transform.FindChild<TMP_Text>("Text_Name");
        typer = transform.FindChild<DialogueTyper>("Text_Dialogue");

        autoPlaying = transform.FindChild<TMP_Text>("Text_AutoPlaying").gameObject;

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
