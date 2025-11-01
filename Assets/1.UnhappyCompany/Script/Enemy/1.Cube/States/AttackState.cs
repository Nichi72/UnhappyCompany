using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AttackState는 적이 플레이어를 공격하는 상태를 나타냅니다.
/// 공격 쿨타임과 시전시간을 관리하며, 공격 범위를 벗어나면 상태를 전환합니다.
/// </summary>
public class CubeAttackState : IState
{
    private Rigidbody rigidbody;
    private EnemyAICube controller;
    private UtilityCalculator utilityCalculator;
    private float lastAttackTime;
    private float attackCastingStartTime; // 공격 시전 시작 시간
    private bool isAttackCasting; // 공격 시전 중인지 여부
    private bool isReenablingAgent; // Agent 재활성화 중인지 여부
    private bool hasStartedChargingScale; // 차징 크기 변화 시작 여부


    public CubeAttackState(EnemyAICube controller, UtilityCalculator calculator)
    {
        this.controller = controller;
        this.utilityCalculator = calculator;
        this.rigidbody = controller.GetComponent<Rigidbody>();
    }

    public void Enter()
    {
        // DisableAgentSafely();
        lastAttackTime = Time.time;
        isAttackCasting = false;
        isReenablingAgent = false;
        hasStartedChargingScale = false;
        
        // 공격 상태 시작
        controller.SetAttackingState(true);
    }

    /// <summary>
    /// 플레이어 Head 위치를 가져옵니다 (없으면 플레이어 위치 반환)
    /// </summary>
    private Vector3 GetPlayerHeadPosition()
    {
        if (controller.playerTr == null)
            return controller.transform.position;

        Player player = controller.playerTr.GetComponent<Player>();
        if (player != null && player.Head != null)
        {
            return player.Head.transform.position;
        }

        // Head가 없으면 플레이어 위치 반환
        return controller.playerTr.position;
    }

    public void Exit()
    {
        // EnableAgentSafely();
        isAttackCasting = false;
        
        // 상태 종료 시 돌진 상태 비활성화
        controller.SetRushingState(false);
        
        // 공격 상태 종료
        controller.SetAttackingState(false);
        
        // 크기 복원
        controller.ResetScale();
    }

    public void ExecuteMorning()
    {
        // 빈 메서드
    }

    public void ExecuteAfternoon() 
    {
        // 빈 메서드
    }
    

    public void ExecuteFixedMorning()
    {
        ExecuteFixedUpdate();
        
    }

    public void ExecuteFixedAfternoon()
    {
        ExecuteFixedUpdate();
    }

