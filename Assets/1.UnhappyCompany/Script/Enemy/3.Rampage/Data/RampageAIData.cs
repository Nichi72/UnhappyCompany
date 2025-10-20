using UnityEngine;

[CreateAssetMenu(fileName = "RampageAIData", menuName = "UnhappyCompany/AI/Rampage AI Data")]
public class RampageAIData : BaseEnemyAIData
{
    [Header("Rampage Settings")]
    public int maxHP = 50;          // 기본 HP
    public float chargeSpeed = 8f;    // 돌진 모드 속도

    [Header("Detection")]
    public float detectRange = 10f;   // 플레이어 감지 범위
    public float detectAngle = 60f;   // 플레이어 감지 각도 (전방 위주)
    
    [Header("Attack Settings")]
    [Tooltip("플레이어 추격 → 물리 돌진 전환 거리 (m)\n이 거리 안에 들어오면 NavMesh 추격을 중단하고 물리 기반 돌진으로 전환")]
    public float attackRadius = 2f;

    [Header("Panel Settings")]
    public int maxPanelHealth = 6;       // 패널 공격 요구량
    public float panelOpenTime = 5f; // 패널 열림 유지 시간 (일반)
    public float cushionPanelOpenTime = 10f; // 패널 열림 유지 시간 (쿠션 충돌)

    [Header("Collision/Cushion Settings")]
    public int cushionPanelCount = 3; // 쿠션 충돌 시 노출 패널 수
    public int noCushionPanelCount = 1; // 쿠션 없이 충돌 시 노출 패널 수
    public int hpLossOnNoCushion = 10;  // 쿠션 없이 충돌 시 HP 감소량

    [Header("Cooldown/Timer")]
    public float chargeCooldown = 3f; // 돌진 후 재돌진 전 대기 시간
    public float stunDuration = 5f;   // 스턴 상태 지속 시간
    [Header("Charge State")]
    public int maxChargeCount = 3; // 돌진 횟수
    [Tooltip("이 속도 이하로 떨어지면 충돌로 간주하여 패널이 열림")]
    public float chargeStopSpeedThreshold = 0.5f; // 돌진 중단 속도 임계값

    [Header("Disabled Settings")]
    [Tooltip("무력화될 때 부서지기까지 대기 시간 (초)")]
    public float breakDelay = 1f;     // 부서지기 전 대기 시간

    [Header("Explode Settings - Damage")]
    [Tooltip("데미지 범위 (중심에서 최대 거리)")]
    public float explosionDamageRadius = 5f;
    [Tooltip("최대 데미지 (중심)")]
    public int explosionMaxDamage = 50;
    [Tooltip("최소 데미지 (범위 끝)")]
    public int explosionMinDamage = 10;
    
    [Header("Explode Settings - Physics")]
    [Tooltip("물리 효과 범위")]
    public float explosionForceRadius = 8f;
    [Tooltip("폭발 힘")]
    public float explosionForce = 1000f;
    [Tooltip("상승 힘 (위로 튀어오르는 정도)")]
    public float explosionUpwardModifier = 1.5f;

    [Header("RUSH")]
    public float rushSpeed = 10f;
    public int rushDamage = 50;
} 