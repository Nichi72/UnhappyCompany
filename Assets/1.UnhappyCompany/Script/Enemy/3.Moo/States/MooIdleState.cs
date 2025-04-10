using UnityEngine;
using UnityEngine.AI;

public class MooIdleState : IState
{
    private MooAIController controller;
    private bool isShowDebug = false;

    public MooIdleState(MooAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        DebugManager.Log("Moo: Idle 상태 시작", isShowDebug);
        controller.PlayAnimation(controller.idleAnimationName);
    }

    public void ExecuteMorning()
    {
        if (controller.agent.enabled && controller.agent.isOnNavMesh)
        {
            if (controller.agent.remainingDistance < 0.5f)
            {
                SetRandomDestination();
            }
        }
    }

    public void ExecuteAfternoon()
    {
        ExecuteMorning();
    }

    public void Exit()
    {
        DebugManager.Log("Moo: Idle 상태 종료", isShowDebug);
    }

    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * controller.PatrolRadius;
        randomDirection += controller.transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, controller.PatrolRadius, 1))
        {
            controller.agent.SetDestination(hit.position);
        }
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