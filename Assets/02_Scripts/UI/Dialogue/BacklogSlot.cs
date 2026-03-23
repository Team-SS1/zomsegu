using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BacklogSlot : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text speaker;
    [SerializeField] private TMP_Text dialogueText;

    public void Init(Sprite portrait, string speaker, string text)
    {
        this.portrait.sprite = portrait;
        this.speaker.text = speaker;
        dialogueText.text = text;
    }

    public void SetDimmed()
    {
        dialogueText.color = Color.gray;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        portrait = transform.FindChild<Image>("Portrait");
        speaker = transform.FindChild<TMP_Text>("Text_Speaker");
        dialogueText = transform.FindChild<TMP_Text>("Text_Dialogue");
    }
#endif
}
