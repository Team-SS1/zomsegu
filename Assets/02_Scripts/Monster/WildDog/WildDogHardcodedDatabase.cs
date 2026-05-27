using MonsterEnum;
using System.Collections.Generic;

public static class WildDogHardcodedDatabase
{
    private static readonly Dictionary<WildDogType, WildDogStats> table =
        new Dictionary<WildDogType, WildDogStats>
        {
            { WildDogType.Leader, WildDogStats.CreateLeader() },
            { WildDogType.Grunt, WildDogStats.CreateGrunt() }
        };

    public static WildDogStats Get(WildDogType type)
    {
        return table[type];
    }
}