using UnityEngine;

/// <summary>
/// RSP 적의 비활성화 상태를 관리하는 클래스입니다.
/// 스택이 0이 되고 쿨다운 중일 때 이 상태에 머무릅니다.
/// </summary>
public class RSPDisableState : IState
{
    private EnemyAIRSP controller;

    public RSPDisableState(EnemyAIRSP controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("RSP: 비활성화 상태 시작");
        
        // NavMeshAgent 정지
        if (controller.agent != null && controller.agent.enabled)
        {
            controller.agent.isStopped = true;
            controller.agent.ResetPath();
        }
        
        // Speed를 0으로 설정 (Blend Tree에서 Idle 상태 유지)
        controller.SetSpeed(0f);
        
        // 에미션 비활성화
        controller.DisableEmission();
        
        // 쿨다운 시작
        controller.StartCooldown();
    }

    public void ExecuteMorning()
    {
        // 쿨다운이 끝나면 순찰 상태로 복귀
        if (!controller.isCoolDown && controller.GetCompulsoryPlayStack() == 0)
        {
            Debug.Log("RSP: 쿨다운 종료, 순찰 상태로 전환");
            controller.ChangeState(new RSPPatrolState(controller));
        }
        
        // 스택이 증가하면 순찰 상태로 복귀
        if (controller.GetCompulsoryPlayStack() > 0)
        {
            Debug.Log("RSP: 스택 증가 감지, 순찰 상태로 전환");
            controller.ChangeState(new RSPPatrolState(controller));
        }
    }

    public void ExecuteAfternoon()
    {
        // 쿨다운이 끝나면 순찰 상태로 복귀
        if (!controller.isCoolDown && controller.GetCompulsoryPlayStack() == 0)
        {
            Debug.Log("RSP: 쿨다운 종료, 순찰 상태로 전환");
            controller.ChangeState(new RSPPatrolState(controller));
        }
        
        // 스택이 증가하면 순찰 상태로 복귀
        if (controller.GetCompulsoryPlayStack() > 0)
        {
            Debug.Log("RSP: 스택 증가 감지, 순찰 상태로 전환");
            controller.ChangeState(new RSPPatrolState(controller));
        }
    }

    public void Exit()
    {
        Debug.Log("RSP: 비활성화 상태 종료");
        
        // NavMeshAgent 재개
        if (controller.agent != null && controller.agent.enabled)
        {
            controller.agent.isStopped = false;
        }
        
        // 에미션 활성화
        controller.EnableEmission();
    }

    public void ExecuteFixedMorning()
    {
        // 물리 업데이트 로직 없음
    }

    public void ExecuteFixedAfternoon()
    {
        // 물리 업데이트 로직 없음
    }
}

