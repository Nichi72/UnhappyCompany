using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Cube 적의 추적 상태를 관리하는 클래스입니다.
/// NavMeshAgent를 사용하여 플레이어의 위치를 지속적으로 추적합니다.
/// </summary>
public class CubeChaseState : IState
{
    private EnemyAICube controller;
    private UtilityCalculator utilityCalculator;
    private float stoppingDistance = 5f; // 플레이어와의 최소 거리

    /// <summary>
    /// Cube 추적 상태 생성자
    /// </summary>
    /// <param name="controller">EnemyAICube 참조</param>
    /// <param name="calculator">UtilityCalculator 참조</param>
    public CubeChaseState(EnemyAICube controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    /// <summary>
    /// 추적 상태 진입 시 호출되는 메서드
    /// </summary>
    public void Enter()
    {
        Debug.Log("Cube: 추적 상태 시작");
    }

    /// <summary>
    /// 오전 시간대의 추적 상태 실행 로직
    /// </summary>
    public void ExecuteMorning()
    {
        HandleChaseLogic();
    }

    /// <summary>
    /// 오후 시간대의 추적 상태 실행 로직 (더 빠른 이동 속도)
    /// </summary>
    public void ExecuteAfternoon()
    {
        // 오후에는 추적 속도 증가
        float originalSpeed = controller.agent.speed;
        controller.agent.speed = originalSpeed * controller.ChaseSpeedMultiplier;
        HandleChaseLogic();
    }

    /// <summary>
    /// 추적 행동의 핵심 로직
    /// 플레이어를 추적하고 공격 범위 확인
    /// </summary>
    private void HandleChaseLogic()
    {
        // 플레이어를 향해 이동
        controller.FollowTarget(stoppingDistance);
        
        // 플레이어 감지 (근접 감지 또는 시야 감지)
        if (controller.DetectPlayer())
        {
            Debug.Log("Cube: 플레이어 공격 범위 감지! 공격 상태로 전환");
            controller.ChangeState(new CubeAttackState(controller, utilityCalculator));
        }
        // 공격 포기 범위를 벗어나면 순찰 상태로 전환
        else
        {
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.playerTr.position);
            if (distanceToPlayer > controller.AttackGiveUpRadius)
            {
                Debug.Log($"Cube: 플레이어가 공격 포기 범위 밖 (거리: {distanceToPlayer:F1}m > {controller.AttackGiveUpRadius}m), 순찰 상태로 전환");
                controller.ChangeState(new CubePatrolState(controller, utilityCalculator));
            }
        }
    }

    /// <summary>
    /// 추적 상태 종료 시 호출되는 메서드
    /// </summary>
    public void Exit()
    {
        Debug.Log("Cube: 추적 상태 종료");
    }

    /// <summary>
    /// 물리 업데이트에서 오전 시간대 실행 로직
    /// </summary>
    public void ExecuteFixedMorning()
    {
        // 물리 기반 업데이트 로직 필요시 여기에 구현
    }

    /// <summary>
    /// 물리 업데이트에서 오후 시간대 실행 로직
    /// </summary>
    public void ExecuteFixedAfternoon()
    {
        // 물리 기반 업데이트 로직 필요시 여기에 구현
    }
} 