using UnityEngine;

public class Test_DialogueSystem : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance.PlayBgm(AudioEnum.AudioName.Test_Bgm, fadeDuration: 1f);
    }
}
