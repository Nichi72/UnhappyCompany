using UnityEngine;

public class RSPChaseState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;

    public RSPChaseState(EnemyAIRSP controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    public void Enter()
    {
        Debug.Log("RSP: 추적 상태 시작");
    }

    public void ExecuteMorning()
    {
        Debug.Log("RSP: 오전 추적 중");
        controller.agent.SetDestination(controller.player.position);
    }

    public void ExecuteAfternoon()
    {
        Debug.Log("RSP: 오후 추적 중");
        controller.agent.SetDestination(controller.player.position);
    }

    public void Exit()
    {
        Debug.Log("RSP: 추적 상태 종료");
    }

    public void ExecuteFixedMorning()
    {
    }

    public void ExecuteFixedAfternoon()
    {
    }
} 