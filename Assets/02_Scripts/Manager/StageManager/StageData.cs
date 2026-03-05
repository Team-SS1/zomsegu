using UnityEngine;

[CreateAssetMenu(fileName = "new StageData", menuName = "SO/Stage/Stage Data")]
public class StageData : ScriptableObject
{
    [SerializeField] private int stageId;
    [SerializeField] private GameObject mapPrefab;

    // todo: 시작 시각, 클리어 조건 추가

    public int StageId => stageId;
    public GameObject MapPrefab => mapPrefab;
}
