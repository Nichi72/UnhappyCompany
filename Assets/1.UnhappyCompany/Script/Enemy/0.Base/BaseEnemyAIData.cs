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
    public float patrolRangeMin = 0.7f;
    public float patrolRangeMax = 1.3f;
    public Color patrolGizmoRangeColor = Color.green;
    public Color patrolGizmoRangeMinMaxColor = Color.yellow;

    [Tooltip("공격 반경")]
    public float attackRadius = 2f;

    [Header("Base Movement Settings")]
    [Tooltip("기본 이동 속도")]
    public float moveSpeed = 5f;
    [Tooltip("비용")]
    public int Cost = 10;
    [Header("Base Status Settings")]
    [Tooltip("체력")]
    public int hpMax = 100;

} 