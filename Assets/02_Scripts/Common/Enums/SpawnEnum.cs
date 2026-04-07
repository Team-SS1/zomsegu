namespace SpawnEnum
{
    public enum SpawnType
    {
        Player,
        Monster,
        NPC
    }
    public enum SpawnID
    {
        /* - Player - */
        Player,
        partner,

        /* - Monster - */
        ElderFemale,
        ElderMale,
        AdultFemale,
        AdultMale,
        YoungAdultFemale,
        YoungAdultMale,
        Athlete,
        Police,
        Soldier,
        Firefighter,

        /* -NPC - */
    }

    public enum FixedSpawnPointKey
    {
        Chapcter1PlayerSpawnPoint,
        Chapcter1PlayerSavePoint,

        Chapcter2PlayerSpawnPoint,
        Chapcter2PlayerSavePoint,

        Chapcter3PlayerSpawnPoint,
        Chapcter3PlayerSavePoint
    }
}