using UnityEngine;

[CreateAssetMenu(fileName = "MooAIData", menuName = "UnhappyCompany/AI/Moo AI Data")]
public class MooAIData : BaseEnemyAIData
{
    [Header("Moo Flee Settings")]
    [Tooltip("도망 최소 거리 비율 (0~2)")]
    [Range(0f, 2f)]
    public float fleeDistanceMinRatio = 0.8f;
    [Tooltip("도망 최대 거리 비율 (0~2)")]
    [Range(0f, 2f)]
    public float fleeDistanceMaxRatio = 1.5f;
    [Tooltip("도망 범위 색상")]
    public Color fleeRangeColor = Color.red;
    
    [Header("Moo Behavior Settings")]
    [Tooltip("슬라임 지속 시간")]
    public float slimeDuration = 5f;
    [Tooltip("슬라임 프리팹")]
    public GameObject slimePrefab;
    [Tooltip("도망 속도")]
    public float fleeSpeed = 6;
} 