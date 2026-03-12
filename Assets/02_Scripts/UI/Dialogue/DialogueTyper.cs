using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueTyper : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text target;

    [Header("Typing")]
    [SerializeField] private float charInterval = 0.03f;    // 글자 간격
    [SerializeField] private bool useUnscaledTime = true;   // 컷신에서 TimeScale=0일 때 대비

    private Coroutine typingCo;
    private string fullText;
    private bool isTyping;

    public bool IsTyping => isTyping;

    // 외부에서 한 줄 재생
    public void PlayLine(string text)
    {
        fullText = text ?? "";
        if (typingCo != null) StopCoroutine(typingCo);

        target.text = fullText;
        target.maxVisibleCharacters = 0;

        typingCo = StartCoroutine(TypeRoutine());
    }

    // 타이핑 중이면 즉시 완성, 아니면 무시(또는 다음 대사로 넘기는 로직을 밖에서)
    public void SkipOrComplete()
    {
        if (!isTyping) return;
        Complete();
    }

    public void Clear()
    {
        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = null;

        isTyping = false;
        fullText = "";
        target.text = "";
        target.maxVisibleCharacters = 0;
    }

    private void Complete()
    {
        isTyping = false;

        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }

        // TMP는 RichText 태그 포함 문자열 길이와 실제 표시 글자 수가 다르므로,
        // textInfo.characterCount(표시 가능한 실제 글자 수)를 사용.
        target.ForceMeshUpdate();
        target.maxVisibleCharacters = target.textInfo.characterCount;
    }

    private IEnumerator TypeRoutine()
    {
        isTyping = true;

        WaitForSecondsRealtime waitForSecondsRealtime = new(charInterval);
        WaitForSeconds waitForSeconds = new(charInterval);

        target.ForceMeshUpdate();
        int totalVisible = target.textInfo.characterCount;

        int visibleCount = 0;
        while (visibleCount < totalVisible)
        {
            visibleCount++;
            target.maxVisibleCharacters = visibleCount;

            if (useUnscaledTime)
                yield return waitForSecondsRealtime;
            else
                yield return waitForSeconds;
        }

        isTyping = false;
        typingCo = null;
    }

    #region 유니티 전용
#if UNITY_EDITOR
    private void Reset()
    {
        target = GetComponent<TMP_Text>();
        if (target == null)
        {
            Logger.LogError("target 없음");
        }
    }
#endif
    #endregion
}
