using UnityEngine;

public class RampagePanelOpenState : IState
{
    private RampageAIController controller;
    private float panelOpenStartTime;
    private float panelOpenDuration;
    private int panelCount; // 쿠션 충돌 시 3, 아니면 1

    public RampagePanelOpenState(RampageAIController controller, int panelCount)
    {
        this.controller = controller;
        this.panelCount = panelCount;
    }

    public void Enter()
    {
        Debug.Log("Rampage: PanelOpen 상태 시작");
        panelOpenStartTime = Time.time;
        panelOpenDuration = controller.enemyData.panelOpenTime;
        // 패널 Health 리셋
        controller.ResetPanelHealth(panelCount);
        // TODO: 패널 애니메이션/사운드 재생
    }

    public void ExecuteMorning()
    {
        UpdatePanelOpen();
    }

    public void ExecuteAfternoon()
    {
        UpdatePanelOpen();
    }

    public void Exit()
    {
        Debug.Log("Rampage: PanelOpen 상태 종료");
        // TODO: 애니메이션/사운드 정리
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void UpdatePanelOpen()
    {
        float elapsed = Time.time - panelOpenStartTime;
        if (controller.currentPanelHealth <= 0)
        {
            // 패널 공격이 완료되면 Disabled 상태로 전환
            controller.ChangeState(new RampageDisabledState(controller));
        }
        else if (elapsed > panelOpenDuration)
        {
            // panelOpenTime 경과 시 패널 닫힘 → Stunned 상태로 전환
            controller.ChangeState(new RampageStunnedState(controller));
        }
    }
} 