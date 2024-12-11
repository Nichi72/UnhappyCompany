using UnityEngine;

/// <summary>
/// RSP 타입 적의 AI 컨트롤러입니다.
/// </summary>
public class EnemyAIRSP : EnemyAIController<RSPEnemyAIData>
{
    // RSP 전용 프로퍼티
    public float AttackCooldown => enemyData.attackCooldown;
    public float SpecialAttackCooldown => enemyData.specialAttackCooldown;
    public float SpecialAttackRange => enemyData.specialAttackRange;
    
    protected override void Start()
    {
        base.Start();
        // RSP 특화 초기화
        ChangeState(new RSPPatrolState(this, utilityCalculator));
    }

    protected override void HandleTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        base.HandleTimeOfDayChanged(newTimeOfDay);
        // RSP 특화 시간 변경 처리
        Debug.Log($"RSP: 시간이 {newTimeOfDay}로 변경되었습니다.");
    }
} 