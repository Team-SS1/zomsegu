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

    [SerializeField] private Vector2 randomPitch = new(1f, 1f);
    [SerializeField] private Vector2 randomVolume = new(1f, 1f);
    [SerializeField] private float cooldown = 0.05f;

    [SerializeField] private List<AudioVariation> audioVariations;

    public AudioCategory AudioCategory => audioCategory;
    public bool Loop => loop;
    public bool Spatial => spatial;
    public AudioPriority Priority => priority;
    public float RandomPitch => Random.Range(randomPitch.x, randomPitch.y);
    public float RandomVolume => Random.Range(randomVolume.x, randomVolume.y);
    public float Cooldown => cooldown;
    public IReadOnlyList<AudioVariation> AudioVariations => audioVariations;

    public AudioVariation GetRandomVariation()
    {
        if (audioVariations == null || audioVariations.Count == 0)
        {
            return null;
        }

        return audioVariations.Random();
    }

    public AudioVariation GetVariation(int index)
    {
        if (0 <= index && index < audioVariations.Count)
        {
            return audioVariations[index];
        }

        return GetRandomVariation();
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
public class AudioVariation
{
    [SerializeField] private AudioClip audioClip;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    public AudioClip AudioClip => audioClip;
    public float Volume => volume;
}