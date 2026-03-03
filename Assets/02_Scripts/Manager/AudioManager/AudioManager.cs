using AudioEnum;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : GlobalSingleton<AudioManager>
{
    #region 필드
    [SerializeField] private AudioDatabase audioDatabase;

    private readonly Dictionary<AudioName, List<AudioEntry>> bgmDict = new();
    private readonly Dictionary<AudioName, List<AudioEntry>> sfxDict = new();
    #endregion

    #region Unity API
    protected override void Awake()
    {
        base.Awake();

        InitializeDict();
    }
    #endregion

    #region 초기화
    /// <summary>
    /// DI용 초기화 메서드
    /// </summary>
    /// <param name="database"></param>
    public void Initialize(AudioDatabase database)
    {
        audioDatabase = database;
    }

    public void InitializeDict()
    {
        audioDatabase.GetDatabase<AudioData>().ForEach(audioData =>
        {
            if (!Enum.TryParse(audioData.name, out AudioEnum.AudioName audioName))
            {
                Logger.LogWarning($"{audioData.name} 이름 오류");
            }

            switch (audioData.AudioType)
            {
                case AudioEnum.AudioType.Bgm:
                    bgmDict.Add(audioName, audioData.AudioEntries);
                    break;
                case AudioEnum.AudioType.Sfx:
                    sfxDict.Add(audioName, audioData.AudioEntries);
                    break;
            }
        });
    }
    #endregion

    #region 에디터 전용
#if UNITY_EDITOR
    private void Reset()
    {
        audioDatabase = AssetLoader.FindAndLoadByName<AudioDatabase>("AudioDatabase");
    }
#endif
    #endregion
}
