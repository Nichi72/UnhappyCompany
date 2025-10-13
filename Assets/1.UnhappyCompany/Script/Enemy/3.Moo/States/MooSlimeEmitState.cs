using UnityEngine;

public class MooSlimeEmitState : IState
{
    private MooAIController controller;
    private bool isShowDebug = false;

    public MooSlimeEmitState(MooAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        DebugManager.Log("Moo: Slime Emit 상태 시작", isShowDebug);
        controller.PlayAnimation("SlimeEmit");
        EmitSlime();
    }

    public void ExecuteMorning()
    {
        controller.ChangeState(new MooWanderState(controller));
    }

    public void ExecuteAfternoon()
    {
        ExecuteMorning();
    }

    public void Exit()
    {
        DebugManager.Log("Moo: Slime Emit 상태 종료", isShowDebug);
    }

    private void EmitSlime()
    {
        // 점액 배출 로직
        GameObject slime = GameObject.Instantiate(controller.SlimePrefab, controller.transform.position, Quaternion.identity);
        GameObject.Destroy(slime, controller.SlimeDuration);
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