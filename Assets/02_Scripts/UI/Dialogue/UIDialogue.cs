using TMPro;
using UnityEngine;
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

    [Header("Extra")]
    [SerializeField] GameObject autoPlaying;

    private bool autoMode = false;

    #region Unity API
    private void Awake()
    {
        skipBtn.onClick.AddListener(OnClickSkipBtn);
        autoBtn.onClick.AddListener(OnClickAutoBtn);
        backlogBtn.onClick.AddListener(OnClickBacklogBtn);
        optionBtn.onClick.AddListener(OnClickOptionBtn);
        dialogueWindowBtn.onClick.AddListener(OnClickDialogueWindowBtn);
    }
    #endregion

    #region 버튼 이벤트
    private void OnClickSkipBtn()
    {

    }

    private void OnClickAutoBtn()
    {
        SetAutoMode(!autoMode);
    }

    private void OnClickBacklogBtn()
    {

    }

    private void OnClickOptionBtn()
    {

    }

    private void OnClickDialogueWindowBtn()
    {
        if (autoMode)
        {
            SetAutoMode(false);
        }
    }
    #endregion

    private void SetAutoMode(bool value)
    {
        autoMode = value;
        autoPlaying.SetActive(autoMode);
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
#endif
    #endregion
}
