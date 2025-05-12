using UnityEngine;
using UnityEngine.AI;

public class RampagePatrolState : IState
{
    private RampageAIController controller;
    private NavMeshAgent agent;
    private float patrolSpeed;

    // TODO: 순찰 지점 리스트나 랜덤 포인트 등을 받아 순찰 로직 구성
    // 여기서는 임시로 랜덤 반경 내 목적지로 이동
    public RampagePatrolState(RampageAIController controller)
    {
        this.controller = controller;
        agent = controller.agent;
        patrolSpeed = controller.EnemyData.moveSpeed;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Patrol 상태 시작");
        agent.enabled = true;
        agent.speed = patrolSpeed;
        SetRandomPatrolDestination();
    }

    public void ExecuteMorning()
    {
        PatrolUpdateLogic();
        // controller.UpdateLineRenderer();
    }

    public void ExecuteAfternoon()
    {
        PatrolUpdateLogic();
        // controller.UpdateLineRenderer();
    }

    public void Exit()
    {
        Debug.Log("Rampage: Patrol 상태 종료");
        controller.DisableLineRenderer();
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void PatrolUpdateLogic()
    {
        if (controller.CheckPlayerInSight())
        {
            controller.ChangeState(new RampageChargeState(controller,"PatrolState"));
            return;
        }
        
        if (!agent.enabled)
        {
            agent.enabled = true;
            return;
        }
        
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            controller.SetRandomPatrolDestination();
        }

        // 플레이어 감지 범위 / 각도 확인
        
    }

    // private bool CheckPlayerDetected()
    // {
    //     if (controller.player == null) return false;

    //     Vector3 toPlayer = controller.player.position - controller.transform.position;
    //     float distance = toPlayer.magnitude;
    //     float angle = Vector3.Angle(controller.transform.forward, toPlayer);

    //     if (distance <= controller.EnemyData.detectRange && angle <= controller.EnemyData.detectAngle)
    //     {
    //         return true;
    //     }
    //     return false;
    // }

    private void SetRandomPatrolDestination()
    {
        Vector3 targetPoint = controller.GenerateRandomPatrolPoint();
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(targetPoint, out hit, controller.EnemyData.patrolRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }
}