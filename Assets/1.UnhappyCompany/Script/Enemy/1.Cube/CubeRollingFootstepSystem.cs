using UnityEngine;
using FMODUnity;
using System.Collections.Generic;

/// <summary>
/// 큐브가 굴러다니면서 바닥에 닿을 때 발소리를 재생하는 시스템입니다.
/// 여러 개의 GroundContactRaycast를 관리하고, 접촉 시 사운드를 재생합니다.
/// </summary>
public class CubeRollingFootstepSystem : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("재생할 발소리 이벤트")]
    [SerializeField] private EventReference footstepSound;
    
    [Tooltip("사운드 최대 거리")]
    [SerializeField] private float soundMaxDistance = 30f;
    
    [Tooltip("전역 사운드 쿨다운 (초) - 모든 raycast 포인트에 적용")]
    [SerializeField] private float globalSoundCooldown = 0.15f;
    
    [Tooltip("속도에 따른 볼륨 조절")]
    [SerializeField] private bool adjustVolumeByVelocity = true;
    
    [Tooltip("최소 볼륨")]
    [SerializeField] private float minVolume = 0.3f;
    
    [Tooltip("최대 볼륨")]
    [SerializeField] private float maxVolume = 1.0f;
    
    [Tooltip("최소 속도 (이 속도에서 minVolume)")]
    [SerializeField] private float minVelocity = 1f;
    
    [Tooltip("최대 속도 (이 속도에서 maxVolume)")]
    [SerializeField] private float maxVelocity = 10f;
    
    [Header("Raycast Points")]
    [Tooltip("자동으로 Raycast 포인트를 생성할지 여부")]
    [SerializeField] private bool autoCreateRaycastPoints = true;
    
    [Tooltip("Raycast 포인트 거리 (큐브 중심으로부터)")]
    [SerializeField] private float raycastPointDistance = 0.5f;
    
    [Tooltip("수동으로 할당된 Raycast 포인트들 (autoCreateRaycastPoints가 false일 때 사용)")]
    [SerializeField] private List<GroundContactRaycast> manualRaycastPoints = new List<GroundContactRaycast>();
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 컴포넌트
    private List<GroundContactRaycast> activeRaycastPoints = new List<GroundContactRaycast>();
    private Rigidbody rb;
    
    // 상태
    private float lastSoundPlayTime = -999f;
    private int totalContactCount = 0;
    
    // 자동 생성된 포인트 홀더
    private GameObject raycastPointsHolder;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError($"[CubeRollingFootstepSystem] {gameObject.name}: Rigidbody를 찾을 수 없습니다!", this);
        }
        
        SetupRaycastPoints();
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        foreach (var point in activeRaycastPoints)
        {
            if (point != null)
            {
                point.OnGroundContact -= OnRaycastContactDetected;
            }
        }
    }
    
    /// <summary>
    /// Raycast 포인트를 설정합니다.
    /// </summary>
    private void SetupRaycastPoints()
    {
        if (autoCreateRaycastPoints)
        {
            CreateRaycastPoints();
        }
        else
        {
            // 수동 할당된 포인트 사용
            activeRaycastPoints = new List<GroundContactRaycast>(manualRaycastPoints);
        }
        
        // 모든 포인트에 이벤트 구독
        foreach (var point in activeRaycastPoints)
        {
            if (point != null)
            {
                point.OnGroundContact += OnRaycastContactDetected;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[CubeRollingFootstepSystem] {activeRaycastPoints.Count}개의 Raycast 포인트가 설정되었습니다.");
        }
    }
    
    /// <summary>
    /// 큐브의 6면 중심에 Raycast 포인트를 자동 생성합니다.
    /// </summary>
    private void CreateRaycastPoints()
    {
        // 기존 홀더가 있으면 제거
        if (raycastPointsHolder != null)
        {
            DestroyImmediate(raycastPointsHolder);
        }
        
        raycastPointsHolder = new GameObject("RaycastPoints");
        raycastPointsHolder.transform.SetParent(transform);
        raycastPointsHolder.transform.localPosition = Vector3.zero;
        raycastPointsHolder.transform.localRotation = Quaternion.identity;
        
        // 큐브의 6개 면 방향 (로컬 좌표계)
        Vector3[] faceDirections = new Vector3[]
        {
            Vector3.up,      // 위
            Vector3.down,    // 아래
            Vector3.left,    // 왼쪽
            Vector3.right,   // 오른쪽
            Vector3.forward, // 앞
            Vector3.back     // 뒤
        };
        
        string[] faceNames = new string[]
        {
            "Top", "Bottom", "Left", "Right", "Front", "Back"
        };
        
        activeRaycastPoints.Clear();
        
        for (int i = 0; i < faceDirections.Length; i++)
        {
            GameObject raycastPoint = new GameObject($"Raycast_{faceNames[i]}");
            raycastPoint.transform.SetParent(raycastPointsHolder.transform);
            raycastPoint.transform.localPosition = faceDirections[i] * raycastPointDistance;
            raycastPoint.transform.localRotation = Quaternion.identity;
            
            var raycastComponent = raycastPoint.AddComponent<GroundContactRaycast>();
            activeRaycastPoints.Add(raycastComponent);
        }
    }
    
    /// <summary>
    /// Raycast 포인트에서 접촉이 감지되었을 때 호출됩니다.
    /// </summary>
    private void OnRaycastContactDetected()
    {
        totalContactCount++;
        
        // 전역 쿨다운 체크
        if (Time.time - lastSoundPlayTime < globalSoundCooldown)
        {
            return;
        }
        
        PlayFootstepSound();
    }
    
    /// <summary>
    /// 발소리를 재생합니다.
    /// </summary>
    private void PlayFootstepSound()
    {
        if (footstepSound.IsNull)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[CubeRollingFootstepSystem] Footstep Sound가 할당되지 않았습니다!", this);
            }
            return;
        }
        
        lastSoundPlayTime = Time.time;
        
        // 사운드 재생
        var emitter = AudioManager.instance.Play3DSoundAtPosition(
            footstepSound,
            transform.position,
            soundMaxDistance,
            $"CubeFootstep_{gameObject.name}"
        );
        
        // 볼륨 조절
        if (adjustVolumeByVelocity && rb != null && emitter != null)
        {
            float velocity = rb.linearVelocity.magnitude;
            float normalizedVelocity = Mathf.InverseLerp(minVelocity, maxVelocity, velocity);
            float volume = Mathf.Lerp(minVolume, maxVolume, normalizedVelocity);
            
            // FMOD EventInstance 볼륨 설정
            if (emitter.emitter != null && emitter.emitter.EventInstance.isValid())
            {
                emitter.emitter.EventInstance.setVolume(volume);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[CubeRollingFootstepSystem] 발소리 재생 (속도: {velocity:F1}, 볼륨: {volume:F2}, 총 접촉: {totalContactCount})");
            }
        }
        else if (showDebugInfo)
        {
            Debug.Log($"[CubeRollingFootstepSystem] 발소리 재생 (총 접촉: {totalContactCount})");
        }
    }
    
    /// <summary>
    /// 수동으로 Raycast 포인트를 추가합니다.
    /// </summary>
    public void AddRaycastPoint(GroundContactRaycast raycastPoint)
    {
        if (raycastPoint == null) return;
        
        if (!activeRaycastPoints.Contains(raycastPoint))
        {
            activeRaycastPoints.Add(raycastPoint);
            raycastPoint.OnGroundContact += OnRaycastContactDetected;
            
            if (showDebugInfo)
            {
                Debug.Log($"[CubeRollingFootstepSystem] Raycast 포인트 추가: {raycastPoint.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 수동으로 Raycast 포인트를 제거합니다.
    /// </summary>
    public void RemoveRaycastPoint(GroundContactRaycast raycastPoint)
    {
        if (raycastPoint == null) return;
        
        if (activeRaycastPoints.Contains(raycastPoint))
        {
            raycastPoint.OnGroundContact -= OnRaycastContactDetected;
            activeRaycastPoints.Remove(raycastPoint);
            
            if (showDebugInfo)
            {
                Debug.Log($"[CubeRollingFootstepSystem] Raycast 포인트 제거: {raycastPoint.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 디버그 정보를 표시합니다.
    /// </summary>
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        // 화면에 디버그 정보 표시
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.white;
        
        string debugText = $"[{gameObject.name}] Cube Footstep System\n";
        debugText += $"Active Raycast Points: {activeRaycastPoints.Count}\n";
        debugText += $"Total Contacts: {totalContactCount}\n";
        
        if (rb != null)
        {
            debugText += $"Velocity: {rb.linearVelocity.magnitude:F2} m/s\n";
        }
        
        int groundedCount = 0;
        foreach (var point in activeRaycastPoints)
        {
            if (point != null && point.IsGrounded)
            {
                groundedCount++;
            }
        }
        debugText += $"Grounded Points: {groundedCount}/{activeRaycastPoints.Count}\n";
        
        // 화면 상단에 표시
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        if (screenPos.z > 0)
        {
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 300, 100), debugText, style);
        }
    }
}

