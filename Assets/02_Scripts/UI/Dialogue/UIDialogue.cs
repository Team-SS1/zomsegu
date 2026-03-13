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
    private int index = 0;

    // dialogue mode
    private DialogueMode curMode = DialogueMode.None;
    private NavigationDirection curDirection = NavigationDirection.Next;

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
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        SetMode(DialogueMode.None);
        typer.Clear();

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
        SetMode(curMode != DialogueMode.Skip ? DialogueMode.Skip : DialogueMode.None);
    }

    private void OnClickAutoBtn()
    {
        SetMode(curMode != DialogueMode.Auto ? DialogueMode.Auto : DialogueMode.None);
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
        PlayLineManunal();
        EventSystem.current?.SetSelectedGameObject(null);   // 버튼 캐싱 삭제
    }
    #endregion

    #region 대화 관리
    public void StartDialogues(List<DialogueData> dialogues)
    {
        gameObject.SetActive(true);
        this.dialogues = dialogues;
        index = 0;
        PlayLineManunal();
    }

    private void PlayLineManunal()
    {
        SetMode(DialogueMode.None);

        if (typer.IsTyping)
        {
            typer.SkipOrComplete();
            return;
        }

        PlayNextLine();
    }

    private void PlayNextLine()
    {
        if (dialogues.Count <= index)
        {
            gameObject.SetActive(false);
            return;
        }

        DialogueData data = dialogues[index];
        characterName.text = data.name;
        typer.PlayLine(data.text);
        index++;
    }


    private void PlayPrevLine()
    {
        index--;
        if (index < 0)
        {
            index = 0;
            return;
        }

        DialogueData data = dialogues[index];
        characterName.text = data.name;
        typer.PlayLine(data.text);
    }

    private IEnumerator SkipDialogue()
    {
        while (true)
        {
            yield return skipDelay;
            PlayNextLine();
            typer.SkipOrComplete();
        }
    }
    #endregion

    #region Input 이벤트
    private void OnNext(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SetDirection(NavigationDirection.Next);
            PlayLineManunal();
        }
    }

    private void OnPrev(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SetDirection(NavigationDirection.Prev);
            SetMode(DialogueMode.None);
            if (typer.IsTyping) typer.SkipOrComplete();
            PlayPrevLine();
        }
    }

    private void OnSkip(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SetMode(DialogueMode.Skip);
        }
        else if (context.canceled)
        {
            SetMode(DialogueMode.None);
        }
    }
    #endregion

    #region 모드 설정
    private void SetMode(DialogueMode mode)
    {
        if (curMode == mode) return;

        curMode = mode;

        ResetMode();

        switch (curMode)
        {
            case DialogueMode.Skip:
                skipCoroutine = StartCoroutine(SkipDialogue());
                break;
            case DialogueMode.Auto:
                autoPlaying.SetActive(true);
                if (!typer.IsTyping) PlayNextLine();
                typer.OnEnd += PlayNextLine;
                break;
            default:
                break;
        }
    }

    private void ResetMode()
    {
        if (skipCoroutine != null)
        {
            StopCoroutine(skipCoroutine);
            skipCoroutine = null;
        }

        autoPlaying.SetActive(false);
        typer.OnEnd -= PlayNextLine;
    }

    private void SetDirection(NavigationDirection direction)
    {
        if (curDirection != direction)
        {
            if (curDirection == NavigationDirection.Next)
            {
                index--;
            }
            else
            {
                index++;
            }
            curDirection = direction;
        }

        if (index < 0)
        {
            index = 0;
        }
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
