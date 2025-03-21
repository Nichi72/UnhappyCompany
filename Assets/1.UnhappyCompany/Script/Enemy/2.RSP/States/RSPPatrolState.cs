using UnityEngine;
using UnityEngine.AI;

public class RSPPatrolState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;
    private Vector3 currentPatrolPoint;
    private bool isMovingToPoint = false;
    private float stuckCheckTime = 3f; // 멈춤 감지 시간
    private float lastMovedTime; // 마지막 이동 시간
    private Vector3 lastPosition; // 마지막 위치
    private float chaseCooldown; // 추적 상태로 전환하기 전 대기 시간
    private float lastChaseAttemptTime; // 마지막 추적 시도 시간
    

    public RSPPatrolState(EnemyAIRSP controller, UtilityCalculator calculator, float chaseCooldown)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        this.chaseCooldown = chaseCooldown;
        this.lastChaseAttemptTime = Time.time; // 현재 시간으로 초기화
        
    }

    public void Enter()
    {
        Debug.Log("RSP: 순찰 상태 시작");
        lastPosition = controller.transform.position;
        lastMovedTime = Time.time;
        lastChaseAttemptTime = Time.time; // 상태 진입 시 초기화
        SetNewDestination();
    }

    public void ExecuteMorning()
    {
        PatrolBehavior();
    }

    public void ExecuteAfternoon()
    {
        PatrolBehavior();
    }

    private void PatrolBehavior()
    {
        // 플레이어가 감지 범위 내에 있는지 체크
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        if (distanceToPlayer < controller.ChaseRadius && Time.time - lastChaseAttemptTime > chaseCooldown)
        {
            lastChaseAttemptTime = Time.time;
            controller.ChangeState(new RSPChaseState(controller, utilityCalculator));
            return;
        }

        // 이동 상태 체크
        CheckIfStuck();

        // 목적지에 도달했는지 체크
        if (!isMovingToPoint || 
            Vector3.Distance(controller.transform.position, currentPatrolPoint) < 1f)
        {
            SetNewDestination();
        }
    }

    private void CheckIfStuck()
    {
        float movedDistance = Vector3.Distance(controller.transform.position, lastPosition);
        if (movedDistance > 0.1f) // 의미있는 이동이 있었다면
        {
            lastPosition = controller.transform.position;
            lastMovedTime = Time.time;
        }
        else if (Time.time - lastMovedTime > stuckCheckTime) // 일정 시간동안 이동이 없었다면
        {
            Debug.Log("RSP: 이동 정체 감지, 새로운 목적지 설정");
            SetNewDestination();
            lastMovedTime = Time.time;
        }
    }

    private void SetNewDestination()
    {
        // NavMesh 위의 랜덤한 위치 찾기
        Vector3 randomPoint = Random.insideUnitSphere * controller.PatrolRadius;
        randomPoint += controller.transform.position;
        NavMeshHit hit;

        // NavMesh 위의 유효한 위치 찾기
        if (NavMesh.SamplePosition(randomPoint, out hit, controller.PatrolRadius, NavMesh.AllAreas))
        {
            currentPatrolPoint = hit.position;
            controller.agent.SetDestination(currentPatrolPoint);
            isMovingToPoint = true;
        }
        else
        {
            // 유효한 위치를 찾지 못한 경우 짧은 대기 후 다시 시도
            isMovingToPoint = false;
        }
    }

    public void Exit()
    {
        Debug.Log("RSP: 순찰 상태 종료");
    }

    public void ExecuteFixedMorning()
    {
    }

    public void ExecuteFixedAfternoon()
    {
    }
} 