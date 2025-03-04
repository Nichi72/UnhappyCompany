using UnityEngine;

public class MooFleeState : IState
{
    private MooAIController controller;
    private float fleeDuration = 3f;
    private float fleeStartTime;

    public MooFleeState(MooAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Moo: Flee 상태 시작");
        controller.PlayAnimation("Flee");
        fleeStartTime = Time.time;
        SetFleeDestination();
    }

    public void ExecuteMorning()
    {
        if (Time.time - fleeStartTime > fleeDuration)
        {
            controller.ChangeState(new MooIdleState(controller));
        }
    }

    public void ExecuteAfternoon()
    {
        ExecuteMorning();
    }

    public void Exit()
    {
        Debug.Log("Moo: Flee 상태 종료");
    }

    private void SetFleeDestination()
    {
        Vector3 fleeDirection = (controller.transform.position - controller.player.position).normalized;
        Vector3 fleeDestination = controller.transform.position + fleeDirection * 5f;
        controller.agent.SetDestination(fleeDestination);
    }

    public void ExecuteFixedMorning()
    {
        // 필요에 따라 구현
    }

    public void ExecuteFixedAfternoon()
    {
        // 필요에 따라 구현
    }
} 