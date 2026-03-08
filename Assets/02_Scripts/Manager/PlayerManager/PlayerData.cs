using PlayerEnum;

/// <summary>
/// PlayerManager가 관리하는 Player Data
/// </summary>
[System.Serializable]
public class PlayerData
{
    public PlayerType Type { get; private set; }
    public int ID { get; private set; }
    public PlayerStat Stat { get; private set; }

    public PlayerData(PlayerType type, int id, PlayerStat stat)
    {
        Type = type;
        ID = id;
        Stat = stat;
    }
}