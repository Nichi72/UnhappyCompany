using UnityEngine;
using System;

/// <summary>
/// 메테리얼별 에미션 데이터 (색상과 인텐시티를 함께 저장)
/// </summary>
[Serializable]
public struct EmissionData
{
    public Color color;
    public float intensity;
}

/// <summary>
/// 에미션을 제어할 렌더러와 인텐시티, 색상 값을 저장하는 구조체
/// </summary>
[Serializable]
public struct EmissionRendererData
{
    [Tooltip("제어할 렌더러 (메테리얼은 런타임에 동적으로 가져옴)")]
    public Renderer renderer;
    
    [Tooltip("복구할 에미션 인텐시티 값 (HDRP의 Emission Intensity)")]
    public float emissionIntensity;
    
    [Tooltip("복구할 에미션 색상 (베이스 컬러, HDRP의 Emissive Map 색상)")]
    public Color emissionColor;
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