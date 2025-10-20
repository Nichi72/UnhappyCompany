using UnityEngine;
using UnityEditor;

/// <summary>
/// ItemGiftBox의 커스텀 인스펙터 에디터
/// </summary>
[CustomEditor(typeof(ItemGiftBox))]
public class ItemGiftBoxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        DrawDefaultInspector();
        
        ItemGiftBox giftBox = (ItemGiftBox)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("테스트 기능", EditorStyles.boldLabel);
        
        // 플레이 모드일 때만 활성화되는 버튼
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("🎁 선물상자 열기 (플레이 모드 전용)", GUILayout.Height(30)))
        {
            // ContextMenu 메서드 호출
            giftBox.SendMessage("TestOpenGiftBox", SendMessageOptions.DontRequireReceiver);
        }
        
        GUI.enabled = true; // 다시 활성화
        
        // 플레이 모드가 아닐 때도 사용 가능한 버튼들
        if (GUILayout.Button("📊 드랍 확률 표시", GUILayout.Height(25)))
        {
            giftBox.SendMessage("ShowDropChances", SendMessageOptions.DontRequireReceiver);
        }
        
        if (GUILayout.Button("🎲 드랍 시뮬레이션 (로그만)", GUILayout.Height(25)))
        {
            giftBox.SendMessage("SimulateDrop", SendMessageOptions.DontRequireReceiver);
        }
        
        // 플레이 모드가 아니면 안내 메시지 표시
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("선물상자를 실제로 열려면 플레이 모드로 진입하세요.", MessageType.Info);
        }
    }
}

