using UnityEngine;

[CreateAssetMenu(fileName = "GhostEnemyData", menuName = "UnhappyCompany/AI/Ghost Enemy Data")]
public class GhostEnemyAIData : BaseEnemyAIData
{
    [Header("Ghost Specific Movement")]
    [Tooltip("텔레포트 최대 거리")]
    public float teleportMaxDistance = 10f;
    [Tooltip("텔레포트 쿨다운")]
    public float teleportCooldown = 5f;
    
    [Header("Ghost Specific Attack")]
    [Tooltip("공포 효과 지속 시간")]
    public float fearEffectDuration = 3f;
} 