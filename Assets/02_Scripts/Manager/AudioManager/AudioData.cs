using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[CreateAssetMenu(fileName = "AudioData", menuName = "SO/Audio/AudioData")]
public class AudioData : ScriptableObject
{
    [SerializeField] private AudioEnum.AudioType audioType;
    [SerializeField] private List<AudioClip> audioClips;

    public AudioEnum.AudioType AudioType => audioType;
    public IReadOnlyList<AudioClip> AudioClips => audioClips;

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

        if (System.Enum.TryParse(folderName, out AudioEnum.AudioType parsed))
        {
            if (audioType == parsed) return;

            audioType = parsed;
            EditorUtility.SetDirty(this);
        }
    }
#endif
    #endregion
}