using System.Collections.Generic;
using UnityEngine;
using AudioEnum;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[CreateAssetMenu(fileName = "AudioData", menuName = "SO/Audio/AudioData")]
public class AudioData : ScriptableObject
{
    [SerializeField] private AudioCategory audioCategory;

    [SerializeField] private bool loop;
    [SerializeField] private bool spatial;

    [SerializeField] private AudioPriority priority;

    [Header("Clips")]
    [SerializeField] private List<AudioEntry> audioEntries;

    public AudioCategory AudioCategory => audioCategory;
    public bool Loop => loop;
    public bool Spatial => spatial;
    public AudioPriority Priority => priority;
    public List<AudioEntry> AudioEntries => audioEntries;

    public AudioEntry GetRandomEntry()
    {
        return audioEntries.Random();
    }

    public AudioEntry GetEntry(int index)
    {
        if (0 <= index && index < audioEntries.Count)
        {
            return audioEntries[index];
        }

        return GetRandomEntry();
    }

    #region 에디터 전용
#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.delayCall += SetAudioType;
        EditorApplication.delayCall += SetAudioSettings;
    }

    /// <summary>
    /// 폴더 이름으로 audioType 자동 설정
    /// </summary>
    private void SetAudioType()
    {
        EditorApplication.delayCall -= SetAudioType;

        string path = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
            return;

        string folderName = Path.GetFileName(Path.GetDirectoryName(path));

        if (System.Enum.TryParse(folderName, out AudioCategory parsed))
        {
            if (audioCategory == parsed) return;

            audioCategory = parsed;
            EditorUtility.SetDirty(this);
        }
    }

    private void SetAudioSettings()
    {
        EditorApplication.delayCall -= SetAudioSettings;

        switch (audioCategory)
        {
            case AudioCategory.Bgm:
                loop = true;
                priority = AudioPriority.Music;
                break;
            case AudioCategory.Gameplay:
                spatial = true;
                break;
        }
    }
#endif
    #endregion
}

[System.Serializable]
public class AudioEntry
{
    [SerializeField] private AudioClip audioClip;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    public AudioClip AudioClip => audioClip;
    public float Volume => volume;
}