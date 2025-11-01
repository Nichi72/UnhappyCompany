using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    private AudioManager audioManager;
    
    private void OnEnable()
    {
        audioManager = (AudioManager)target;
    }
    
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        DrawDefaultInspector();
        
        // 실행 중일 때만 디버그 정보 표시
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서 실행하면 재생 중인 사운드 정보를 볼 수 있습니다.", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎵 사운드 디버그 정보", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 재생 중인 사운드 개수
        int soundCount = audioManager.GetActiveSoundCount();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"재생 중인 사운드: {soundCount}개", EditorStyles.largeLabel);
        EditorGUILayout.Space(5);
        
        if (soundCount > 0)
        {
            // 상세 정보 표시
            string info = audioManager.GetActiveSoundsInfo();
            EditorGUILayout.TextArea(info, GUILayout.MinHeight(100));
        }
        else
        {
            EditorGUILayout.LabelField("현재 재생 중인 사운드가 없습니다.", EditorStyles.centeredGreyMiniLabel);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        // 범례
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("📋 씬뷰 범례:", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("● 시안색 구 = One-Shot 사운드", GUILayout.Width(220));
        EditorGUILayout.LabelField("● 노란색 구 = 루프 사운드");
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("→ 흰색 화살표 = 사운드 이동 방향 및 속도");
        GUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // 자동 새로고침
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}

