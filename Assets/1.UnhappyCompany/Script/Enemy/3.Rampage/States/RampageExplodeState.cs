using UnityEngine;

public class RampageExplodeState : IState
{
    private RampageAIController controller;
    private bool exploded = false;

    public RampageExplodeState(RampageAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Explode(자폭) 상태 시작");
        // DoExplode();
    }

    public void ExecuteMorning()
    {
        Debug.Log("Rampage: Explode(자폭) 상태 시작");
    }

    public void ExecuteAfternoon()
    {
        
    }

    public void Exit()
    {
        Debug.Log("Rampage: Explode 상태 종료");
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void DoExplode()
    {
      
        // 자폭 후 곧바로 제거
        GameObject.Destroy(controller.gameObject);
    }
} 