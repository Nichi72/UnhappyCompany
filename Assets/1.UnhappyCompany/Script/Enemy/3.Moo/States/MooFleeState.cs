using UnityEngine;

/// <summary>
/// MooFleeState 클래스는 MooAIController가 도망 상태에 있을 때의 행동을 정의합니다.
/// </summary>
public class MooFleeState : IState
{
    private MooAIController controller;
    private float fleeDuration = 15f; // 도망 상태 최대 지속 시간
    private float fleeStartTime; // 도망 상태 시작 시간

    public MooFleeState(MooAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        DebugManager.Log("Moo: Flee 상태 시작", controller.isShowDebug);
        controller.agent.speed = controller.EnemyData.fleeSpeed;
        controller.PlayAnimation("Flee"); // 도망 애니메이션 재생
        fleeStartTime = Time.time;
        SetFleeDestination();
    }

    public void ExecuteMorning()
    {
        // 기력 소모는 StaminaUpdateRoutine에서 자동 처리
        
        // 기력 소진 시 지침 상태로 전환
        if (controller.IsExhausted)
        {
            DebugManager.Log("Moo: 기력 소진으로 지침 상태로 전환!", controller.isShowDebug);
            controller.ChangeState(new MooExhaustedState(controller));
            return;
        }
        
        // 도망 시간 초과 시 배회로 복귀
        if (Time.time - fleeStartTime > fleeDuration)
        {
            DebugManager.Log("Moo: 도망 시간 초과, 배회로 복귀", controller.isShowDebug);
            controller.ChangeState(new MooWanderState(controller));
            return;
        }
        
        // 목적지 도달 시 배회로 복귀
        if (controller.agent.enabled && !controller.agent.pathPending && 
            controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            DebugManager.Log("Moo: 도망 목적지 도달, 배회로 복귀", controller.isShowDebug);
            controller.ChangeState(new MooWanderState(controller));
        }
    }

    /// <summary>
    /// 오후 시간 동안 도망 상태를 실행합니다.
    /// </summary>
    public void ExecuteAfternoon()
    {
        ExecuteMorning(); // 아침과 동일한 로직 실행
    }

    /// <summary>
    /// 도망 상태에서 나갈 때 호출됩니다.
    /// </summary>
    public void Exit()
    {
        DebugManager.Log("Moo: Flee 상태 종료", controller.isShowDebug);
        controller.agent.speed = controller.EnemyData.moveSpeed;
        
        // 목표 지점 시각화 제거
        controller.currentTargetPosition = null;
        controller.currentTargetLabel = "";
    }

    /// <summary>
    /// 도망 목적지를 설정합니다.
    /// 플레이어 반대 방향으로 도망칩니다.
    /// </summary>
    private void SetFleeDestination()
    {
        float fleeSpeed = controller.EnemyData.fleeSpeed;
        controller.agent.speed = fleeSpeed;
        
        Vector3 fleePoint;
        string fleeType;
        
        if (TryGetFleePointAwayFromPlayer(out fleePoint, out fleeType))
        {
            controller.agent.SetDestination(fleePoint);
            controller.currentTargetPosition = fleePoint;
            controller.currentTargetLabel = $"Flee Target\n({fleeType})";
            DebugManager.Log($"Moo: {fleeType} 도망! 목적지: {fleePoint}", controller.isShowDebug);
        }
        else
        {
            // 반대 방향 찾기 실패 시 랜덤 위치로 도망
            var randomPoint = controller.GenerateRandomPatrolPoint();
            controller.agent.SetDestination(randomPoint);
            controller.currentTargetPosition = randomPoint;
            controller.currentTargetLabel = "Flee Target\n(Fallback)";
            DebugManager.Log($"Moo: 반대 방향 찾기 실패, 랜덤 위치로 도망! 목적지: {randomPoint}", controller.isShowDebug);
        }
    }

    /// <summary>
    /// 플레이어 반대 방향으로 도망칠 지점을 찾습니다.
    /// 거리가 너무 짧으면 먼 랜덤 위치로 도망칩니다.
    /// </summary>
    private bool TryGetFleePointAwayFromPlayer(out Vector3 fleePoint, out string fleeType)
    {
        fleePoint = controller.transform.position;
        fleeType = "Unknown";
        
        // 플레이어 위치 확인
        Transform playerTr = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTr == null)
        {
            DebugManager.Log("Moo: 플레이어를 찾을 수 없음", controller.isShowDebug);
            return false;
        }

        // 플레이어 반대 방향 계산
        Vector3 directionAwayFromPlayer = (controller.transform.position - playerTr.position).normalized;
        
