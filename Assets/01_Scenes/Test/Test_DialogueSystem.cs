using System.Collections.Generic;
using UnityEngine;

public class Test_DialogueSystem : MonoBehaviour
{
    // todo: 나중에 ui manager 만들어서 관리
    [SerializeField] UIDialogue ui;

    List<DialogueData> testList = new()
    {
        new DialogueData()
        {
            name = "아무개",
            text ="지금 이 순간이 얼마나 작고 평범해 보이더라도 괜찮다. 대부분의 변화는 아주 사소한 결심에서 시작되고, 그 결심이 쌓여 결국 큰 흐름이 된다. 오늘 조금 더 배우고, 조금 더 버티고, 조금 더 앞으로 나아갔다면 이미 충분히 잘하고 있는 것이다. 길이 멀어 보일수록 속도를 재촉하기보다 방향을 잃지 않는 것이 더 중요하다. 결국 끝까지 걸어간 사람만이 자신이 얼마나 멀리 왔는지 알게 된다."
        }
    };

    private void Start()
    {
        AudioManager.Instance.PlayBgm(AudioEnum.AudioName.Test_Bgm, fadeDuration: 1f);
        ui.SetDialogues(testList);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Logger.Log("trigger enter");
        ui.gameObject.SetActive(true);
    }
}
