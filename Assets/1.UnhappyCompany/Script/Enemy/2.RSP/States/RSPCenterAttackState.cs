using UnityEngine;

public class RSPCenterAttackState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;

    public RSPCenterAttackState(EnemyAIRSP controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        this.controller.agent.speed = 8f;
    }

    public void Enter()
    {
        Debug.Log("RSP: Center Attack 상태 시작");
        // TODO: 상태 진입 시 필요한 초기화 로직 구현
    }

    public void ExecuteMorning()
    {
        controller.FollowTarget(targetPosition: RoomManager.Instance.centerRoom.transform.position);
    }

    public void ExecuteAfternoon()
    {
        controller.FollowTarget(targetPosition: RoomManager.Instance.centerRoom.transform.position);
    }

    public void Exit()
    {
        Debug.Log("RSP: Center Attack 상태 종료");
        // TODO: 상태 종료 시 필요한 정리 로직 구현
    }

    public void ExecuteFixedMorning()
    {
        // TODO: 오전 시간대에 FixedUpdate에서 수행할 로직 구현 (물리 관련 등)
    }

    public void ExecuteFixedAfternoon()
    {
        // TODO: 오후 시간대에 FixedUpdate에서 수행할 로직 구현 (물리 관련 등)
    }
} 