using UnityEngine;

public class Test_DialogueSystem : MonoBehaviour
{
    // todo: 나중에 ui manager 만들어서 관리
    [SerializeField] UIDialogue ui;

    private void Start()
    {
        //AudioManager.Instance.PlayBgm(AudioEnum.AudioName.Test_Bgm, fadeDuration: 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Logger.Log("trigger enter");
        ui.StartDialogue(1);
    }
}
