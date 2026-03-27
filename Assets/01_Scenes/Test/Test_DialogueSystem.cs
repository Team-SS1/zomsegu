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
            UIManager.Instance.OpenUI<TestPanel>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UIManager.Instance.OpenUI<TestPopup_1>();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UIManager.Instance.OpenUI<TestPopup_2>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var ui = UIManager.Instance.OpenUI<UIDialogue>();
        ui.StartDialogue(1);
    }
}
