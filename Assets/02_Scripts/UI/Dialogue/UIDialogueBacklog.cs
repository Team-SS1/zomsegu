using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct DialogueBacklog
{
    public Sprite portrait;
    public string speaker;
    public string dialogueText;
    public string[] choiceTexts;
    public bool isPlayer;
}

public class UIDialogueBacklog : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private BacklogSlot backlogLeftPrefab;
    [SerializeField] private BacklogSlot backlogRightPrefab;
    [SerializeField] private Button closeBtn;

    private UIDialogue uiDialogue;
    private List<BacklogSlot> backlogs = new();

    public void Init(UIDialogue uiDialogue)
    {
        this.uiDialogue = uiDialogue;
    }

    private void Start()
    {
        closeBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            uiDialogue.ChangeMode(DialogueEnum.DialogueMode.None);
        });
    }

    private void OnDestroy()
    {
        closeBtn.onClick.RemoveAllListeners();
    }

    public void AddBackLog(in DialogueBacklog backlog)
    {
        BacklogSlot slot = Instantiate((backlog.isPlayer ? backlogLeftPrefab : backlogRightPrefab), root);

        string text = backlog.dialogueText;
        if (backlog.choiceTexts != null)
        {
            foreach (string choiceText in backlog.choiceTexts)
            {
                text += $"\n{choiceText}";
            }
        }

        slot.Init(backlog.portrait, backlog.speaker, text);
        backlogs.Add(slot);
    }

    public void ResetLogs()
    {
        for (int i = backlogs.Count - 1; i >= 0; i--)
        {
            Destroy(backlogs[i].gameObject);
        }
        backlogs.Clear();
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        root = transform.FindChild<RectTransform>("Content");
        backlogLeftPrefab = AssetLoader.FindAndLoadByName("UI_BacklogSlot_Left").GetComponent<BacklogSlot>();
        backlogRightPrefab = AssetLoader.FindAndLoadByName("UI_BacklogSlot_Right").GetComponent<BacklogSlot>();
        closeBtn = transform.FindChild<Button>("Btn_Close");
    }
#endif
    #endregion
}
