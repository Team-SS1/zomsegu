namespace AudioEnum
{
    public enum AudioCategory
    {
        Bgm,
        Sfx
    }

    public enum AudioName
    {
        // === BGM ===
        Title = 0,

        // === SFX ===
        // 남자 좀비
        Zombie_Male_Aggro = 100,
        Zombie_Male_Scratch,
        Zombie_Male_Hit,
        Zombie_Male_Move,

        // 여자 좀비
        Zombie_Female_Aggro,
        Zombie_Female_Scratch,
        Zombie_Female_Hit,
        Zombie_Female_Move,

        // 공통 좀비
        Zombie_Common_Bite,
        Zombie_Common_Idle,
        Zombie_Common_Eating,

        // 자동차 소리
        Car_Opened,
        Car_Closed,
        Car_BurglarAlarm_30sec,
        Car_Crash,
        Car_PassingBy,

        //아이템 먹는 소리
        Item_Drinking,
        Item_Eating,

        // 플레이어 펀치소리
        Player_Punch_Miss,
        Player_Punch_Hit_Enemy,
        Player_Punch_Hit_Collision,
        Player_Punch_Hit_Object,

        // 무기 나무타입 무기소리(반드시 오디오테이블 참고할 것)
        Player_Wood_Miss,
        Player_Wood_Hit_Enemy,
        Player_Wood_Hit_Collision,
        Player_Wood_Hit_Object,

        // 무기 쇠타입 무기소리(반드시 오디오테이블 참고할 것)
        Player_Iron_Miss,
        Player_Iron_Hit_Enemy,
        Player_Iron_Hit_Collision,
        Player_Iron_Hit_Object,

        // 무기 일반원거리타입 무기소리(반드시 오디오테이블 참고할 것)
        Player_Projectile_Hit_Enemy,
        Player_Projectile_Hit_Collision,
        Player_Projectile_Hit_Object,

        // 무기 유리원거리타입 무기소리(반드시 오디오테이블 참고할 것)
        Player_GlassProjectile_Hit_EnemyCollision,
        Player_GlassProjectile_Hit_Object,

        // 무기 골프채타입 무기소리(반드시 오디오테이블 참고할 것)
        Player_Golfclub_Miss,
        Player_Golfclub_Hit_Object,

        // 무기 칼타입 무기소리(반드시 오디오테이블 참고할 것)
        Player_Knife_Hit_Enemy,
        Player_Knife_Hit_Collision,
        Player_Knife_Hit_Object,

        // 무기 기타타입 무기소리(반드시 오디오테이블 참고할 것)
        Player_Misc_Hit_EnemyCollision,

        //무기, 방어구 파괴(반드시 오디오테이블 참고할 것)
        Player_Wood_Destroy,
        Player_Iron_Destroy,
        Player_Armor_Destroy,

        // NPC 소리
        NPC_Male_Runaway,
        NPC_Female_Runaway,
        NPC_Male_Scream,
        NPC_Female_Scream,

        // 휴대전화 소리
        CellPhone_Vibrate,

        // 연출용
        Car_Shaking,
        Zombie_Scream,

        // === Test ===
        Test_Bgm,
        Test_Sfx,
    }
}