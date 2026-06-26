using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Test_AudioSystem))]
public class TestAudioSystemEditor : Editor
{
    private Test_AudioSystem testAudioSystem;

    private void OnEnable()
    {
        testAudioSystem = (Test_AudioSystem)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("오디오 테스트 버튼", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("오디오 테스트 버튼은 Play Mode에서만 동작합니다.", MessageType.Info);
        }

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            DrawBgmButtons();
            DrawSfxButtons();
            DrawLoopButtons();
            DrawControlButtons();
            DrawMovementButtons();
        }
    }

    private void DrawBgmButtons()
    {
        DrawSection("BGM");
        DrawButton("BGM 재생", testAudioSystem.Example_PlayBgm);
        DrawButton("BGM 페이드 재생", testAudioSystem.Example_PlayBgm_Fade);
        DrawButton("BGM 정지", testAudioSystem.Example_StopBgm);
    }

    private void DrawSfxButtons()
    {
        DrawSection("SFX");
        DrawButton("2D SFX 재생", testAudioSystem.Example_PlayTestSfx2D);
        DrawButton("2D SFX Clip 0 재생", testAudioSystem.Example_PlayTestSfx2D_ClipIndex);
        DrawButton("3D SFX 위치 재생", testAudioSystem.Example_PlayTestSfx3D_Position);
        DrawButton("3D SFX 추적 재생", testAudioSystem.Example_PlayTestSfx3D_Transform);
        DrawButton("3D SFX 거리 커스텀 재생", testAudioSystem.Example_PlayTestSfx3D_CustomDistance);
    }

    private void DrawLoopButtons()
    {
        DrawSection("Loop SFX");
        DrawButton("2D Loop 재생", testAudioSystem.Example_PlayLoopSfxByCancellationToken);
        DrawButton("3D Loop 위치 재생", testAudioSystem.Example_PlayLoopSfx3D_Position);
        DrawButton("3D Loop 추적 재생", testAudioSystem.Example_PlayLoopSfx3D_Follow);
        DrawButton("3D Loop 거리 커스텀 재생", testAudioSystem.Example_PlayLoopSfx3D_CustomDistance);
        DrawButton("Loop 정지", testAudioSystem.Example_StopLoopSfxByCancellationToken);
    }

    private void DrawControlButtons()
    {
        DrawSection("제어");
        DrawButton("전체 일시정지", testAudioSystem.Example_Pause);
        DrawButton("전체 재개", testAudioSystem.Example_UnPause);
        DrawButton("전체 정지", testAudioSystem.Example_StopAll);
    }

    private void DrawMovementButtons()
    {
        DrawSection("이동 타겟 3D SFX");
        DrawButton("이동 타겟 SFX 재생", testAudioSystem.Example_Movable3DSFX);
    }

    private static void DrawSection(string title)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
    }

    private static void DrawButton(string label, System.Action action)
    {
        if (GUILayout.Button(label))
        {
            action.Invoke();
        }
    }
}
