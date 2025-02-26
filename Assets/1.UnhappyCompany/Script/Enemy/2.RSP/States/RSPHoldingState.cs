using UnityEngine;

public class RSPHoldingState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;
    private float holdingDuration = 5f; // 홀딩 지속 시간
    private float holdingStartTime;

    public RSPHoldingState(EnemyAIRSP controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
    }

    public void Enter()
    {
        Debug.Log("RSP: 홀딩 상태 시작");
        holdingStartTime = Time.time;
        RSPHolding();
    }

    public void ExecuteMorning()
    {
        Debug.Log("RSP: 오전 홀딩 중");
        CheckHoldingCompletion();
    }

    public void ExecuteAfternoon()
    {
        Debug.Log("RSP: 오후 홀딩 중");
        CheckHoldingCompletion();
    }

    private void RSPHolding()
    {
        // 홀딩 패턴 실행 로직
        // 예: 적이 멈추고 특정 애니메이션을 재생
    }

    private void CheckHoldingCompletion()
    {
        if (Time.time - holdingStartTime >= holdingDuration)
        {
            Debug.Log("RSP: 홀딩 완료, 순찰 상태로 전환");
            controller.ChangeState(new RSPPatrolState(controller, utilityCalculator));
        }
    }

    public void Exit()
    {
        Debug.Log("RSP: 홀딩 상태 종료");
    }

    public void ExecuteFixedMorning()
    {
    }

    public void ExecuteFixedAfternoon()
    {
    }
}
