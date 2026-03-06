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
    [SerializeField] private List<AudioEntry> audioEntries;

    public AudioCategory AudioCategory => audioCategory;
    public List<AudioEntry> AudioEntries => audioEntries;

    #region 에디터 전용
#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.delayCall += TrySetAudioType;
    }

    /// <summary>
    /// 폴더 이름으로 audioType 자동 설정
    /// </summary>
    private void TrySetAudioType()
    {
        EditorApplication.delayCall -= TrySetAudioType;

        string path = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
            return;

        string folderName = Path.GetFileName(Path.GetDirectoryName(path));

        if (System.Enum.TryParse(folderName, out AudioEnum.AudioCategory parsed))
        {
            if (audioCategory == parsed) return;

            audioCategory = parsed;
            EditorUtility.SetDirty(this);
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