using UnityEngine;

public class Test_StageSystem : MonoBehaviour
{
    public void Example_SpawnMap()
    {
        StageManager.Instance.SpawnStage(0);
    }

    public void Example_DespawnMap()
    {
        StageManager.Instance.DespawnStage();
    }

    public void Example_RespawnMap()
    {
        StageManager.Instance.RespawnStage();
    }
}
