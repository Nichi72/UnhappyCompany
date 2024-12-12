using UnityEngine;

[CreateAssetMenu(fileName = "RSPEnemyData", menuName = "UnhappyCompany/AI/RSP Enemy Data")]
public class RSPEnemyAIData : BaseEnemyAIData
{
    [Header("RSP Attack Settings")]
    [Header("공격 쿨다운 시간(초)")]
    public float attackCooldown = 2f;
    [Header("특수 공격 쿨다운 시간(초)")]
    public float specialAttackCooldown = 5f;
    [Header("특수 공격 범위")]
    public float specialAttackRange = 5f;
    
    [Header("RSP Time of Day Modifiers")]
    [Header("오후 시간대 이동 속도 증가 배율")]
    public float afternoonSpeedMultiplier = 1.3f;
    [Header("오후 시간대 순찰 범위 증가 배율")]
    public float afternoonPatrolRangeMultiplier = 1.4f;
} 