using System;
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
        HandleDragAndDrop();
        serializedObject.ApplyModifiedProperties();
    }

    #region 리스트 관리
    private void InitReorderableList()
    {
        SerializedProperty listProp = serializedObject.FindProperty("audioEntries");

        list = new ReorderableList(serializedObject, listProp, true, true, true, true);

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Audio Entries");
        };

        // 높이 자동 계산
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

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                SortEntries(list.serializedProperty);
                return; // 재정렬 후 다시 그리기
            }

            EditorGUI.PropertyField(clipRect, clipProp, GUIContent.none);
            EditorGUI.PropertyField(volumeRect, volumeProp, GUIContent.none);

            if (GUI.Button(playRect, playIcon))
                PlayClip(clipProp.objectReferenceValue as AudioClip, volumeProp.floatValue);

            if (GUI.Button(stopRect, stopIcon))
                StopAll();
        };

        list.onAddCallback = l =>
        {
            int index = l.serializedProperty.arraySize;
            l.serializedProperty.InsertArrayElementAtIndex(index);

            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("audioClip").objectReferenceValue = null;
            element.FindPropertyRelative("volume").floatValue = 1f;

            serializedObject.ApplyModifiedProperties();
        };
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;

        if (evt.type != EventType.DragPerform && evt.type != EventType.DragUpdated)
            return;

        Rect dropArea = GUILayoutUtility.GetLastRect();

        if (!dropArea.Contains(evt.mousePosition))
            return;

        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();

            SerializedProperty listProp = serializedObject.FindProperty("audioEntries");

            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is AudioClip clip)
                {
                    if (ContainsClip(listProp, clip))
                        continue;

                    int index = listProp.arraySize;
                    listProp.InsertArrayElementAtIndex(index);

                    var element = listProp.GetArrayElementAtIndex(index);
                    element.FindPropertyRelative("audioClip").objectReferenceValue = clip;
                    element.FindPropertyRelative("volume").floatValue = 1f;
                }
            }

            SortEntries(listProp);

            serializedObject.ApplyModifiedProperties();
            evt.Use();
        }
    }

    private bool ContainsClip(SerializedProperty listProp, AudioClip clip)
    {
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            var clipProp = element.FindPropertyRelative("audioClip");

            if (clipProp.objectReferenceValue == clip)
                return true;
        }
        return false;
    }

    private void SortEntries(SerializedProperty listProp)
    {
        var entries = new System.Collections.Generic.List<(AudioClip clip, float volume)>();

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            entries.Add((
                element.FindPropertyRelative("audioClip").objectReferenceValue as AudioClip,
                element.FindPropertyRelative("volume").floatValue
            ));
        }

        entries.Sort((a, b) =>
        {
            if (a.clip == null) return 1;
            if (b.clip == null) return -1;
            return string.Compare(a.clip.name, b.clip.name, StringComparison.Ordinal);
        });

        listProp.ClearArray();

        for (int i = 0; i < entries.Count; i++)
        {
            listProp.InsertArrayElementAtIndex(i);
            var element = listProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("audioClip").objectReferenceValue = entries[i].clip;
            element.FindPropertyRelative("volume").floatValue = entries[i].volume;
        }
    }
    #endregion

    #region clip preview

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
    #endregion
}