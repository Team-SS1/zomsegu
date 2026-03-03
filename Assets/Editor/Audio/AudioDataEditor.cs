using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(AudioData))]
public class AudioDataEditor : Editor
{
    private ReorderableList list;

    private GUIContent playIcon;
    private GUIContent stopIcon;

    private static AudioSource previewSource;

    private void OnEnable()
    {
        playIcon = EditorGUIUtility.IconContent("PlayButton");
        stopIcon = EditorGUIUtility.IconContent("PreMatQuad");

        InitReorderableList();
        EnsurePreviewSource();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private void EnsurePreviewSource()
    {
        if (previewSource != null) return;

        GameObject go = EditorUtility.CreateGameObjectWithHideFlags(
            "AudioPreview",
            HideFlags.HideAndDontSave,
            typeof(AudioSource)
        );

        previewSource = go.GetComponent<AudioSource>();
    }

    private void InitReorderableList()
    {
        SerializedProperty listProp = serializedObject.FindProperty("audioEntries");

        list = new ReorderableList(serializedObject, listProp, true, true, true, true);

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Audio Entries");
        };

        // 🔹 높이 자동 계산
        list.elementHeightCallback = index =>
        {
            var element = listProp.GetArrayElementAtIndex(index);
            var volumeProp = element.FindPropertyRelative("volume");

            float volumeHeight = EditorGUI.GetPropertyHeight(volumeProp);
            return Mathf.Max(EditorGUIUtility.singleLineHeight, volumeHeight) + 6;
        };

        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = listProp.GetArrayElementAtIndex(index);
            var clipProp = element.FindPropertyRelative("audioClip");
            var volumeProp = element.FindPropertyRelative("volume");

            rect.y += 3;

            float spacing = 4f;
            float buttonWidth = 28f;

            float totalButtonWidth = buttonWidth * 2 + spacing * 2;
            float remainingWidth = rect.width - totalButtonWidth - spacing;

            float clipWidth = remainingWidth * 0.6f;
            float volumeWidth = remainingWidth * 0.4f;

            float volumeHeight = EditorGUI.GetPropertyHeight(volumeProp);

            Rect clipRect = new Rect(rect.x, rect.y, clipWidth, EditorGUIUtility.singleLineHeight);
            Rect volumeRect = new Rect(clipRect.xMax + spacing, rect.y, volumeWidth, volumeHeight);
            Rect playRect = new Rect(volumeRect.xMax + spacing, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight);
            Rect stopRect = new Rect(playRect.xMax + spacing, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);
            EditorGUI.PropertyField(volumeRect, volumeProp, GUIContent.none);

            if (GUI.Button(playRect, playIcon))
                PlayClip(clipProp.objectReferenceValue as AudioClip, volumeProp.floatValue);

            if (GUI.Button(stopRect, stopIcon))
                StopAll();
        };
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null) return;

        previewSource.Stop();
        previewSource.clip = clip;
        previewSource.volume = volume;
        previewSource.Play();

    }

    private void StopAll()
    {
        previewSource.Stop();
    }
}