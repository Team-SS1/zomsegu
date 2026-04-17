using UnityEngine;
using PlayerEnum;

/// <summary>
/// SHIN / HAN의 고정 스탯 데이터를 보관하는 매니저
/// 외부에서는 PlayerManager를 통해 직접 Stat에 접근할 수 있음
/// 예) PlayerManager.Instance.Player_HAN.Stat.BaseAttack
/// </summary>
public class PlayerDataManager : GlobalSingleton<PlayerDataManager>
{
    public PlayerData Player_HAN { get; private set; }
    public PlayerData Player_SHIN { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 게임 시작시 SHIN / HAN 데이터를 미리 생성해서 보관
    /// </summary>
    public void Init()
    {
        Player_SHIN = CreatePlayerData(PlayerType.Player_SHIN);
        Player_HAN = CreatePlayerData(PlayerType.Player_HAN);

        // 임시 세팅
        // 신재현
        Player_SHIN.Inventory.TryAddNewInstance(30002);
        Player_SHIN.Inventory.TryAddStack(31001, 10);
        Player_SHIN.Inventory.TryAddNewInstance(32000);
        Player_SHIN.Inventory.TryAddNewInstance(33001);
        Player_SHIN.Inventory.TryAddNewInstance(34000);
        Player_SHIN.Inventory.TryAddNewInstance(35101);
        Player_SHIN.Inventory.TryAddStack(36000, 8);
        Player_SHIN.Inventory.TryAddNewInstance(30002);
        Player_SHIN.Inventory.TryAddStack(31001, 10);
        Player_SHIN.Inventory.TryAddNewInstance(32000);
        Player_SHIN.Inventory.TryAddNewInstance(33001);
        Player_SHIN.Inventory.TryAddNewInstance(34000);
        Player_SHIN.Inventory.TryAddNewInstance(35101);
        Player_SHIN.Inventory.TryAddStack(36000, 10);

        // 한세희
        Player_HAN.Inventory.TryAddNewInstance(35200);
        Player_HAN.Inventory.TryAddNewInstance(35200);
        
    }

    /// <summary>
    /// PlayerType을 받아 해당 캐릭터의 PlayerData를 생성
    /// </summary>
    private PlayerData CreatePlayerData(PlayerType type)
    {
        int id = GetPlayerID(type);
        PlayerStat stat = GetPlayerStat(id);

        if (stat == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"CreatePlayerData failed. type = {type}, id = {id}");
#endif
            return null;
        }

        return new PlayerData(type, id, stat);
    }

    /// <summary>
    /// PlayerType에 맞는 DataTable ID 반환
    /// </summary>
    public int GetPlayerID(PlayerType type)
    {
        switch (type)
        {
            case PlayerType.Player_SHIN:
                return GameConstants.PlayerID_A;
            case PlayerType.Player_HAN:
                return GameConstants.PlayerID_B;
            default:
#if UNITY_EDITOR
                Debug.LogError($"Invalid PlayerType : {type}");
#endif
                return 0;
        }
    }

    /// <summary>
    /// ID를 통해 PlayerStat 테이블 데이터 반환
    /// </summary>
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

    /// <summary>
    /// 타입으로 PlayerData를 직접 가져오고 싶을 때 사용
    /// </summary>
    public PlayerData GetPlayerData(PlayerType type)
    {
        switch (type)
        {
            case PlayerType.Player_SHIN:
                return Player_SHIN;
            case PlayerType.Player_HAN:
                return Player_HAN;
            default:
                return null;
        }
    }
}