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

    // skip
    private Coroutine skipCoroutine;
    private WaitForSecondsRealtime skipDelay;
    private bool skipMode = false;

    // auto
    private bool autoMode = false;
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
        mg.RemoveMaps(ActionMaps.Gameplay);
        mg.RemoveMaps(ActionMaps.UI);
        mg.AddMaps(ActionMaps.Dialogue);
    }

    private void Start()
    {
        InputManager mg = InputManager.Instance;
        mg.BindInput(ActionMaps.Dialogue, Actions.Next, OnNext);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        InputManager mg = InputManager.Instance;
        mg.AddMaps(ActionMaps.Gameplay);
        mg.AddMaps(ActionMaps.UI);
        mg.RemoveMaps(ActionMaps.Dialogue);

        SetAutoMode(false);
        SetSkipMode(false);
        typer.Clear();
    }
    #endregion

    #region 버튼 이벤트
    private void OnClickSkipBtn()
    {
        SetSkipMode(!skipMode);
    }

    private void OnClickAutoBtn()
    {
        SetAutoMode(!autoMode);
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
        SetAutoMode(false);
        SetSkipMode(false);

        if (typer.IsTyping)
        {
            typer.SkipOrComplete();
            return;
        }

        PlayLine();
    }

    private void PlayLine()
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

    private void OnNext(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PlayLineManunal();
        }
    }

    private IEnumerator SkipDialogue()
    {
        while (true)
        {
            PlayLine();
            typer.SkipOrComplete();
            yield return skipDelay;
        }
    }
    #endregion

    private void SetSkipMode(bool value)
    {
        if (skipMode == value) return;

        skipMode = value;

        if (skipCoroutine != null)
        {
            StopCoroutine(skipCoroutine);
            skipCoroutine = null;
        }

        if (skipMode)
        {
            SetAutoMode(false);
            skipCoroutine = StartCoroutine(SkipDialogue());
        }
    }

    private void SetAutoMode(bool value)
    {
        if (autoMode == value) return;

        autoMode = value;
        autoPlaying.SetActive(autoMode);

        if (autoMode)
        {
            SetSkipMode(false);
            if (!typer.IsTyping) PlayLine();
            typer.OnEnd += PlayLine;
        }
        else
        {
            typer.OnEnd -= PlayLine;
        }
    }

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
