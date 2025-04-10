using UnityEngine;

public abstract class BaseEnemyAIData : ScriptableObject
{
    [Header("Enemy Name")]
    public string enemyName = "...";

    [Header("Enemy Prefab")]
    public GameObject prefab;

    [Header("Notification Category")]
    public ENotificationCategory category = ENotificationCategory.Normal;
    [Header("Enemy Type")]
    public EnemyType enemyType;
    [Header("Enemy Cost")]
    public float enemyCost;


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
    [Tooltip("비용")]
    public int Cost = 10;

} 