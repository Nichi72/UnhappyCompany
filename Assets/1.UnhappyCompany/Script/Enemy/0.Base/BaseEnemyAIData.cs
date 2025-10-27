using UnityEngine;

public abstract class BaseEnemyAIData : ScriptableObject
{
    [Header("Enemy Name")]
    public string enemyName = "...";

    [Header("Enemy Prefab")]
    public GameObject prefab;
    [Header("dangerLevel")]
    public EnemyDangerLevel dangerLevel = EnemyDangerLevel.Low;

    [Header("Notification Category")]
    public ENotificationCategory category = ENotificationCategory.Normal;
    [Header("Enemy Type")]
    public EnemyType enemyType;
    [Header("Enemy Cost")]
    public float enemyCost;


    [Header("Base Range Settings")]
    [Tooltip("순찰 기준 반경")]
    public float patrolRadius = 10f;
    
    [Header("Patrol Distance Settings (Percentage)")]
    [Tooltip("순찰 최소 거리 비율 (0~1)")]
    [Range(0f, 1f)]
    public float patrolDistanceMinRatio = 0.3f;
    [Tooltip("순찰 최대 거리 비율 (0~1)")]
    [Range(0f, 1f)]
    public float patrolDistanceMaxRatio = 0.7f;
    
    [Header("Range Visualization")]
    public Color patrolRangeColor = Color.green;
    public bool showRangesInGame = false; // 게임뷰에서 범위 표시 여부

    [Header("Base Movement Settings")]
    [Tooltip("기본 이동 속도")]
    public float moveSpeed = 5f;
    [Tooltip("비용")]
    public int Cost = 10;
    [Header("Base Status Settings")]
    [Tooltip("체력")]
    public int hpMax = 100;

} 