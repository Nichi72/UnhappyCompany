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

    [Header("Panel Settings")]
    public int maxPanelHealth = 6;       // 패널 공격 요구량
    public float panelOpenTime = 10f; // 패널 열림 유지 시간

    [Header("Collision/Cushion Settings")]
    public int cushionPanelCount = 3; // 쿠션 충돌 시 노출 패널 수
    public int noCushionPanelCount = 1; // 쿠션 없이 충돌 시 노출 패널 수
    public int hpLossOnNoCushion = 10;  // 쿠션 없이 충돌 시 HP 감소량

    [Header("Cooldown/Timer")]
    public float chargeCooldown = 3f; // 돌진 후 재돌진 전 대기 시간
    public float stunDuration = 5f;   // 스턴 상태 지속 시간
    [Header("Charge State")]
    public int maxChargeCount = 3; // 돌진 횟수

    [Header("Explode Settings")]
    public float explodeRadius = 5f;  // 자폭 범위
    public int explodeDamage = 30;    // 자폭 대미지

    [Header("RUSH")]
    public float rushSpeed = 10f;
    public int rushDamage = 50;
} 