using System.Collections.Generic;

[System.Serializable]
public class DialogueChoiceData
{
    public int id;
    public string text;
    public int nextDialogueId;

    public static Dictionary<int, DialogueChoiceData> tableDic = new()
    {
        { 1, new DialogueChoiceData() {
            id = 1,
            text = "1번을 고른다",
            nextDialogueId = 3,
        } },
        { 2, new DialogueChoiceData()
        {
            id = 2,
            text = "2번을 고른다",
            nextDialogueId = 4,
        } }
    };
}