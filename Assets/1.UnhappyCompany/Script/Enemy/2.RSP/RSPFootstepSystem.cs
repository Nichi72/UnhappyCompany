using UnityEngine;
using FMODUnity;
using System.Collections.Generic;

/// <summary>
/// RSP가 이동하면서 바닥에 닿을 때 발소리를 재생하는 시스템입니다.
/// 여러 개의 GroundContactRaycast를 관리하고, 접촉 시 사운드를 재생합니다.
/// </summary>
public class RSPFootstepSystem : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("사운드 최대 거리")]
    [SerializeField] private float soundMaxDistance = 30f;
    
    [Tooltip("전역 사운드 쿨다운 (초) - 모든 raycast 포인트에 적용")]
    [SerializeField] private float globalSoundCooldown = 0.15f;
    
    [Header("Raycast Points")]
    [Tooltip("수동으로 할당할 Raycast 포인트들")]
    [SerializeField] private List<GroundContactRaycast> raycastPoints = new List<GroundContactRaycast>();
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 컴포넌트
    private List<GroundContactRaycast> activeRaycastPoints = new List<GroundContactRaycast>();
    private EnemyAIRSP rspController;
    
    // 상태
    private float lastSoundPlayTime = -999f;
    private int totalContactCount = 0;
    
    private void Awake()
    {
        rspController = GetComponent<EnemyAIRSP>();
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
        // 수동 할당된 포인트 사용
        activeRaycastPoints = new List<GroundContactRaycast>(raycastPoints);
        
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
            Debug.Log($"[RSPFootstepSystem] {activeRaycastPoints.Count}개의 Raycast 포인트가 설정되었습니다.");
        }
    }
    
    /// <summary>
    /// Raycast 포인트에서 접촉이 감지되었을 때 호출됩니다.
    /// </summary>
    private void OnRaycastContactDetected()
    {
        totalContactCount++;
        
        // 상태 체크 - Chase 또는 Patrol 상태일 때만 발소리 재생
        if (rspController != null && rspController.currentState != null)
        {
            string stateName = rspController.currentState.GetType().Name;
            if (stateName != "RSPChaseState" && stateName != "RSPPatrolState")
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[RSPFootstepSystem] 현재 상태({stateName})에서는 발소리를 재생하지 않습니다.");
                }
                return;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[RSPFootstepSystem] Raycast 포인트에서 접촉이 감지되었습니다.");
        }
        
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
        if (FMODEvents.instance.rspFootstep.IsNull)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[RSPFootstepSystem] RSP Footstep 사운드가 FMODEvents에 할당되지 않았습니다!", this);
            }
            return;
        }
        
        lastSoundPlayTime = Time.time;
        
        // 사운드 재생
        AudioManager.instance.Play3DSoundAtPosition(
            FMODEvents.instance.rspFootstep,
            transform.position,
            soundMaxDistance,
            $"RSPFootstep_{gameObject.name}"
        );
        
        if (showDebugInfo)
        {
            Debug.Log($"[RSPFootstepSystem] 발소리 재생 (총 접촉: {totalContactCount})");
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
                Debug.Log($"[RSPFootstepSystem] Raycast 포인트 추가: {raycastPoint.gameObject.name}");
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
                Debug.Log($"[RSPFootstepSystem] Raycast 포인트 제거: {raycastPoint.gameObject.name}");
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
        
        string debugText = $"[{gameObject.name}] RSP Footstep System\n";
        debugText += $"Active Raycast Points: {activeRaycastPoints.Count}\n";
        debugText += $"Total Contacts: {totalContactCount}\n";
        
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

