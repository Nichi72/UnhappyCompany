using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Egg))]
public class EggEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();

        // 여백 추가
        EditorGUILayout.Space(10);

        // Egg 컴포넌트 가져오기
        Egg egg = (Egg)target;

        // 에디터에서만 실행 가능하도록 확인
        if (Application.isPlaying)
        {
            // 굵은 글씨 스타일로 라벨 표시
            GUIStyle boldStyle = new GUIStyle(GUI.skin.label);
            boldStyle.fontStyle = FontStyle.Bold;
            boldStyle.fontSize = 12;
            
            EditorGUILayout.LabelField("부화 제어", boldStyle);
            
            // 버튼 생성
            if (GUILayout.Button("즉시 부화", GUILayout.Height(30)))
            {
                egg.ForceHatch();
            }

            EditorGUILayout.HelpBox("이 버튼은 시간을 무시하고 알을 즉시 부화시킵니다.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 부화 버튼이 활성화됩니다.", MessageType.Warning);
        }
    }
}

