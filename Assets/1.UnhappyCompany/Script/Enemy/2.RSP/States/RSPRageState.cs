using UnityEngine;

public class RSPRageState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;
    private float rageSpeed = 15f; // 화난 상태의 속도
    private float stoppingDistance = 1.5f; // 플레이어와의 최소 거리
    private Player player;

    public RSPRageState(EnemyAIRSP controller, UtilityCalculator calculator, Player player)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        controller.agent.speed = rageSpeed;
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("RSP: 화난 상태 시작");
    }

    public void ExecuteMorning()
    {
        controller.FollowTarget(stoppingDistance ,player.transform.position, () =>
        {
            controller.ChangeState(new RSPHoldingState(controller, utilityCalculator, player, controller.rspSystem));
        });
        
    }

    public void ExecuteAfternoon()
    {
        controller.FollowTarget(stoppingDistance);
    }

    public void Exit()
    {
        Debug.Log("RSP: 화난 상태 종료");
    }

    public void ExecuteFixedMorning()
    {
    }

    public void ExecuteFixedAfternoon()
    {
    }

    
}
