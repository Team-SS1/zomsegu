using UnityEngine;

public class Test_DialogueSystem : MonoBehaviour
{
    private void Start()
    {
        //AudioManager.Instance.PlayBgm(AudioEnum.AudioName.Test_Bgm, fadeDuration: 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UIConfirmPopup ui = UIManager.Instance.OpenPopup<UIConfirmPopup>();
            ui.Register("open pop up ui", null);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var ui = UIManager.Instance.GetPanel<UIDialogue>(true);
        ui.StartDialogue(1);
    }
}
