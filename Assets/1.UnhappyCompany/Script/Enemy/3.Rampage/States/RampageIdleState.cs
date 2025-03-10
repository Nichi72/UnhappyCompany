using UnityEngine;

public class RampageIdleState : IState
{
    private RampageAIController controller;
    private float idleTimer = 2f; // 대기 후 순찰로 넘어가기 위한 시간
    private float startTime;

    public RampageIdleState(RampageAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Idle 상태 시작");
        startTime = Time.time;
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
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void UpdateLogic()
    {
        // 플레이어 감지 시 Charge 상태로 전환
        if (CheckPlayerDetected())
        {
            controller.ChangeState(new RampageChargeState(controller));
            return;
        }

        // 일정 시간 후 순찰 상태로 전환
        if (Time.time - startTime > idleTimer)
        {
            controller.ChangeState(new RampagePatrolState(controller));
        }
    }

    private bool CheckPlayerDetected()
    {
        if (controller.player == null) return false;

        Vector3 toPlayer = controller.player.position - controller.transform.position;
        float distance = toPlayer.magnitude;
        float angle = Vector3.Angle(controller.transform.forward, toPlayer);

        if (distance <= controller.enemyData.detectRange && angle <= controller.enemyData.detectAngle)
        {
            return true;
        }
        return false;
    }
} 