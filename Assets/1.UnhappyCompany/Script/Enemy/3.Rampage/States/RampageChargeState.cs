using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RampageChargeState : IState
{
    private RampageAIController controller;
    private Rigidbody rb;
    
    private float waitBeforeCharge = 1f; // 돌진 전 대기 시간
    private float chargeSpeed;

    private Coroutine chargeCoroutine;
    private bool isShowDebug = false;
    private Vector3 playerDirection = Vector3.zero;

    private float maxChargeTime = 5f; // 최대 돌진 시간 (초)

    public RampageChargeState(RampageAIController controller, string beforeState)
    {
        DebugManager.Log($"{beforeState} 상태에서 돌진 시작", isShowDebug);
        this.controller = controller;
        this.rb = controller.GetComponent<Rigidbody>();
        chargeSpeed = controller.EnemyData.rushSpeed;
    }

    public void Enter()
    {
        DebugManager.Log("Rampage: Charge 상태 시작", isShowDebug);
        
        // NavMeshAgent 비활성화 및 물리 동작 제한
        controller.agent.enabled = false;
        rb.isKinematic = true;
        
        // 돌진 준비 애니메이션 재생
        AudioManager.instance.PlayTestBeep("돌진 준비 애니메이션 재생", controller.transform);
        
        // 돌진 시작
        chargeCoroutine = controller.StartCoroutine(ChargeCoroutine());
    }

    public void ExecuteMorning()
    {
        // 오브젝트의 회전값의 x와 z축을 0으로 고정
        Vector3 eulerRotation = controller.transform.rotation.eulerAngles;
        eulerRotation.x = 0;
        eulerRotation.z = 0;
        controller.transform.rotation = Quaternion.Euler(eulerRotation);
    }

    public void ExecuteAfternoon() { }

    public void Exit()
    {
        DebugManager.Log("Rampage: Charge 상태 종료", isShowDebug);
        
        // 실행 중인 코루틴이 있다면 중지
        if (chargeCoroutine != null)
        {
            controller.StopCoroutine(chargeCoroutine);
        }
        
        // TODO: 돌진 종료 시 애니메이션, 사운드 정리 등
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    IEnumerator ChargeCoroutine()
    {
        yield return RotateTowardsPlayerCoroutine();
        yield return MoveToPlayerCoroutine();
        yield return ChargePhysicsCoroutine();
        
        // 상태 전환
        if (controller.chargeCount > 0)
        {
            controller.ChangeState(new RampagePanelOpenState(controller, 1, "ChargeState"));
        }
        else
        {
            controller.chargeCount = controller.EnemyData.maxChargeCount;
            controller.ChangeState(new RampageIdleState(controller, "ChargeState(연속 돌진)"));
        }
    }

    // 1단계: 플레이어 방향으로 회전
    private IEnumerator RotateTowardsPlayerCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < waitBeforeCharge)
        {
            elapsedTime += Time.deltaTime;
            
            Vector3 initialDirection = (controller.playerTr.position - controller.transform.position).normalized;
            initialDirection.y = 0;
            
            Quaternion targetRotation = Quaternion.LookRotation(initialDirection);
            controller.transform.rotation = Quaternion.Slerp(
                controller.transform.rotation, 
                targetRotation, 
                Time.deltaTime * 5f
            );
            
            DebugManager.Log("플레이어를 향해 회전중 direction " + initialDirection, isShowDebug);
            yield return null;
        }
        
        controller.onceReduceHP = true;
        DebugManager.Log("회전 완료", isShowDebug);
    }

    // 2단계: NavMeshAgent로 플레이어 방향 이동
    private IEnumerator MoveToPlayerCoroutine()
    {
        Vector3 playerPosition = GameManager.instance.currentPlayer.transform.position;
        bool isPlayerInRange = true;
        
        rb.isKinematic = true;
        controller.agent.enabled = true;
        
        // 동일한 속도값 사용 - rushSpeed
        controller.agent.speed = chargeSpeed;
        Vector3 lastAgentVelocity = Vector3.zero;
        
        DebugManager.Log("playerPosition: " + playerPosition, isShowDebug);
        
        while (true)
        {
            controller.agent.SetDestination(playerPosition);
            
            // 현재 NavMeshAgent의 실제 이동 속도 추적
            if (controller.agent.velocity.magnitude > 0.1f)
            {
                lastAgentVelocity = controller.agent.velocity;
            }
            
            float distance = Vector3.Distance(controller.transform.position, playerPosition);
            
            if (distance <= controller.EnemyData.attackRadius)
            {
                if (isPlayerInRange)
                {
                    isPlayerInRange = false;
                    playerDirection = (playerPosition - controller.transform.position).normalized;
                    yield return null;
                }
                
                if (controller.agent.remainingDistance <= controller.agent.stoppingDistance)
                {
                    // 전환 직전 속도와 방향 저장
                    playerDirection = lastAgentVelocity.normalized;
                    yield return null;
                    break;
                }
            }
            
            yield return null;
        }
        
        // 다음 코루틴으로 현재 속도 정보 전달
        StartPhysicsCharge(lastAgentVelocity);
    }

    // 물리 기반 돌진 시작을 위한 준비
    private void StartPhysicsCharge(Vector3 lastVelocity)
    {
        // NavMeshAgent 비활성화
        controller.agent.enabled = false;
        
        // Rigidbody 활성화
        rb.isKinematic = false;
        
        // 중요: 마지막 NavMeshAgent 속도를 기반으로 초기 Rigidbody 속도 설정
        if (lastVelocity.magnitude > 0.1f)
        {
            // 방향은 유지하되 동일한 크기의 속도 적용
            rb.linearVelocity = lastVelocity.normalized * chargeSpeed;
        }
        else
        {
            // 기존 방식 사용
            rb.linearVelocity = playerDirection * chargeSpeed;
        }
    }

    // 3단계: 물리 기반 돌진
    private IEnumerator ChargePhysicsCoroutine()
    {
        rb.isKinematic = false;
        controller.agent.enabled = false;
        
        // 돌진 시작 시간 기록
        float chargeStartTime = Time.time;
        bool isTimedOut = false;
        
        while (true)
        {
            // 돌진 시간 체크
            float currentChargeTime = Time.time - chargeStartTime;
            
            // 최대 돌진 시간을 초과하면 루프 종료
            if (currentChargeTime > maxChargeTime)
            {
                Debug.Log($"돌진 시간 초과: {currentChargeTime:F1}초. 재시도합니다.");
                isTimedOut = true;
                break;
            }
            
            // 충돌 감지
            if (controller.isCollided)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                DebugManager.Log("돌진 충돌로 종료", isShowDebug);
                break;
            }
            
            // 속도 유지 (필요시에만 방향 조정)
            if (rb.linearVelocity.magnitude < chargeSpeed * 0.9f)
            {
                rb.linearVelocity = playerDirection * chargeSpeed;
            }
            
            yield return null;
        }
        
        // 시간 초과로 종료된 경우 기존 함수를 재사용하여 다시 돌진 시작
        if (isTimedOut)
        {
            // 상태 초기화
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // 돌진 코루틴 재시작
            controller.StopCoroutine(chargeCoroutine);
            chargeCoroutine = controller.StartCoroutine(ChargeCoroutine());
            yield break; // 현재 코루틴 종료
        }
        
        DebugManager.Log("돌진 충돌로 종료후 처리", isShowDebug);
    }

   

    public bool CheckPlayerInPatrolRange()
    {
        if (controller.playerTr == null) return false;

        Vector3 toPlayer = controller.playerTr.position - controller.transform.position;
        float distance = toPlayer.magnitude;

        return distance <= controller.EnemyData.patrolRadius;
    }
}

