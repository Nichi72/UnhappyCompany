using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Moo의 배회 상태입니다.
/// 랜덤하게 이동하며 플레이어를 감지합니다.
/// </summary>
public class MooWanderState : IState
{
    private MooAIController controller;
    private float wanderTimer;
    private float wanderInterval = 5f; // 5초마다 새로운 목적지

    public MooWanderState(MooAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        DebugManager.Log("Moo: Wander 상태 시작", controller.isShowDebug);
        controller.PlayAnimation(controller.walkAnimationName);
        controller.agent.speed = controller.EnemyData.moveSpeed;
        SetRandomDestination();
    }

    public void ExecuteMorning()
    {
        // 기력 회복은 StaminaUpdateRoutine에서 자동 처리
        
        // 플레이어 감지 체크 (시야 + 청각)
        string detectionType;
        if (controller.DetectPlayerThreat(out detectionType))
        {
            // 감지됨! 지쳐있지 않으면 도망
            if (!controller.IsExhausted)
            {
                DebugManager.Log($"Moo: {detectionType} 감지로 도망 시작!", controller.isShowDebug);
                controller.ChangeState(new MooFleeState(controller));
                return;
            }
            else
            {
                // 지쳐서 도망 못 감
                DebugManager.Log("Moo: 감지했지만 기력이 없어 도망 못 침!", controller.isShowDebug);
            }
        }
        
        // 목적지 도착 시 새로운 목적지 설정
        if (controller.agent.enabled && controller.agent.isOnNavMesh)
        {
            if (!controller.agent.pathPending && controller.agent.remainingDistance <= 0.5f)
            {
                wanderTimer += Time.deltaTime;
                if (wanderTimer >= wanderInterval)
                {
                    SetRandomDestination();
                    wanderTimer = 0f;
                }
            }
        }
    }

    public void ExecuteAfternoon()
    {
        ExecuteMorning();
    }

    public void Exit()
    {
        DebugManager.Log("Moo: Wander 상태 종료", controller.isShowDebug);
    }

    private void SetRandomDestination()
    {
        Vector3 point = controller.GenerateRandomPatrolPoint();
        controller.agent.SetDestination(point);
        
        // 목표 지점 시각화
        controller.currentTargetPosition = point;
        controller.currentTargetLabel = "Wander Target";
        
        DebugManager.Log($"Moo: 새 목적지 설정 - {point}", controller.isShowDebug);
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
