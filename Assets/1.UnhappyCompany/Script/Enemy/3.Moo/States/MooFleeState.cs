using UnityEngine;

/// <summary>
/// MooFleeState 클래스는 MooAIController가 도망 상태에 있을 때의 행동을 정의합니다.
/// </summary>
public class MooFleeState : IState
{
    private MooAIController controller; // MooAIController 인스턴스
    private float fleeDuration = 3f; // 도망 상태 지속 시간
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
        Debug.Log("Moo: Flee 상태 시작");
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
        Debug.Log("Moo: Flee 상태 종료");
    }

    /// <summary>
    /// 도망 목적지를 설정합니다.
    /// </summary>
    private void SetFleeDestination()
    {
        // 플레이어로부터 멀어지는 방향으로 도망 목적지 설정
        Vector3 fleeDirection = (controller.transform.position - controller.player.position).normalized;
        Vector3 fleeDestination = controller.transform.position + fleeDirection * 5f;
        controller.agent.SetDestination(fleeDestination);
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