using UnityEngine;
using UnityEngine.AI;

public class RSPChaseState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;
    private float stoppingDistance = 2.0f; // 플레이어와의 최소 거리
    private bool isRage = false;
    // private float currentSpeed = 0;
    private float speedOfNormal = 3.5f;
    private float speedOfRage = 12f;


    public RSPChaseState(EnemyAIRSP controller)
    {
        this.controller = controller;
        this.stoppingDistance = 5.5f;
        
    }

    public void Enter()
    {
        // Debug.Log("RSP: 추적 상태 시작");
    }

    public void ExecuteMorning()
    {
        // Debug.Log("RSP: 오전 추적 중");
        controller.FollowTarget(stoppingDistance);
        // ExceptionalFollowPlayer();
    }

    public void ExecuteAfternoon()
    {
        // Debug.Log("RSP: 오후 추적 중");
        controller.FollowTarget(stoppingDistance);
        // ExceptionalFollowPlayer();
    }

    public void Exit()
    {
        // Debug.Log("RSP: 추적 상태 종료");
    }

    public void ExecuteFixedMorning()
    {
    }

    public void ExecuteFixedAfternoon()
    {

    }

    

    
} 