using UnityEngine;
using System;

/// <summary>
/// 에미션을 제어할 렌더러와 켜기/끄기 값을 저장하는 구조체
/// </summary>
[Serializable]
public struct EmissionRendererData
{
    [Tooltip("제어할 렌더러")]
    public Renderer renderer;
    
    [Header("켜졌을 때")]
    [Tooltip("에미션 켜졌을 때 색상")]
    public Color emissionColorOn;
    
    [Tooltip("에미션 켜졌을 때 인텐시티")]
    public float emissionIntensityOn;
    
    [Header("꺼졌을 때")]
    [Tooltip("에미션 꺼졌을 때 색상")]
    public Color emissionColorOff;
    
    [Tooltip("에미션 꺼졌을 때 인텐시티")]
    public float emissionIntensityOff;
}

[CreateAssetMenu(fileName = "RSPEnemyAIData", menuName = "UnhappyCompany/Enemy/RSPEnemyAIData")]
public class RSPEnemyAIData : BaseEnemyAIData
{
    [Header("RSP 특화 설정")]
    [Tooltip("회전 속도")]
    public float rotationSpeed = 5f;
    [Tooltip("플레이어 감지 범위")]
    public float detectionRange = 10f;
    
    [Header("RSP 공격 설정")]
    [Tooltip("공격 쿨다운")]
    public float attackCooldown = 2f;
    [Tooltip("특수 공격 범위")]
    public float specialAttackRange = 5f;
    [Tooltip("특수 공격 쿨다운")]
    public float specialAttackCooldown = 5f;
    
    [Header("RSP 패트롤 설정")]
    [Tooltip("최소 패트롤 대기 시간")]
    public float minPatrolWaitTime = 1f;
    [Tooltip("최대 패트롤 대기 시간")]
    public float maxPatrolWaitTime = 3f;
    
    [Header("RSP 비활성화 설정")]
    [Tooltip("비활성화 지속 시간 (초)")]
    public float disabledDuration = 60f;
} 