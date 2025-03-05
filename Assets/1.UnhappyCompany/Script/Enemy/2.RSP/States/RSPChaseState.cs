using UnityEngine;

public class RSPChaseState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;
    private float stoppingDistance = 2.0f; // 플레이어와의 최소 거리
    private bool isRage = false;
    private float currentSpeed = 0;
    private float speedOfNormal = 3.5f;
    private float speedOfRage = 12f;


    public RSPChaseState(EnemyAIRSP controller, UtilityCalculator calculator, float stoppingDistance = 2.0f)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        this.stoppingDistance = stoppingDistance;
        controller.agent.speed = currentSpeed;
        controller.StartCheckCompulsoryPlayStack();
    }

    public void Enter()
    {
        // Debug.Log("RSP: 추적 상태 시작");
    }

    public void ExecuteMorning()
    {
        // Debug.Log("RSP: 오전 추적 중");
        FollowPlayer();
        ExceptionalFollowPlayer();
    }

    public void ExecuteAfternoon()
    {
        // Debug.Log("RSP: 오후 추적 중");
        FollowPlayer();
        ExceptionalFollowPlayer();
    }

    public void Exit()
    {
        // Debug.Log("RSP: 추적 상태 종료");
    }

    public void ExecuteFixedMorning()
    {
    }

    public void ExecuteFixedAfternoon()
    {
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        
        if (distanceToPlayer > stoppingDistance)
        {
            controller.agent.SetDestination(controller.player.position);
        }
        else
        {
            // 플레이어 근처에 도달하면 서서히 멈춤
            controller.agent.velocity = Vector3.Lerp(controller.agent.velocity, Vector3.zero, Time.deltaTime * 5f);
            if (controller.agent.velocity.magnitude < 0.1f)
            {
                controller.agent.ResetPath();
            }
        }
    }

    private Vector3 lastPosition;
    private float lastMovedTime;
    private float stuckCheckTime = 3f; // 멈춤 감지 시간
    private void ExceptionalFollowPlayer()
    {
        float movedDistance = Vector3.Distance(controller.transform.position, lastPosition);
        if (movedDistance > 0.1f) // 의미있는 이동이 있었다면
        {
            lastPosition = controller.transform.position;
            lastMovedTime = Time.time;
        }
        else if (Time.time - lastMovedTime > stuckCheckTime) // 일정 시간동안 이동이 없었다면
        {
            Debug.LogError("RSP: 비정상적인 멈춤 감지, 경로 재탐색");
            controller.agent.ResetPath();
            controller.agent.SetDestination(controller.player.position);
            lastMovedTime = Time.time;
        }
    }

    
} 