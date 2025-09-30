using System.Collections;
using UnityEngine;

// 참고: Moo AI의 정확한 상태 베이스 클래스 또는 인터페이스를 확인해야 합니다.
// IState 인터페이스를 사용한다고 가정합니다.
public class MooCenterAttackState : IState
{
    private MooAIController controller; // MooAIController 클래스 확인 필요

    public MooCenterAttackState(MooAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Moo: Center Attack 상태 시작");
        // TODO: 상태 진입 시 로직 구현
        this.controller.agent.enabled = true;
        this.controller.agent.speed = 6f;
    }

    private void AttackCenter()
    {
        controller.FollowTarget(targetPosition: RoomManager.Instance.centerRoom.transform.position);
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

    public void Exit()
    {
        Debug.Log("Moo: Center Attack 상태 종료");
        // TODO: 상태 종료 시 로직 구현
    }
} 