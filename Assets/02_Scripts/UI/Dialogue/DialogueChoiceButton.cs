using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChoiceButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text text;

    private int id;

    public void Init(int id, string text)
    {
        this.id = id;
        this.text.text = text;

        // text 길이에 맞게 width 조정하기

    }

    // hover 또는 select 시 폰트 수정

}
