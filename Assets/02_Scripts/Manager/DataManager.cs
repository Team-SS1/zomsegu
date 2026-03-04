using System.Collections;
using System.Collections.Generic;
using PlayerEnum;
using UnityEngine;

public class DataManager : GlobalSingleton<DataManager>
{
    protected override void Awake()
    {
        base.Awake();
        DataTableLoader.LoadTables();
    }
    public PlayerStat GetPlayerStat(int id)
    {
        if (!PlayerStat.tableDic.TryGetValue(id, out var stat))
        {
#if UNITY_EDITOR
            Debug.LogError($"PlayerStat not found. ID = {id}");
#endif
            return null;
        }
        return stat;
    }
    public int GetPlayerID(PlayerType type)
    {
        switch (type)
        {
            case PlayerType.Player_SHIN:
                return GameConstants.PlayerID_A;
            case PlayerType.Player_HAN:
                return GameConstants.PlayerID_B;
            default:
                return 0;
        }
    }
}
