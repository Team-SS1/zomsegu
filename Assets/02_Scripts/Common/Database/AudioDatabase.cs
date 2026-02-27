using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "SO/Database/AudioDatabase")]
public class AudioDatabase : SoDatabase
{
    #region 에디터 전용
#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        List.Clear();
        AssetLoader.FindAndLoadAllByType<AudioData>().ForEach(AudioClip => List.Add(AudioClip));
        List.Sort((a, b) =>
        {
            if (((AudioData)a).AudioType != ((AudioData)b).AudioType)
            {
                return ((AudioData)a).AudioType.CompareTo(((AudioData)b).AudioType);
            }
            return a.name.CompareTo(b.name);
        });
    }

    public void CollectAudioData()
    {
        List.Clear();

        AssetLoader.FindAndLoadAllByType<AudioData>()
            .ForEach(data => List.Add(data));

        List.Sort((a, b) =>
        {
            AudioData aData = a as AudioData;
            AudioData bData = b as AudioData;

            if (aData.AudioType != bData.AudioType)
                return aData.AudioType.CompareTo(bData.AudioType);

            return a.name.CompareTo(b.name);
        });

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
    #endregion
}
