using System.Collections;
using UnityEngine;

public class RampagePanelOpenState : IState
{
    private RampageAIController controller;
    private float panelOpenDuration;
    private int panelCount; // 쿠션 충돌 시 3, 아니면 1
    private System.Collections.Generic.List<RampagePanel> openedPanels = new System.Collections.Generic.List<RampagePanel>();


    public RampagePanelOpenState(RampageAIController controller, int panelCount, string beforeState)
    {
        Debug.Log($"{beforeState} 상태에서 패널 열기 시작");
        this.controller = controller;
        this.panelCount = panelCount;

        // 쿠션 충돌 여부에 따라 다른 오픈 시간 적용
        panelOpenDuration = controller.isCushionCollision 
            ? controller.EnemyData.cushionPanelOpenTime 
            : controller.EnemyData.panelOpenTime;
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
        Debug.Log($"OpenPanelCoroutine - {panelCount}개 패널 열기, {panelOpenDuration}초 동안 유지");
        
        // panelCount만큼 패널 열기
        OpenMultiplePanels(panelCount);
        
        yield return new WaitForSeconds(panelOpenDuration);
        
        // 열린 패널들 닫기
        CloseAllPanels();

        // 쿠션 충돌 플래그 리셋
        controller.isCushionCollision = false;
        
        // 돌진 횟수 감소
        controller.chargeCount--;
        
        // 다음 상태 결정
        if (controller.chargeCount > 0)
        {
            // 아직 돌진 횟수가 남았으면 다시 돌진
            Debug.Log($"남은 돌진 횟수: {controller.chargeCount} - 다시 돌진");
            controller.ChangeState(new RampageChargeState(controller, "PanelOpenState"));
        }
        else
        {
            // 돌진 횟수를 모두 소진하면 Idle로 전환하고 리셋
            Debug.Log("돌진 횟수 소진 - Idle 상태로 전환");
            controller.chargeCount = controller.EnemyData.maxChargeCount;
            controller.ChangeState(new RampageIdleState(controller, "PanelOpenState(연속 돌진 종료)"));
        }
    }

    /// <summary>
    /// 지정된 개수만큼 랜덤 패널 열기
    /// </summary>
    private void OpenMultiplePanels(int count)
    {
        // 사용 가능한 패널 목록 생성
        System.Collections.Generic.List<RampagePanel> availablePanels = new System.Collections.Generic.List<RampagePanel>(controller.panels);
        
        // 요청한 개수만큼 패널 열기 (사용 가능한 패널 수를 초과하지 않도록)
        int panelsToOpen = Mathf.Min(count, availablePanels.Count);
        
        for (int i = 0; i < panelsToOpen; i++)
        {
            if (availablePanels.Count == 0) break;
            
            // 랜덤 패널 선택
            int randomIndex = Random.Range(0, availablePanels.Count);
            RampagePanel panel = availablePanels[randomIndex];
            
            // 패널 열기
            panel.gameObject.SetActive(true);
            panel.OpenPanel();
            openedPanels.Add(panel);
            
            // 선택된 패널은 목록에서 제거 (중복 방지)
            availablePanels.RemoveAt(randomIndex);
            
            Debug.Log($"패널 {i + 1}/{panelsToOpen} 열림");
        }
    }

    /// <summary>
    /// 열린 모든 패널 닫기
    /// </summary>
    private void CloseAllPanels()
    {
        foreach (RampagePanel panel in openedPanels)
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(false);
            }
        }
        openedPanels.Clear();
        Debug.Log("모든 패널 닫힘");
    }
} 