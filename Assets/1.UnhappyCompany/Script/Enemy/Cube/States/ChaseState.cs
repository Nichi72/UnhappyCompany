using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ChaseState는 적이 플레이어를 추적하는 상태를 나타냅니다.
/// NavMeshAgent를 사용하여 플레이어의 위치를 지속적으로 추적합니다.
/// </summary>
public class ChaseState : IState
{
    private EnemyAIController controller; // EnemyAIController 참조
    private UtilityCalculator utilityCalculator; // 유틸리티 계산기 참조

    /// <summary>
    /// ChaseState의 생성자. 상태 전환에 필요한 컨트롤러와 유틸리티 계산기를 할당합니다.
    /// </summary>
    /// <param name="controller">EnemyAIController 참조</param>
    /// <param name="calculator">UtilityCalculator 참조</param>
    public ChaseState(EnemyAIController controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    /// <summary>
    /// 상태 시작 시 호출되는 메서드. 추적 시작 시 필요한 초기화를 수행합니다.
    /// </summary>
    public void Enter()
    {
        // 추적 시작 시 애니메이션 전환 등 필요한 로직을 여기에 추가할 수 있습니다.
    }

    /// <summary>
    /// 매 프레임마다 호출되는 메서드. 플레이어를 추적하고 상태 전환 조건을 확인합니다.
    /// </summary>
    public void Execute()
    {
        // NavMeshAgent를 통해 플레이어의 현재 위치로 이동
        controller.agent.SetDestination(controller.player.position);

        // 플레이어와의 현재 거리 계산
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);

        if (distanceToPlayer < controller.attackRadius) // 공격 거리 내에 있는지 확인
        {
            controller.ChangeState(new AttackState(controller, utilityCalculator)); // 공격 상태로 전환
        }
        else if (distanceToPlayer > controller.chaseRadius) // 추적 반경을 벗어났는지 확인
        {
            controller.ChangeState(new PatrolState(controller, utilityCalculator)); // 다시 순찰 상태로 전환
        }
    }

    /// <summary>
    /// 상태 종료 시 호출되는 메서드. 필요한 경우 종료 로직을 추가할 수 있습니다.
    /// </summary>
    public void Exit()
    {
        // 추적 종료 시 애니메이션 초기화 등 필요한 로직을 여기에 추가할 수 있습니다.
    }
} 