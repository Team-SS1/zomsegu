using System.Collections.Generic;

[System.Serializable]
public class DialogueData
{
    // todo: 필요한 거 더 입력하기
    public int id;
    public string portrait;
    public string name;
    public string text;
    public List<int> choiceIds;
    public int nextDialogueId = -1;

    public static Dictionary<int, DialogueData> tableDic = new()
    {
        { 1, new DialogueData()
            {
                id = 1,
                name = "1",
                text ="지금 이 순간이 얼마나 작고 평범해 보이더라도 괜찮다. 대부분의 변화는 아주 사소한 결심에서 시작되고, 그 결심이 쌓여 결국 큰 흐름이 된다. 오늘 조금 더 배우고, 조금 더 버티고, 조금 더 앞으로 나아갔다면 이미 충분히 잘하고 있는 것이다. 길이 멀어 보일수록 속도를 재촉하기보다 방향을 잃지 않는 것이 더 중요하다. 결국 끝까지 걸어간 사람만이 자신이 얼마나 멀리 왔는지 알게 된다.",
                nextDialogueId = 2,
            }
        },
        { 2, new DialogueData()
            {
                id = 2,
                name = "2",
                text = "처음에는 누구나 서툴다. 중요한 것은 얼마나 빨리 잘하느냐가 아니라, 포기하지 않고 계속 시도하느냐이다. 실수는 방향을 수정하는 과정일 뿐 실패가 아니다.",
                nextDialogueId = 3,
            }
        },
        { 3, new DialogueData()
            {
                id = 3,
                name = "3",
                text = "생각보다 많은 일이 천천히 진행된다. 눈에 띄는 변화가 없더라도 과정이 멈춘 것은 아니다. 시간이 쌓이면 결국 결과로 드러난다.",
                nextDialogueId = 5,
            }
        },
        { 4, new DialogueData()
            {
                id = 4,
                name = "4",
                text = "어떤 선택이 맞는지 확신할 수 없을 때도 있다. 그래도 선택을 미루기보다 한 걸음 내딛는 편이 낫다. 멈춰 있는 동안에는 아무 것도 바뀌지 않는다.",
                nextDialogueId = 6,
            }
        },
        {5,  new DialogueData()
            {
                id = 5,
                name = "5",
                text = "지금 하는 작은 노력들이 당장은 의미 없어 보일 수도 있다. 하지만 나중에 돌아보면 그 순간들이 방향을 바꾼 계기였다는 것을 알게 된다.",
            }
        },
        {6, new DialogueData()
            {
                id = 6,
                name = "6",
                text = "멀리 있는 목표만 보면 지치기 쉽다. 그래서 사람은 보통 하루 단위로 앞으로 나아간다. 오늘 할 일을 해내는 것만으로도 충분한 진전이다.",
            }
        }
    };
}