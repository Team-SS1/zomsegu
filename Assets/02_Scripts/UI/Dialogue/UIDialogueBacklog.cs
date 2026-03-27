using System.Collections;
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

public class UIDialogueBacklog : BaseUI
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform root;
    [SerializeField] private BacklogSlot backlogLeftPrefab;
    [SerializeField] private BacklogSlot backlogRightPrefab;
    [SerializeField] private Button closeBtn;

    private UIDialogue uiDialogue;
    private List<BacklogSlot> backlogSlots = new();

    private Coroutine scrollRoutine;

    private void Start()
    {
        uiDialogue = UIManager.Instance.GetUI<UIDialogue>();

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

    private void OnEnable()
    {
        scrollRoutine = StartCoroutine(CoOpenAndScrollBottom());
    }

    private void OnDisable()
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        backlogSlots.ForEach(slot => slot.gameObject.SetActive(false));
    }

    private IEnumerator CoOpenAndScrollBottom()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void AddBacklogs(List<DialogueBacklog> backlogs)
    {
        for (int i = 0; i < backlogs.Count; i++)
        {
            DialogueBacklog backlog = backlogs[i];

            if (backlogSlots.Count <= i)
            {
                BacklogSlot slot = Instantiate((backlog.isPlayer ? backlogLeftPrefab : backlogRightPrefab), root);
                backlogSlots.Add(slot);
            }

            SetSlot(backlogSlots[i], backlog);
        }

        if (backlogSlots.Count > 1)
        {
            backlogSlots[^2].SetDimmed();
        }
    }

    private void SetSlot(BacklogSlot slot, in DialogueBacklog backlog)
    {
        string text = backlog.dialogueText;
        if (backlog.choiceTexts != null)
        {
            foreach (string choiceText in backlog.choiceTexts)
            {
                text += $"\n{choiceText}";
            }
        }

        slot.Init(backlog.portrait, backlog.speaker, text);
    }

    public void ResetLogs()
    {
        for (int i = backlogSlots.Count - 1; i >= 0; i--)
        {
            Destroy(backlogSlots[i].gameObject);
        }
        backlogSlots.Clear();
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        scrollRect = transform.FindChild<ScrollRect>("BacklogScrollList");
        root = transform.FindChild<RectTransform>("Content");
        backlogLeftPrefab = AssetLoader.FindAndLoadByName("UI_BacklogSlot_Left").GetComponent<BacklogSlot>();
        backlogRightPrefab = AssetLoader.FindAndLoadByName("UI_BacklogSlot_Right").GetComponent<BacklogSlot>();
        closeBtn = transform.FindChild<Button>("Btn_Close");
    }
#endif
    #endregion
}
