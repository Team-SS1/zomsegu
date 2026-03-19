using UnityEngine;

public class Test_DialogueSystem : MonoBehaviour
{
    private void Start()
    {
        //AudioManager.Instance.PlayBgm(AudioEnum.AudioName.Test_Bgm, fadeDuration: 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var ui = UIManager.Instance.GetOrCreateUI<UIDialogue>(true);
        ui.StartDialogue(1);
    }
}
