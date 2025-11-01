using UnityEngine;
using FMODUnity;

/// <summary>
/// FMOD Listener 자동 설정 헬퍼
/// 씬에 FMOD Listener가 없으면 자동으로 추가합니다.
/// </summary>
public class FMODListenerAutoSetup : MonoBehaviour
{
    [Header("자동 설정")]
    [Tooltip("씬에 FMOD Listener가 없으면 자동으로 추가")]
    [SerializeField] private bool autoAddListener = true;
    
    [Tooltip("Listener를 추가할 대상 (비어있으면 Main Camera 사용)")]
    [SerializeField] private GameObject targetObject;
    
    void Start()
    {
        CheckAndSetupListener();
    }
    
    [ContextMenu("FMOD Listener 확인 및 설정")]
    void CheckAndSetupListener()
    {
        // FMOD Listener가 이미 있는지 확인
        if (StudioListener.ListenerCount > 0)
        {
            Debug.Log($"[FMOD] ✓ FMOD Listener가 이미 있습니다. ({StudioListener.ListenerCount}개)");
            return;
        }
        
        if (!autoAddListener)
        {
            Debug.LogWarning("[FMOD] ✗ FMOD Listener가 없습니다! autoAddListener를 활성화하거나 수동으로 추가하세요.");
            return;
        }
        
        // 대상 오브젝트 결정
        GameObject target = targetObject;
        
        if (target == null)
        {
            // Main Camera 찾기
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                target = mainCam.gameObject;
                Debug.Log($"[FMOD] Main Camera를 대상으로 선택: {target.name}");
            }
            else
            {
                // 현재 오브젝트 사용
                target = gameObject;
                Debug.LogWarning($"[FMOD] Main Camera를 찾을 수 없어 현재 오브젝트 사용: {target.name}");
            }
        }
        
        // FMOD Listener 추가
        StudioListener listener = target.GetComponent<StudioListener>();
        if (listener == null)
        {
            listener = target.AddComponent<StudioListener>();
            Debug.Log($"[FMOD] ✓ FMOD Studio Listener가 '{target.name}'에 추가되었습니다!");
        }
        else
        {
            Debug.Log($"[FMOD] FMOD Studio Listener가 이미 '{target.name}'에 있습니다.");
        }
    }
    
    void OnGUI()
    {
        // 화면에 경고 표시
        if (StudioListener.ListenerCount == 0)
        {
            GUI.color = Color.red;
            GUILayout.BeginArea(new Rect(Screen.width - 410, 10, 400, 100));
            GUILayout.Box("⚠️ FMOD Listener 없음!", GUILayout.Width(390), GUILayout.Height(80));
            GUILayout.Label("3D 사운드가 들리지 않습니다.");
            GUILayout.Label("카메라나 플레이어에 'FMOD Studio Listener' 추가 필요!");
            GUILayout.EndArea();
            GUI.color = Color.white;
        }
    }
}

