using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioData))]
public class AudioDataEditor : Editor
{
    private MethodInfo playMethod;
    private MethodInfo stopMethod;

    private GUIContent playIcon;
    private GUIContent stopIcon;

    private void OnEnable()
    {
        var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

        playMethod = audioUtilType.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );

        stopMethod = audioUtilType.GetMethod(
            "StopAllPreviewClips",
            BindingFlags.Static | BindingFlags.Public
        );

        playIcon = EditorGUIUtility.IconContent("PlayButton");
        stopIcon = EditorGUIUtility.IconContent("PreMatQuad");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        SerializedProperty clipsProp = serializedObject.FindProperty("audioClips");

        if (clipsProp != null)
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            for (int i = 0; i < clipsProp.arraySize; i++)
            {
                SerializedProperty element = clipsProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(element, GUIContent.none);

                if (GUILayout.Button(playIcon, GUILayout.Width(28), GUILayout.Height(20)))
                {
                    PlayClip(element.objectReferenceValue as AudioClip);
                }

                if (GUILayout.Button(stopIcon, GUILayout.Width(28), GUILayout.Height(20)))
                {
                    StopAll();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        playMethod?.Invoke(null, new object[] { clip, 0, false });
    }

    private void StopAll()
    {
        stopMethod?.Invoke(null, null);
    }
}