        // 도망 거리 설정 (Base 설정 사용)
        float minFleeDistance = controller.FleeDistanceMin; // 최소 도망 거리
        float maxFleeDistance = controller.FleeDistanceMax; // 최대 도망 거리
        float idealFleeDistance = Random.Range(minFleeDistance, maxFleeDistance); // 범위 내 랜덤
        
        Vector3 targetFleePosition = controller.transform.position + directionAwayFromPlayer * idealFleeDistance;
        
        // NavMesh 위에서 유효한 위치 찾기
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetFleePosition, out hit, maxFleeDistance, UnityEngine.AI.NavMesh.AllAreas))
        {
            // 찾은 지점이 플레이어로부터 충분히 멀리 있는지 확인
            float distanceFromPlayer = Vector3.Distance(playerTr.position, hit.position);
            
            if (distanceFromPlayer >= minFleeDistance)
            {
                fleePoint = hit.position;
                fleeType = "Direct Away";
                DebugManager.Log($"Moo: 도망 지점 발견! 플레이어로부터 {distanceFromPlayer:F1}m", controller.isShowDebug);
                return true;
            }
            else
            {
                DebugManager.Log($"Moo: 도망 지점이 너무 가까움 ({distanceFromPlayer:F1}m < {minFleeDistance}m), 먼 랜덤 위치 탐색", controller.isShowDebug);
            }
        }
        
        // 반대 방향으로 충분히 멀리 못 가면, 약간 옆으로도 시도
        for (int i = 0; i < 5; i++)
        {
            // 약간의 랜덤성 추가 (±45도)
            float randomAngle = Random.Range(-45f, 45f);
            Vector3 adjustedDirection = Quaternion.Euler(0, randomAngle, 0) * directionAwayFromPlayer;
            float adjustedDistance = Random.Range(minFleeDistance, maxFleeDistance);
            targetFleePosition = controller.transform.position + adjustedDirection * adjustedDistance;
            
            if (UnityEngine.AI.NavMesh.SamplePosition(targetFleePosition, out hit, maxFleeDistance * 1.5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                float distanceFromPlayer = Vector3.Distance(playerTr.position, hit.position);
                
                if (distanceFromPlayer >= minFleeDistance)
                {
                    fleePoint = hit.position;
                    fleeType = $"Adjusted ±{Mathf.Abs(randomAngle):F0}°";
                    DebugManager.Log($"Moo: 조정된 도망 지점 발견! (각도: {randomAngle:F0}도, 거리: {distanceFromPlayer:F1}m)", controller.isShowDebug);
                    return true;
                }
            }
        }
        
        // 반대 방향으로 충분히 멀리 못 가면 아예 먼 랜덤 위치로 도망
        DebugManager.Log("Moo: 반대 방향 도망 실패, 먼 랜덤 위치로 텔레포트식 도망!", controller.isShowDebug);
        
        // 먼 랜덤 위치 찾기 (Flee Max의 1.5-2배 범위)
        float farMinDistance = maxFleeDistance * 1.5f;
        float farMaxDistance = maxFleeDistance * 2f;
        
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0;
            randomDirection.Normalize();
            
            float randomDistance = Random.Range(farMinDistance, farMaxDistance);
            Vector3 farRandomPosition = controller.transform.position + randomDirection * randomDistance;
            
            if (UnityEngine.AI.NavMesh.SamplePosition(farRandomPosition, out hit, farMaxDistance, UnityEngine.AI.NavMesh.AllAreas))
            {
                float distanceFromPlayer = Vector3.Distance(playerTr.position, hit.position);
                
                // 최소 거리 체크
                if (distanceFromPlayer >= minFleeDistance)
                {
                    fleePoint = hit.position;
                    fleeType = $"Far Random {distanceFromPlayer:F0}m";
                    DebugManager.Log($"Moo: 먼 랜덤 도망 지점 발견! 플레이어로부터 {distanceFromPlayer:F1}m", controller.isShowDebug);
                    return true;
                }
            }
        }
        
        DebugManager.Log("Moo: 유효한 도망 지점을 찾을 수 없음", controller.isShowDebug);
        return false;
    }

    /// <summary>
    /// 고정된 아침 시간 동안의 도망 상태를 실행합니다. 필요에 따라 구현합니다.
    /// </summary>
    public void ExecuteFixedMorning()
    {
        // 필요에 따라 구현
    }

    /// <summary>
    /// 고정된 오후 시간 동안의 도망 상태를 실행합니다. 필요에 따라 구현합니다.
    /// </summary>
    public void ExecuteFixedAfternoon()
    {
        // 필요에 따라 구현
    }
} 