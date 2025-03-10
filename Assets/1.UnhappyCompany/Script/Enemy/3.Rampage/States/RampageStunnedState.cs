using UnityEngine;

public class RampageStunnedState : IState
{
    private RampageAIController controller;
    private float stunEndTime;

    public RampageStunnedState(RampageAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Stunned 상태 시작");
        // 지정된 스턴 지속 시간
        stunEndTime = Time.time + controller.enemyData.stunDuration;
        // TODO: 스턴 애니메이션 재생, 사운드 재생
    }

    public void ExecuteMorning()
    {
        UpdateStun();
    }

    public void ExecuteAfternoon()
    {
        UpdateStun();
    }

    public void Exit()
    {
        Debug.Log("Rampage: Stunned 상태 종료");
        // TODO: 스턴 애니메이션 중단
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void UpdateStun()
    {
        if (Time.time >= stunEndTime)
        {
            // 스턴이 끝나면 순찰로 복귀
            controller.ChangeState(new RampagePatrolState(controller));
        }
    }
} 