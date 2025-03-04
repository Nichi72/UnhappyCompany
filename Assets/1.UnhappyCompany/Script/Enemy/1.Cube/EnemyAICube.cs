using UnityEngine;

/// <summary>
/// Cube 타입 적의 AI 컨트롤러입니다.
/// </summary>
public class EnemyAICube : EnemyAIController<CubeEnemyAIData>
{
    // Cube 전용 프로퍼티
    public float RushForce => enemyData.rushForce;
    public float TorqueForce => enemyData.torqueForce;
    public float AttackCooldown => CurrentTimeOfDay == TimeOfDay.Morning ? enemyData.morningAttackCooldown : enemyData.afternoonAttackCooldown;
    public float AttackCastingTime => CurrentTimeOfDay == TimeOfDay.Morning ? enemyData.morningAttackCastingTime : enemyData.afternoonAttackCastingTime;
    
    public PathCalculator pathCalculator;
    
    protected override void Start()
    {
        base.Start();
        // Cube 특화 초기화
        ChangeState(new CubePatrolState(this, utilityCalculator, pathCalculator));
    }

    protected override void HandleTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        base.HandleTimeOfDayChanged(newTimeOfDay);
        // Cube 특화 시간 변경 처리
        Debug.Log($"Cube: 시간이 {newTimeOfDay}로 변경되었습니다.");
    }
} 