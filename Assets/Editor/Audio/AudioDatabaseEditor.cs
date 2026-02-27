#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioDatabase))]
public class AudioDatabaseEditor : Editor
{
    private bool showBgm = true;
    private bool showSfx = true;

    private string searchText = "";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AudioDatabase db = (AudioDatabase)target;
        SerializedProperty listProp = serializedObject.FindProperty("list");

        DrawToolbar(db);
        EditorGUILayout.Space(8);

        List<AudioData> bgmList = new();
        List<AudioData> sfxList = new();

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            if (element.objectReferenceValue is AudioData data)
            {
                if (!string.IsNullOrEmpty(searchText) &&
                    !data.name.ToLower().Contains(searchText.ToLower()))
                    continue;

                if (data.AudioType == AudioEnum.AudioType.Bgm)
                    bgmList.Add(data);
                else
                    sfxList.Add(data);
            }
        }

        DrawGroup("BGM", bgmList, ref showBgm);
        DrawDivider();
        DrawGroup("SFX", sfxList, ref showSfx);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawToolbar(AudioDatabase db)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        searchText = GUILayout.TextField(
            searchText,
            EditorStyles.toolbarSearchField,
            GUILayout.ExpandWidth(true));

        if (!string.IsNullOrEmpty(searchText))
        {
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                searchText = "";
                GUI.FocusControl(null);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        if (GUILayout.Button("Auto Collect AudioData", GUILayout.Height(26)))
        {
            db.CollectAudioData();
            serializedObject.Update();
        }
    }

    private void DrawGroup(string title, List<AudioData> list, ref bool foldout)
    {
        EditorGUILayout.Space(6);

        GUIStyle container = new GUIStyle("HelpBox")
        {
            padding = new RectOffset(20, 10, 6, 8)
        };

        EditorGUILayout.BeginVertical(container);

        foldout = EditorGUILayout.Foldout(
            foldout,
            $"{title} ({list.Count})",
            true,
            EditorStyles.foldoutHeader
        );

        if (foldout)
        {
            EditorGUILayout.Space(4);

            foreach (var data in list)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(data, typeof(AudioData), false, GUILayout.ExpandWidth(true));

                int clipCount = data.AudioClips != null ? data.AudioClips.Count : 0;

                GUILayout.Label($"{clipCount} Clips",
                    EditorStyles.miniLabel,
                    GUILayout.Width(50));

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDivider()
    {
        EditorGUILayout.Space(6);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 0.6f));
        EditorGUILayout.Space(6);
    }
}
#endif