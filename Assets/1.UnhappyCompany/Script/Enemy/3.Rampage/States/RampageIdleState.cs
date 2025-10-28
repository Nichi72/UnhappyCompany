using UnityEngine;

public class RampageIdleState : IState
{
    private RampageAIController controller;
    private float idleTimer = 2f; // 대기 후 순찰로 넘어가기 위한 시간
    private float startTime;

    public RampageIdleState(RampageAIController controller,string beforeState)
    {
        Debug.Log($"{beforeState} 상태에서 대기 시작");
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Idle 상태 시작");
        startTime = Time.time;
        
        // Idle 사운드 재생
        controller.PlayIdleSound();
        
        // TODO: 애니메이션 재생(Idle)
    }

    public void ExecuteMorning()
    {
        UpdateLogic();
    }

    public void ExecuteAfternoon()
    {
        UpdateLogic();
    }

    public void Exit()
    {
        Debug.Log("Rampage: Idle 상태 종료");
        
        // Idle 사운드는 Patrol 상태에서도 유지되므로 여기서 정지하지 않음
        // (Charge 상태로 전환될 때 정지됨)
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void UpdateLogic()
    {
        // 시각 감지 또는 근접 감지로 플레이어 발견 시 돌진 상태로 전환
        if (controller.DetectPlayer())
        {
            controller.ChangeState(new RampageChargeState(controller,"IdleState"));
            return;
        }

        // 일정 시간 후 순찰 상태로 전환
        if (Time.time - startTime > idleTimer)
        {
            controller.ChangeState(new RampagePatrolState(controller));
        }
    }

    
} 