using UnityEngine;

/// <summary>
/// Moo의 지침 상태입니다.
/// 기력이 소진되어 도망치지 못하고 제자리에서 쉽니다.
/// </summary>
public class MooExhaustedState : IState
{
    private MooAIController controller;
    private float exhaustedTime;

    public MooExhaustedState(MooAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        DebugManager.Log("Moo: Exhausted 상태 시작 (지쳐서 쉼)", controller.isShowDebug);
        
        // 멈춤
        controller.agent.isStopped = true;
        
        // 목표 지점 제거
        controller.currentTargetPosition = null;
        controller.currentTargetLabel = "";
        
        // 지침 애니메이션 재생 (Cry)
        controller.PlayAnimation(controller.cryAnimationName);
        
        exhaustedTime = 0f;
    }

    public void ExecuteMorning()
    {
        exhaustedTime += Time.deltaTime;
        
        // 기력 회복은 StaminaUpdateRoutine에서... 아니다!
        // 지침 상태에서는 회복이 느려야 함
        // 또는 일정 시간 후 자동으로 배회 복귀
        
        // 기력이 30% 이상 회복되면 배회로 복귀
        if (controller.StaminaPercent >= 0.3f)
        {
            DebugManager.Log("Moo: 기력 회복으로 배회 상태로 복귀!", controller.isShowDebug);
            controller.ChangeState(new MooWanderState(controller));
            return;
        }
        
        // 또는 최소 5초 이상 쉬었고 기력이 조금이라도 있으면 복귀
        if (exhaustedTime >= 5f && !controller.IsExhausted)
        {
            DebugManager.Log("Moo: 충분히 쉬어서 배회 상태로 복귀!", controller.isShowDebug);
            controller.ChangeState(new MooWanderState(controller));
        }
    }

    public void ExecuteAfternoon()
    {
        ExecuteMorning();
    }

    public void Exit()
    {
        DebugManager.Log("Moo: Exhausted 상태 종료", controller.isShowDebug);
        controller.agent.isStopped = false;
    }

    public void ExecuteFixedMorning()
    {
        // 필요에 따라 구현
    }

    public void ExecuteFixedAfternoon()
    {
        // 필요에 따라 구현
    }
}
