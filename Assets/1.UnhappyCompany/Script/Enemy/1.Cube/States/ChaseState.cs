using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// =-===============Cube에서는 사용하지 않는 방향으로==================
/// ChaseState는 적이 플레이어를 추적하는 상태를 나타냅니다.
/// NavMeshAgent를 사용하여 플레이어의 위치를 지속적으로 추적합니다.
/// </summary>
public class CubeChaseState : IState
{
    private EnemyAICube controller; // EnemyAICube 참조
    private UtilityCalculator utilityCalculator; // 유틸리티 계산기 참조

    /// <summary>
    /// ChaseState의 생성자. 상태 전환에 필요한 컨트롤러와 유틸리티 계산기를 할당합니다.
    /// </summary>
    /// <param name="controller">EnemyAICube 참조</param>
    /// <param name="calculator">UtilityCalculator 참조</param>
    public CubeChaseState(EnemyAICube controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    /// <summary>
    /// 상태 시작 시 호출되는 메서드. 추적 시작 시 필요한 초기화를 수행합니다.
    /// </summary>
    public void Enter()
    {
        // 추적 시작 시 필요한 로직 추가
    }

    public void ExecuteMorning()
    {
        Debug.Log("오전 추적 실행");
        HandleChaseLogic(
            () => controller.ChangeState(new CubeAttackState(controller, utilityCalculator)),
            () => controller.ChangeState(new CubePatrolState(controller, utilityCalculator, controller.pathCalculator))
        );
    }

    public void ExecuteAfternoon()
    {
        Debug.Log("오후 추적 실행");
        controller.agent.speed *= 1.5f; // 추적 속도 증가
        HandleChaseLogic(
            () => controller.ChangeState(new CubeAttackState(controller, utilityCalculator)),
            () => controller.ChangeState(new CubePatrolState(controller, utilityCalculator, controller.pathCalculator))
        );
    }

    public void ExecuteFixedMorning()
    {
        // ChaseState에서는 FixedUpdate에서 별도의 물리 연산이 필요 없으므로 빈 메서드로 둡니다.
    }

    public void ExecuteFixedAfternoon()
    {
        // ChaseState에서는 FixedUpdate에서 별도의 물리 연산이 필요 없으므로 빈 메서드로 둡니다.
    }

    public void ExecuteFixed()
    {
        // 기본 FixedExecute는 빈 메서드로 둡니다.
    }

    private void HandleChaseLogic(System.Action OnInnerAttackRadius = null, System.Action OnOuterAttackRadius = null)
    {
        controller.agent.SetDestination(controller.player.position);
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        
        if (distanceToPlayer < controller.AttackRadius)
        {
            OnInnerAttackRadius?.Invoke();
        }
        else if (distanceToPlayer > controller.AttackRadius)
        {
            OnOuterAttackRadius?.Invoke();
        }
    }

    /// <summary>
    /// 상태 종료 시 호출되는 메서드. 필요한 경우 종료 로직을 추가할 수 있습니다.
    /// </summary>
    public void Exit()
    {
        // 추적 종료 시 필요한 로직 추가
    }
} 