using UnityEngine;
using System.Collections;

public class RampageDisabledState : IState
{
    private RampageAIController controller;
    private float disableStartTime;
    private float removeDelay = 5f; // 예시: 5초 후 제거

    public RampageDisabledState(RampageAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Disabled(무력화) 상태 시작");
        disableStartTime = Time.time;
        
        // 부서지는 시퀀스 시작 (코루틴)
        controller.StartCoroutine(BreakSequence());
    }

    /// <summary>
    /// 부서지는 연출 시퀀스
    /// </summary>
    private IEnumerator BreakSequence()
    {
        // 1단계: 부서지기 직전 소리 재생
        if (AudioManager.instance != null && FMODEvents.instance != null && 
            !FMODEvents.instance.rampageBreakWarning.IsNull)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.rampageBreakWarning, 
                controller.transform,
                "Rampage 부서지기 직전 경고음"
            );
            Debug.Log("부서지기 직전 소리 재생");
        }
        
        // 2단계: 설정된 시간만큼 대기
        float breakDelay = controller.EnemyData.breakDelay;
        Debug.Log($"{breakDelay}초 대기 중...");
        yield return new WaitForSeconds(breakDelay);
        
        // 3단계: 부서지는 소리 재생
        if (AudioManager.instance != null && FMODEvents.instance != null && 
            !FMODEvents.instance.rampageBreak.IsNull)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.rampageBreak,
                controller.transform,
                "Rampage 부서지는 소리"
            );
            Debug.Log("부서지는 소리 재생");
        }
        
        // 4단계: 부서진 비주얼로 교체
        controller.SwitchToDisabledVisual();
        
        // 5단계: 선물상자 드랍
        controller.DropGiftBox();
    }

    public void ExecuteMorning()
    {
        UpdateDisabled();
    }

    public void ExecuteAfternoon()
    {
        UpdateDisabled();
    }

    public void Exit()
    {
        Debug.Log("Rampage: Disabled 상태 종료");
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void UpdateDisabled()
    {
        Debug.Log("해체 작업 처리");
    }
} 