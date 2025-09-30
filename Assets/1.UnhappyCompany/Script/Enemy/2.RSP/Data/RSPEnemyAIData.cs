using UnityEngine;

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
} 