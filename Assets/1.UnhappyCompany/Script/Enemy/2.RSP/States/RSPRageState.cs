using UnityEngine;

public class RSPRageState : IState
{
    private EnemyAIRSP controller;
    private UtilityCalculator utilityCalculator;
    private float rageSpeed = 15f; // 화난 상태의 속도
    private float stoppingDistance = 1.0f; // 플레이어와의 최소 거리
    private Player player;

    public RSPRageState(EnemyAIRSP controller, UtilityCalculator calculator, Player player)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        controller.agent.speed = rageSpeed;
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("RSP: 화난 상태 시작");
    }

    public void ExecuteMorning()
    {
        // Debug.Log("RSP: 오전 화난 상태");
        FollowPlayer();
    }

    public void ExecuteAfternoon()
    {
        // Debug.Log("RSP: 오후 화난 상태");
        FollowPlayer();
    }

    public void Exit()
    {
        Debug.Log("RSP: 화난 상태 종료");
    }

    public void ExecuteFixedMorning()
    {
    }

    public void ExecuteFixedAfternoon()
    {
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.player.position);
        if (distanceToPlayer > stoppingDistance)
        {
            controller.agent.SetDestination(controller.player.position);
        }
        else
        {
            // 플레이어 근처에 도달하면 서서히 멈춤
            controller.agent.velocity = Vector3.Lerp(controller.agent.velocity, Vector3.zero, Time.deltaTime * 5f);
            if (controller.agent.velocity.magnitude < 0.1f)
            {
                controller.agent.ResetPath();
                controller.ChangeState(new RSPHoldingState(controller, utilityCalculator, player, controller.rspSystem));
            }
        }
    }
}
