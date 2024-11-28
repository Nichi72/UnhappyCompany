using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PatrolState는 적이 순찰을 수행하는 상태를 나타냅니다.
/// 무작위 순찰 지점을 설정하고 NavMeshAgent를 통해 이동을 제어합니다.
/// </summary>
public class PatrolState : IState
{
    private EnemyAIController controller; // EnemyAIController 참조
    private UtilityCalculator utilityCalculator; // 유틸리티 계산기 참조
    private Vector3 patrolPoint; // 현재 순찰 지점

    /// <summary>
    /// PatrolState의 생성자. 상태 전환에 필요한 컨트롤러와 유틸리티 계산기를 할당합니다.
    /// </summary>
    /// <param name="controller">EnemyAIController 참조</param>
    /// <param name="calculator">UtilityCalculator 참조</param>
    public PatrolState(EnemyAIController controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    /// <summary>
    /// 상태 시작 시 호출되는 메서드. 새로운 순찰 지점을 설정하고 이동을 시작합니다.
    /// </summary>
    public void Enter()
    {
        // 새로운 순찰 지점 설정
        patrolPoint = GetRandomPatrolPoint();
        controller.agent.SetDestination(patrolPoint); // NavMeshAgent를 통해 순찰 지점으로 이동
    }

    /// <summary>
    /// 매 프레임마다 호출되는 메서드. 순찰을 수행하고 플레이어 감지 여부를 확인합니다.
    /// </summary>
    public void Execute()
    {
        // 현재 목적지에 도착했는지 확인
        if (!controller.agent.pathPending && controller.agent.remainingDistance < 0.5f)
        {
            patrolPoint = GetRandomPatrolPoint(); // 새로운 순찰 지점 설정
            controller.agent.SetDestination(patrolPoint); // NavMeshAgent를 통해 새로운 지점으로 이동
        }

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        if (distanceToPlayer < controller.chaseRadius) // 감지 거리 내에 플레이어가 있는지 확인
        {
            controller.ChangeState(new ChaseState(controller, utilityCalculator)); // 추적 상태로 전환
        }
    }

    /// <summary>
    /// 상태 종료 시 호출되는 메서드. 필요한 경우 종료 로직을 추가할 수 있습니다.
    /// </summary>
    public void Exit()
    {
        // 순찰 종료 시 추가 로직이 필요한 경우 여기에 작성
    }

    /// <summary>
    /// 무작위 순찰 지점을 생성하는 메서드. NavMesh 내에서 유효한 위치를 반환합니다.
    /// </summary>
    /// <returns>유효한 순찰 지점의 Vector3 위치</returns>
    private Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * controller.patrolRadius; // 반경 내 무작위 방향 계산
        randomDirection += controller.transform.position; // 현재 위치를 기준으로 위치 설정
        NavMeshHit hit;
        // NavMesh 내에서 무작위 지점 샘플링
        NavMesh.SamplePosition(randomDirection, out hit, controller.patrolRadius, NavMesh.AllAreas);
        return hit.position; // 유효한 순찰 지점 반환
    }
} 