using UnityEngine;

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
        // TODO: 무력화 애니메이션, 효과, 보상 로직(아이템 드롭 등)
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
        // 필요에 따라 영구 방치도 가능, 여기서는 5초 후 제거 예시
        if (Time.time - disableStartTime > removeDelay)
        {
            // AI 제거
            GameObject.Destroy(controller.gameObject);
        }
    }
} 