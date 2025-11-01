using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Cube 적의 순찰 상태를 관리하는 클래스입니다.
/// 이 상태에서 적은 지정된 구역을 랜덤하게 순찰하며 플레이어를 탐지합니다.
/// </summary>
public class CubePatrolState : IState
{
    private EnemyAICube controller;        // Cube 적 컨트롤러 참조
    private UtilityCalculator utilityCalculator;
    private float patrolTimer = 0f;       // 현재 위치에서 대기한 시간
    private float patrolInterval = 1f;    // 다음 순찰 지점으로 이동하기까지 대기 시간
    private Vector3 lastPosition;         // 마지막으로 기록된 위치 (막힘 상태 감지용)
    private float stuckCheckTimer = 0f;   // 막힘 상태 체크 타이머
    private float stuckCheckInterval = 10f; // 막힘 상태 체크 주기 (초)
    private float stuckThreshold = 0.1f;  // 이동 거리가 이 값보다 작으면 막힘 상태로 간주

    /// <summary>
    /// Cube 순찰 상태 생성자
    /// </summary>
    /// <param name="controller">Cube 적 컨트롤러</param>
    /// <param name="calculator">유틸리티 계산기</param>
    public CubePatrolState(EnemyAICube controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        lastPosition = controller.transform.position;
    }

    /// <summary>
    /// 순찰 상태 진입 시 호출되는 메서드
    /// </summary>
    public void Enter()
    {
        Debug.Log("Cube: 순찰 상태 시작");
        
        // 이동 시작 - BlendShape 0 -> 100 애니메이션 시작
        controller.StartMovementBlendShape();
        
        // 상태 진입 시 즉시 순찰 목적지 설정
        controller.SetRandomPatrolDestination();
    }

    /// <summary>
    /// 오전 시간대의 순찰 상태 실행 로직
    /// </summary>
    public void ExecuteMorning()
    {
        // 플레이어 감지 (근접 감지 또는 시야 감지)
        if(controller.DetectPlayer())
        {
            Debug.Log("Cube: 플레이어 감지! 공격 상태로 전환");
            controller.ChangeState(new CubeAttackState(controller, utilityCalculator));
            return;
        }
        
        // 기본 순찰 행동 실행
        PatrolBehavior();
    }

    /// <summary>
    /// 오후 시간대의 순찰 상태 실행 로직
    /// </summary>
    public void ExecuteAfternoon()
    {
        // 플레이어 감지 (근접 감지 또는 시야 감지)
        if(controller.DetectPlayer())
        {
            Debug.Log("Cube: 플레이어 감지! 공격 상태로 전환");
            controller.ChangeState(new CubeAttackState(controller, utilityCalculator));
            return;
        }
        
        // 기본 순찰 행동 실행
        PatrolBehavior();
    }

    /// <summary>
    /// 순찰 행동의 핵심 로직
    /// 목적지 도착 확인 및 새 목적지 설정을 처리합니다.
    /// </summary>
    private void PatrolBehavior()
    {
        // 목적지에 도착했는지 확인 (경로 계산이 완료되고 남은 거리가 정지 거리 이하인 경우)
        if (!controller.agent.pathPending && controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;
            
            // 일정 시간 대기 후 새로운 목적지 설정
            if (patrolTimer >= patrolInterval)
            {
                controller.SetRandomPatrolDestination();
                patrolTimer = 0f;
            }
        }
        
        // 주기적으로 막힘 상태 확인
        CheckIfStuck();
    }

    /// <summary>
    /// 적이 이동 중 막힘 상태인지 확인하는 메서드
    /// 일정 시간 동안 거의 이동하지 않았다면 새 목적지를 설정합니다.
    /// </summary>
    private void CheckIfStuck()
    {
        stuckCheckTimer += Time.deltaTime;
        
        // 주기적으로 위치 확인 (stuckCheckInterval 시간마다)
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            stuckCheckTimer = 0f;
            
            // 마지막 기록 위치와 현재 위치 사이의 이동 거리 계산
            float movedDistance = Vector3.Distance(controller.transform.position, lastPosition);
            
            // 경로가 활성화되어 있지만 거의 움직이지 않은 경우 (막힘 상태)
            if (controller.agent.hasPath && movedDistance < stuckThreshold)
            {
                Debug.Log("Cube: 순찰 중 막힘 감지, 새 목적지 설정");
                controller.SetRandomPatrolDestination();
            }
            
            // 마지막 위치 업데이트
            lastPosition = controller.transform.position;
        }
    }

    /// <summary>
    /// 순찰 상태 종료 시 호출되는 메서드
    /// </summary>
    public void Exit()
    {
        Debug.Log("Cube: 순찰 상태 종료");
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