using System.Collections.Generic;
using UnityEngine;

public struct DialogueBacklog
{
    public Sprite portrait;
    public string speaker;
    public string dialogueText;
    public string choiceText;
}

public class UIDialogueBacklog : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private BacklogSlot backlogPrefab;
    [SerializeField] private float padding = 30f;
    [SerializeField] private float spacing = 30f;

    private List<DialogueBacklog> backlogs = new();



    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        root = transform.FindChild<RectTransform>("Content");
        backlogPrefab = AssetLoader.FindAndLoadByName("UI_BacklogSlot").GetComponent<BacklogSlot>();
    }
#endif
    #endregion
}