    private void ExecuteFixedUpdate()
    {
        if(isReenablingAgent)
        {
            // Debug.Log("Cube: Agent 재활성화 대기중...");
            return;
        }

        // 공격 쿨다운이 끝나고 Agent 재활성화 중이 아닐 때만 공격 시전 시작
        if (!isAttackCasting &&  Time.time - lastAttackTime >= controller.AttackCooldown)
        {
            isAttackCasting = true;
            attackCastingStartTime = Time.time;
            hasStartedChargingScale = false; // 차징 스케일 플래그 초기화
            ChangeMaterialColor(Color.red);
            
            // 공격 시작 - BlendShape 100 -> 0 애니메이션 시작
            controller.StartAttackBlendShape();
            
            // NavMeshAgent를 완전히 비활성화 (Y축 이동을 방해하지 않도록)
            if (controller.agent != null && controller.agent.enabled)
            {
                controller.agent.isStopped = true;
                controller.agent.enabled = false;
                Debug.Log("Cube: NavMeshAgent 비활성화");
            }
            
            Debug.Log("Cube: 공격 시전 시작!");
        }

        // 공격 시전 중: 플레이어 Head를 향해 회전
        if (isAttackCasting && Time.time - attackCastingStartTime < controller.AttackCastingTime)
        {
            Vector3 playerHeadPos = GetPlayerHeadPosition();
            Vector3 directionToPlayer = (playerHeadPos - controller.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
            
            // 발사 직전에 크기 줄이기 (한 번만)
            float remainingTime = controller.AttackCastingTime - (Time.time - attackCastingStartTime);
            if (!hasStartedChargingScale && remainingTime <= controller.scaleChargingStartBeforeRush)
            {
                controller.StartAttackChargingScale();
                hasStartedChargingScale = true;
            }
        }

        // 공격 시전 시간이 끝나면 플레이어 Head로 돌진 공격 실행
        if (isAttackCasting && Time.time - attackCastingStartTime >= controller.AttackCastingTime)
        {
            ResetMaterialColor();
            Vector3 playerHeadPos = GetPlayerHeadPosition();
            Vector3 directionToPlayer = (playerHeadPos - controller.transform.position).normalized;
            Debug.DrawRay(controller.transform.position, directionToPlayer * controller.RushForce, Color.red, 1f);

            // 물리 기반 돌진 공격 실행
            if (rigidbody != null)
            {
                // 돌진 상태 활성화 (데미지 트리거용)
                controller.SetRushingState(true);
                
                // 공격 러쉬 - 크기 복원
                controller.StartAttackRushScale();
                
                // Rigidbody 제약 해제 (Y축 이동 허용)
                rigidbody.constraints = RigidbodyConstraints.None;
                rigidbody.isKinematic = false;
                rigidbody.AddForce(directionToPlayer * controller.RushForce, ForceMode.Impulse);
                var random = Random.value < 0.5f ? -0.4f : 0.4f;
                rigidbody.AddTorque((Vector3.up + Vector3.right * random) * controller.TorqueForce * 10f, ForceMode.Impulse);
                Debug.Log($"Cube: 돌진! 방향: {directionToPlayer}, 힘: {controller.RushForce}");
            }

            // 돌진 후 잠시 대기 후 NavMeshAgent 재활성화
            isReenablingAgent = true; // Agent 재활성화 시작
            controller.StartCoroutine(ReEnableAgentAfterDelay(controller.ReEnableDelay));

            lastAttackTime = Time.time;
            isAttackCasting = false;
            Debug.Log("Cube: 공격 실행!");
        }
    }
   
    private Material originalMaterial;
    private Color originalColor;

    private void ChangeMaterialColor(Color newColor)
    {
        if (controller.GetComponent<Renderer>() != null)
        {
            Material material = controller.GetComponent<Renderer>().material;
            if (originalMaterial == null)
            {
                originalMaterial = material;
                originalColor = material.color;
            }
            material.color = newColor;
            Debug.Log("메테리얼 색상이 변경되었습니다.");
        }
        else
        {
            Debug.LogWarning("Renderer 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void ResetMaterialColor()
    {
        if (originalMaterial != null && controller.GetComponent<Renderer>() != null)
        {
            controller.GetComponent<Renderer>().material.color = originalColor;
            Debug.Log("메테리얼 색상이 초기화되었습니다.");
        }
        else
        {
            Debug.LogWarning("원본 메테리얼이 없거나 Renderer 컴포넌트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 돌진 후 안정화 대기 → 일정 시간 대기 → NavMeshAgent 재활성화 및 상태 판단
    /// </summary>
    private System.Collections.IEnumerator ReEnableAgentAfterDelay(float delay)
    {
        // 타이머 시작 (안정화 대기 상태)
        controller.StartReEnableTimer(delay, true);
        Debug.Log("Cube: 안정화 대기 중...");
        
        // 1단계: 돌진 시작 후 1초 대기 (초기 속도 0 방지)
        float initialDelay = 1f;
        yield return new WaitForSeconds(initialDelay);
        
        // 2단계: 속도 기반 안정화 체크
        float stabilizedSpeedThreshold = 0.5f; // 이 속도 이하면 안정화로 판단
        float checkInterval = 0.1f; // 0.1초마다 체크
        
        Debug.Log("Cube: 속도 기반 안정화 체크 시작...");
        
        while (rigidbody != null && rigidbody.linearVelocity.magnitude > stabilizedSpeedThreshold)
        {
            // 디버그용 속도 출력 (필요시)
            // Debug.Log($"Cube 현재 속도: {rigidbody.linearVelocity.magnitude:F2}");
            yield return new WaitForSeconds(checkInterval);
        }
        
        // 안정화 감지
        controller.OnLandingDetected();
        Debug.Log($"Cube: 안정화 감지! (속도: {rigidbody?.linearVelocity.magnitude:F2}) 이제 {delay}초 대기 시작");
        
        // 3단계: 안정화 후 추가 대기 시간 (타이머 표시하며 대기)
        float elapsed = 0f;
        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            controller.UpdateReEnableTimer(Time.deltaTime);
            yield return null;
        }
        
        // 타이머 종료
        controller.StopReEnableTimer();
        
        // 돌진 상태 비활성화 (데미지 적용 종료)
        controller.SetRushingState(false);
        
        if (controller.agent != null && !controller.agent.enabled)
        {
            // NavMeshAgent를 현재 위치에서 안전하게 재활성화
            controller.agent.enabled = true;
            controller.agent.isStopped = false;
            
            // Rigidbody를 다시 kinematic으로 (NavMesh 제어로 전환)
            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
                rigidbody.linearVelocity = UnityEngine.Vector3.zero;
                rigidbody.angularVelocity = UnityEngine.Vector3.zero;
            }
            
            Debug.Log("Cube: NavMeshAgent 재활성화 완료");
        }
        
        // Agent 재활성화 완료 후 플레이어와의 거리 체크
        isReenablingAgent = false;
        
        if (controller.playerTr != null)
        {
            float distanceToPlayer = UnityEngine.Vector3.Distance(controller.transform.position, controller.playerTr.position);
            
            // 공격 범위 안인지 체크 (시야 안 OR 공격 거리 안)
            bool inSight = controller.CheckPlayerInSight();
            bool inAttackRange = distanceToPlayer <= controller.AttackRadius;
            bool inGiveUpRange = distanceToPlayer <= controller.AttackGiveUpRadius;
            
            if (inSight || inAttackRange)
            {
                // 공격 범위 안 → AttackState 유지 (다시 공격)
                Debug.Log($"Cube: 플레이어가 공격 범위 안에 있음 (거리: {distanceToPlayer:F1}m) - 공격 상태 유지");
            }
            else if (inGiveUpRange)
            {
                // 공격 범위 밖 + 공격 포기 범위 안 → ChaseState (추격해서 공격 범위로 진입)
                Debug.Log($"Cube: 플레이어가 추격 범위 안에 있음 (거리: {distanceToPlayer:F1}m / 공격포기범위: {controller.AttackGiveUpRadius}m) - 추격 상태로 전환");
                controller.ChangeState(new CubeChaseState(controller, utilityCalculator));
            }
            else
            {
                // 모든 범위 밖 → PatrolState (순찰)
                Debug.Log($"Cube: 플레이어가 모든 범위 밖에 있음 (거리: {distanceToPlayer:F1}m) - 순찰 상태로 전환");
                controller.ChangeState(new CubePatrolState(controller, utilityCalculator));
            }
        }
    }
} 