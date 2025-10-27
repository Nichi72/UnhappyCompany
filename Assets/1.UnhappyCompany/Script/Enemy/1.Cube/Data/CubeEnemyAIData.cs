using UnityEngine;

[CreateAssetMenu(fileName = "CubeEnemyData", menuName = "UnhappyCompany/AI/Cube Enemy Data")]
public class CubeEnemyAIData : BaseEnemyAIData
{
    [Header("Cube Attack Give Up Radius")]
    [Tooltip("공격 포기 범위")]
    public float attackGiveUpRadius = 10f;
    [Header("Cube Attack Settings")]
    [Tooltip("공격 시 돌진 힘")]
    public float rushForce = 20f;
    [Tooltip("공격 시 회전력")]
    public float torqueForce = 2f;
    [Tooltip("회전 공격 시 Y축 회전 각도")]
    public float rotationAttackAngle = 15f;
    [Tooltip("추적 시 이동 속도 증가 배율")]
    public float chaseSpeedMultiplier = 1.5f;
    
    [Header("Cube Damage Settings")]
    [Tooltip("돌진 공격 데미지 (매우 강력)")]
    public int rushDamage = 50;
    [Tooltip("평소 접촉 데미지 (지속)")]
    public int contactDamage = 10;
    [Tooltip("접촉 데미지 쿨다운(초) - 초당 데미지 간격")]
    public float contactDamageCooldown = 1f;
    
    [Header("== 오전 시간 설정 ==")]
    [Tooltip("공격 쿨다운 시간(초)")]
    public float morningAttackCooldown = 1.5f;
    [Tooltip("공격 시전 시간(초)")]
    public float morningAttackCastingTime = 2f;

    [Header("== 오후 시간 설정 ==")]
    [Tooltip("공격 쿨다운 시간(초)")]
    public float afternoonAttackCooldown = 1.2f;
    [Tooltip("공격 시전 시간(초)")]
    public float afternoonAttackCastingTime = 1.5f;

} 