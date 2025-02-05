using UnityEngine;

[CreateAssetMenu(fileName = "CubeEnemyData", menuName = "UnhappyCompany/AI/Cube Enemy Data")]
public class CubeEnemyAIData : BaseEnemyAIData
{
    [Header("Cube Attack Settings")]
    [Header("공격 시 돌진 힘")]
    public float rushForce = 20f;
    [Header("공격 시 회전력")]
    public float torqueForce = 2f;
    [Header("회전 공격 시 Y축 회전 각도")]
    public float rotationAttackAngle = 15f;
    [Header("추적 시 이동 속도 증가 배율")]
    public float chaseSpeedMultiplier = 1.5f;
    
    [Header("== 오전 시간 설정 ==")]
    [Header("공격 쿨다운 시간(초)")]
    public float morningAttackCooldown = 1.5f;
    [Header("공격 시전 시간(초)")]
    public float morningAttackCastingTime = 2f;

    [Header("== 오후 시간 설정 ==")]
    [Header("공격 쿨다운 시간(초)")]
    public float afternoonAttackCooldown = 1.2f;
    [Header("공격 시전 시간(초)")]
    public float afternoonAttackCastingTime = 1.5f;

} 