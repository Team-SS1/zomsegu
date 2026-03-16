using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChoiceButton : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float horizontalPadding = 16f;

    private Button button;
    private RectTransform rect;
    private DialogueChoiceData data;

    private void Awake()
    {
        button = GetComponent<Button>();
        rect = GetComponent<RectTransform>();
    }

    public void Init(DialogueChoiceData data, int no)
    {
        this.data = data;
        text.text = $"{no}. {data.text}";

        // text 길이에 맞게 width 조정하기
        float width = text.preferredWidth + horizontalPadding;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    // hover 또는 select 시 폰트 수정

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        text = GetComponentInChildren<TMP_Text>();
    }
#endif
    #endregion
}
