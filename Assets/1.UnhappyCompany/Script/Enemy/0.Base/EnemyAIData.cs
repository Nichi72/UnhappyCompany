using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIData", menuName = "UnhappyCompany/AI/Enemy AI Data")]
public class EnemyAIData : ScriptableObject
{
    [Header("Range Settings")]
    [Tooltip("순찰 반경")]
    public float patrolRadius = 10f;
    [Tooltip("추적 반경")]
    public float chaseRadius = 15f;
    [Tooltip("공격 반경")]
    public float attackRadius = 2f;

    [Header("Movement Settings")]
    [Tooltip("기본 이동 속도")]
    public float moveSpeed = 5f;
    [Tooltip("추적 시 이동 속도 증가 배율")]
    public float chaseSpeedMultiplier = 1.5f;

    [Header("Attack Settings")]
    [Tooltip("공격 쿨다운 시간(초)")]
    public float attackCooldown = 1.5f;
    [Tooltip("공격 시 돌진 힘")]
    public float rushForce = 20f;
    [Tooltip("공격 시 회전력")]
    public float torqueForce = 2f;

    [Header("Time of Day Modifiers")]
    [Tooltip("오후 시간대 이동 속도 증가 배율")]
    public float afternoonSpeedMultiplier = 1.2f;
    [Tooltip("오후 시간대 순찰 범위 증가 배율")]
    public float afternoonPatrolRangeMultiplier = 1.5f;
} 