using System.Collections;
using UnityEngine;

public class RampagePanelOpenState : IState
{
    private RampageAIController controller;
    // private 

    private float panelOpenDuration;
    private int panelCount; // 쿠션 충돌 시 3, 아니면 1


    public RampagePanelOpenState(RampageAIController controller, int panelCount, string beforeState)
    {
        Debug.Log($"{beforeState} 상태에서 패널 열기 시작");
        this.controller = controller;
        this.panelCount = panelCount;

        panelOpenDuration = controller.enemyData.panelOpenTime;
        controller.chargeCount--;
    }

    public void Enter()
    {
        Debug.Log("Rampage: PanelOpen 상태 시작");
        
        controller.StartCoroutine(OpenPanelCoroutine());
    }

    public void ExecuteMorning()
    {
        // UpdatePanelOpen();
    }

    public void ExecuteAfternoon()
    {
        // UpdatePanelOpen();
    }

    public void Exit()
    {
        Debug.Log("Rampage: PanelOpen 상태 종료");
        // TODO: 애니메이션/사운드 정리
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private IEnumerator OpenPanelCoroutine()
    {
        Debug.Log("OpenPanelCoroutine");
        RampagePanel panel = OpenPanel();
        yield return new WaitForSeconds(panelOpenDuration);
        ClosePanel(panel);
        controller.ChangeState(new RampageChargeState(controller,"PanelOpenState"));
    }

    private RampagePanel GetRandomPanel()
    {
        int randomIndex = Random.Range(0, controller.panels.Count);
        return controller.panels[randomIndex];
    }
    private RampagePanel OpenPanel()
    {
        RampagePanel panel = GetRandomPanel();
        panel.gameObject.SetActive(true);
        panel.OpenPanel();
        return panel;
    }

    private void ClosePanel(RampagePanel panel)
    {
        panel.gameObject.SetActive(false);
    }
} 