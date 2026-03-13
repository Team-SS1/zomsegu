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
    [SerializeField] Button skipBtn;
    [SerializeField] Button autoBtn;
    [SerializeField] Button backlogBtn;
    [SerializeField] Button optionBtn;
    [SerializeField] Button dialogueWindowBtn;

    [Header("Dialogue")]
    [SerializeField] Image portrait;
    [SerializeField] TMP_Text characterName;
    [SerializeField] DialogueTyper typer;
    [SerializeField] float skipDelayTime = 0.3f;

    [Header("Extra")]
    [SerializeField] GameObject autoPlaying;

    private List<DialogueData> dialogues = new();
    private DialogueData curDialogue;
    private int index = 0;

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
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        ChangeMode(DialogueMode.None);
        typer.Clear();
        dialogues.Clear();
        curDialogue = null;

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
        index++;

        if (index < dialogues.Count)
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

        ShowLine(curDialogue);
    }

    private void ShowPreviousLine()
    {
        index--;
        if (index < 0)
        {
            index = 0;
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
        characterName.text = data.name;
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
        typer.OnEnd -= ShowNextLine;
    }
    #endregion

    #region 유니티 전용
#if UNITY_EDITOR
    private void Reset()
    {
        skipBtn = transform.FindChild<Button>("Btn_Skip");
        autoBtn = transform.FindChild<Button>("Btn_Auto");
        backlogBtn = transform.FindChild<Button>("Btn_Backlog");
        optionBtn = transform.FindChild<Button>("Btn_Option");
        dialogueWindowBtn = transform.FindChild<Button>("Panel_Dialogue");

        portrait = transform.FindChild<Image>("Portrait");
        characterName = transform.FindChild<TMP_Text>("Text_Name");
        typer = transform.FindChild<DialogueTyper>("Text_Dialogue");

        autoPlaying = transform.FindChild<TMP_Text>("Text_AutoPlaying").gameObject;
    }

    private void OnValidate()
    {
        skipDelay = new WaitForSecondsRealtime(skipDelayTime);
    }
#endif
    #endregion
}
