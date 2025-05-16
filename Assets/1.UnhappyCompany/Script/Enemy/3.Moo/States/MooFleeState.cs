using UnityEngine;

/// <summary>
/// MooFleeState 클래스는 MooAIController가 도망 상태에 있을 때의 행동을 정의합니다.
/// </summary>
public class MooFleeState : IState
{
    private MooAIController controller; // MooAIController 인스턴스
    private float fleeDuration = 15f; // 도망 상태 지속 시간
    
    private float fleeStartTime; // 도망 상태 시작 시간

    /// <summary>
    /// MooFleeState의 생성자. MooAIController를 매개변수로 받습니다.
    /// </summary>
    /// <param name="controller">MooAIController 인스턴스</param>
    public MooFleeState(MooAIController controller)
    {
        this.controller = controller;
    }

    /// <summary>
    /// 도망 상태에 진입할 때 호출됩니다.
    /// </summary>
    public void Enter()
    {
        DebugManager.Log("Moo: Flee 상태 시작", controller.isShowDebug);
        controller.PlayAnimation("Flee"); // 도망 애니메이션 재생
        fleeStartTime = Time.time; // 도망 시작 시간 기록
        SetFleeDestination(); // 도망 목적지 설정
    }

    /// <summary>
    /// 아침 시간 동안 도망 상태를 실행합니다.
    /// </summary>
    public void ExecuteMorning()
    {
        // 도망 상태가 지속 시간을 초과하면 Idle 상태로 전환
        if (Time.time - fleeStartTime > fleeDuration)
        {
            controller.ChangeState(new MooIdleState(controller));
            return;
        }
        
        // 목적지에 도달했는지 확인
        if (controller.agent.enabled && !controller.agent.pathPending && 
            controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            // 목적지에 도달했으면 Idle 상태로 전환
            controller.ChangeState(new MooIdleState(controller));
        }
    }

    /// <summary>
    /// 오후 시간 동안 도망 상태를 실행합니다.
    /// </summary>
    public void ExecuteAfternoon()
    {
        ExecuteMorning(); // 아침과 동일한 로직 실행
    }

    /// <summary>
    /// 도망 상태에서 나갈 때 호출됩니다.
    /// </summary>
    public void Exit()
    {
        DebugManager.Log("Moo: Flee 상태 종료", controller.isShowDebug);
        controller.agent.speed = controller.EnemyData.moveSpeed;
        controller.agent.enabled = false;

    }

    /// <summary>
    /// 도망 목적지를 설정합니다.
    /// </summary>
    private void SetFleeDestination()
    {
        var point = controller.GenerateRandomPatrolPoint();
        controller.agent.enabled = true;
        float fleeSpeed = controller.EnemyData.fleeSpeed;
        controller.agent.speed = fleeSpeed;
        controller.agent.SetDestination(point);
    }

    /// <summary>
    /// 고정된 아침 시간 동안의 도망 상태를 실행합니다. 필요에 따라 구현합니다.
    /// </summary>
    public void ExecuteFixedMorning()
    {
        // 필요에 따라 구현
    }

    /// <summary>
    /// 고정된 오후 시간 동안의 도망 상태를 실행합니다. 필요에 따라 구현합니다.
    /// </summary>
    public void ExecuteFixedAfternoon()
    {
        // 필요에 따라 구현
    }
} 