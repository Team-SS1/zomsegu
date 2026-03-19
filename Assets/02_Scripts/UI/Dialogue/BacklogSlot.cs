using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BacklogSlot : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text speaker;
    [SerializeField] private TMP_Text dialogueText;

#if UNITY_EDITOR
    private void Reset()
    {
        portrait = transform.FindChild<Image>("Portrait");
        speaker = transform.FindChild<TMP_Text>("Text_Speaker");
        dialogueText = transform.FindChild<TMP_Text>("Text_Dialogue");
    }
#endif
}
