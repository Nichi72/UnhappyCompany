using UnityEngine;

public abstract class BaseEnemyAIData : ScriptableObject
{
    [Header("Base Range Settings")]
    [Tooltip("순찰 반경")]
    public float patrolRadius = 10f;
    [Tooltip("추적 반경")]
    public float chaseRadius = 15f;
    [Tooltip("공격 반경")]
    public float attackRadius = 2f;

    [Header("Base Movement Settings")]
    [Tooltip("기본 이동 속도")]
    public float moveSpeed = 5f;

} 