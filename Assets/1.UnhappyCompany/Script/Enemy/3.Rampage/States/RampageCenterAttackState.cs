using UnityEngine;
using UnityEngine.AI;

// 참고: Rampage AI의 정확한 상태 베이스 클래스 또는 인터페이스를 확인해야 합니다.
// BaseState<RampageAIController>를 사용한다고 가정합니다.
public class RampageCenterAttackState : IState
{
    private RampageAIController controller;
    private NavMeshAgent agent;
    private float moveSpeed;

    public RampageCenterAttackState(RampageAIController controller)
    {
        this.controller = controller;
        this.agent = controller.GetComponent<NavMeshAgent>();
        this.moveSpeed = controller.EnemyData.moveSpeed;
        this.controller.agent.speed = 6f;
    }
    private void AttackCenter()
    {
        controller.FollowTarget(targetPosition: RoomManager.Instance.centerRoom.transform.position);
    }

    public void Enter()
    {
        Debug.Log("Rampage: Center Attack 상태 시작");
        // NavMesh 에이전트 활성화
        agent.enabled = true;
        agent.speed = moveSpeed;
    }

    public void Exit()
    {
        Debug.Log("Rampage: Center Attack 상태 종료");
        // 필요하다면 NavMesh 에이전트 비활성화
        agent.enabled = false;
    }

    public void ExecuteFixedUpdate()
    {
        // AttackCenter();
    }

    public void ExecuteMorning()
    {
        AttackCenter();
    }

    public void ExecuteAfternoon()
    {
        AttackCenter();
    }

    public void ExecuteFixedMorning()
    {
        // AttackCenter();
    }

    public void ExecuteFixedAfternoon()
    {
        // AttackCenter();
    }
} 