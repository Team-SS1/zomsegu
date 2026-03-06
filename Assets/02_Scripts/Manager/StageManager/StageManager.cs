using System.Collections.Generic;
using UnityEngine;

public class StageManager : GlobalSingleton<StageManager>
{
    [SerializeField] private SoDatabase stageDatabase;

    private List<StageData> stages = new();

    private StageData activeStageData;
    private GameObject activeMap;

    protected override void Awake()
    {
        base.Awake();

        stages = stageDatabase.GetDatabase<StageData>();
    }

    /// <summary>
    /// 스테이지 스폰
    /// </summary>
    public void SpawnStage(int stageId)
    {
        if (activeStageData != null && activeStageData.StageId == stageId)
        {
            Logger.Log("동일 스테이지 스폰");
            return;
        }

        foreach (StageData data in stages)
        {
            if (data.StageId != stageId) { continue; }

            if (data.MapPrefab == null)
            {
                Logger.LogWarning("맵 프리팹 없음");
                return;
            }

            activeStageData = data;

            activeMap = Instantiate(activeStageData.MapPrefab);
            activeMap.transform.position = Vector3.zero;
        }
    }

    public void RespawnStage()
    {
        if (activeStageData == null) { return; }

        GameObject newGo = Instantiate(activeStageData.MapPrefab);
        newGo.transform.position = Vector3.zero;

        DespawnStage();

        activeMap = newGo;
    }

    /// <summary>
    /// 열려 있는 스테이지 go 파괴
    /// </summary>
    public void DespawnStage()
    {
        if (activeMap == null) { return; }

        activeStageData = null;

        Destroy(activeMap);
    }

#if UNITY_EDITOR
    private void Reset()
    {
        stageDatabase = AssetLoader.FindAndLoadByName<SoDatabase>("StageDatabase");
    }
#endif
}
