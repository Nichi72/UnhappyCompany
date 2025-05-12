using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyAIController는 적의 기본 AI 동작을 정의하는 추상 클래스입니다.
/// </summary>
public abstract class EnemyAIController : MonoBehaviour, IDamageable, IDamager
{
    [Header("DEBUG")]
    public string currentStateName;
    [SerializeField] public IState currentState; // 현재 활성화된 상태
    protected UtilityCalculator utilityCalculator; // 유틸리티 계산기
    public NavMeshAgent agent; // NavMeshAgent 컴포넌트 참조
    public Transform playerTr; // 플레이어의 Transform 참조
    

    
    [Header("AI Settings")]
    [SerializeField] protected BaseEnemyAIData enemyData;
    
    public virtual BaseEnemyAIData EnemyData => enemyData;
    
    // 공통 프로퍼티
    public float PatrolRadius => EnemyData.patrolRadius;
    public float ChaseRadius => EnemyData.chaseRadius;
    public float AttackRadius => EnemyData.attackRadius;

    [Header("AI GizmoRange Settings")]
    public Color patrolGizmoRangeColor = Color.green;
    public Color chaseGizmoRangeColor = Color.yellow;
    public Color attackGizmoRangeColor = Color.red;

    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public UtilityCalculator UtilityCalculator { get => utilityCalculator; set => utilityCalculator = value; }
    private int _hp = 0;
    public int hp { get => _hp; set => _hp = value; }

    [Header("Debug Settings")]
    public bool enableDebugUI = true;
    public GameObject debugUIPrefab;  // Inspector에서 디버그 UI 프리팹 할당
    [SerializeField] protected EnemyStateDebugUI debugUI;
    public EnemyBudgetFlag budgetFlag;

    [Header("Vision Settings")]
    public EnemyVision vision = new EnemyVision();
    private bool playerDetected = false;
    private Vector3 lastKnownPlayerPosition;

    protected virtual void Start()
    {
        InitializeAI();
    }

    protected virtual void InitializeAI()
    {
        utilityCalculator = new UtilityCalculator();
        agent = GetComponent<NavMeshAgent>();
        playerTr = GameManager.instance.currentPlayer.transform;
        TimeManager.instance.OnTimeOfDayChanged += HandleTimeOfDayChanged;
        CurrentTimeOfDay = TimeManager.instance.CurrentTimeOfDay;
        _hp = enemyData.hpMax;

        if (enableDebugUI)
        {
            InitializeDebugUI();
        }
    }

    protected virtual void InitializeDebugUI()
    {
        if (debugUIPrefab != null)
        {
            var debugUIObject = Instantiate(debugUIPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            debugUI = debugUIObject.GetComponent<EnemyStateDebugUI>();
            if (debugUI != null)
            {
                debugUI.Initialize(transform, patrolGizmoRangeColor, chaseGizmoRangeColor, attackGizmoRangeColor);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (TimeManager.instance != null)
        {
            TimeManager.instance.OnTimeOfDayChanged -= HandleTimeOfDayChanged;
        }

        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.activeEnemies.Remove(gameObject);
        }

        // 디버그 UI 정리
        if (debugUI != null)
        {
            Destroy(debugUI.gameObject);
        }
    }

    protected virtual void HandleTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        CurrentTimeOfDay = newTimeOfDay;
    }

    protected virtual void HandleExecute()
    {
        TimeOfDay currentTimeOfDay = TimeManager.instance.CurrentTimeOfDay;
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Morning:
                currentState?.ExecuteMorning();
                break;
            case TimeOfDay.Afternoon:
                currentState?.ExecuteAfternoon();
                break;
        }
    }

    protected virtual void Update()
    {
        HandleExecute();
        currentStateName = currentState.GetType().Name;

        // 디버그 UI 업데이트
        if (debugUI != null)
        {
            debugUI.UpdateState(currentState.GetType().Name.Replace("State", ""));
        }
    }

    protected virtual void FixedUpdate()
    {
        TimeOfDay currentTimeOfDay = TimeManager.instance.CurrentTimeOfDay;
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Morning:
                currentState?.ExecuteFixedMorning();
                break;
            case TimeOfDay.Afternoon:
                currentState?.ExecuteFixedAfternoon();
                break;
        }
    }

    public virtual void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        MyUtility.UtilityGizmos.DrawCircle(transform.position, PatrolRadius, patrolGizmoRangeColor);
        MyUtility.UtilityGizmos.DrawCircle(transform.position, ChaseRadius, chaseGizmoRangeColor);
        MyUtility.UtilityGizmos.DrawCircle(transform.position, AttackRadius, attackGizmoRangeColor);
    }

    // 에디터에서도 항상 기즈모 표시
    protected virtual void OnDrawGizmos()
    {
        // 시야 기즈모 그리기
        DrawVisionGizmos();
    }
    
    // 시야 기즈모 그리기 메서드
    protected void DrawVisionGizmos()
    {
        if (!vision.drawGizmos) return;
        
        // 시야 방향과 각도 계산
        Vector3 forward = transform.forward;
        Vector3 position = transform.position;
        float sightRange = vision.sightRange;
        float sightHalfAngle = vision.sightAngle * 0.5f;
        
        // 시야 색상 설정
        Gizmos.color = vision.sightColor;
        
        // 부채꼴 형태로 시야각 그리기
        Vector3 leftDirection = Quaternion.Euler(0, -sightHalfAngle, 0) * forward;
        Vector3 rightDirection = Quaternion.Euler(0, sightHalfAngle, 0) * forward;
        
        // 시야 경계선 그리기
        Gizmos.DrawLine(position, position + leftDirection * sightRange);
        Gizmos.DrawLine(position, position + rightDirection * sightRange);
        
        // 부채꼴 그리기
        int segments = 20;
        float angleStep = vision.sightAngle / segments;
        Vector3 prevPoint = position + leftDirection * sightRange;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = -sightHalfAngle + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = position + direction * sightRange;
            
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
        
        // 마지막으로 감지된 플레이어 위치 표시
        if (playerDetected && lastKnownPlayerPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
            Gizmos.DrawLine(position, lastKnownPlayerPosition);
        }
    }
    
    // 플레이어 감지 확인 메서드
    public bool CheckPlayerInSight()
    {
        if (playerTr == null)
        {
            Debug.Log("플레이어가 없습니다.");
            return false;
        }
        
        
        // 1. 거리 체크
        Vector3 directionToPlayer = playerTr.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        if (distanceToPlayer > vision.sightRange)
            return false;
        
        // 2. 각도 체크
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > vision.sightAngle * 0.5f)
            return false;
        
        // 3. 장애물 체크 (레이캐스트)
        if (Physics.Raycast(
            transform.position + Vector3.up, // 눈 높이에서 시작
            directionToPlayer.normalized, 
            out RaycastHit hit, 
            distanceToPlayer,
            vision.obstacleLayer))
        {
            // 레이가 장애물에 먼저 닿았다면 플레이어 감지 실패
            return false;
        }
        
        // 모든 조건 통과 시 플레이어 감지 성공
        playerDetected = true;
        lastKnownPlayerPosition = playerTr.position;
        return true;
    }

    public virtual void TakeDamage(int damage, DamageType damageType)
    {
        hp -= damage;
        if(hp <= 0)
        {
            EnemyManager.instance.activeEnemies.Remove(gameObject);
            Debug.Log($"{gameObject.name} 사망");
            Destroy(gameObject);
        }
    }

    public virtual void DealDamage(int damage, IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Nomal);
    }

    public virtual void AttackCenter()
    {
        Debug.Log($"{gameObject.name} 센터 공격");
    }

    public void FollowTarget(float stoppingDistance = 1f, Vector3 targetPosition = default , Action onArrive = null)
    {
        if (targetPosition == default)
        {
            if (playerTr != null)
            {
                targetPosition = playerTr.position;
            }
            else
            {
                Debug.LogWarning("FollowTarget: 타겟 위치가 지정되지 않았고 플레이어도 없습니다.");
                return;
            }
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        // distanceToTarget += 4f;
        
        if (distanceToTarget > stoppingDistance)
        {
            // 타겟이 정지 거리보다 멀리 있으면 따라감
            agent.stoppingDistance = stoppingDistance;
            agent.SetDestination(targetPosition);
            
            // 이동 중 정지하지 않도록 함
            if (agent.pathPending || agent.remainingDistance > stoppingDistance)
            {
                // 이동 중
            }
            else
            {
                // 목적지에 도달했지만 플레이어가 이동했을 수 있으니 경로 갱신
                agent.SetDestination(targetPosition);
            }
        }
        else
        {
            // 타겟과 충분히 가까운 경우
            agent.velocity = Vector3.Lerp(agent.velocity, Vector3.zero, Time.deltaTime * 5f);
            
            // 그러나 타겟이 다시 멀어지면 즉시 추적을 재개할 수 있도록 경로를 유지
            if (distanceToTarget > stoppingDistance * 1.1f) // 약간의 여유를 두고 재추적
            {
                agent.SetDestination(targetPosition);
            }
            //Debug.Log("도착");

            if(onArrive != null)
            {
                onArrive();
            }
        }
    }

    public void SetRandomPatrolDestination()
    {
        Vector3 targetPoint = GenerateRandomPatrolPoint();
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(targetPoint, out hit, PatrolRadius, 1))
        {
            agent.SetDestination(hit.position);
            Debug.Log($"순찰 위치 설정: {hit.position}");
        }
    }
    
    /// <summary>
    /// NavMesh 위에서 안전한 랜덤 순찰 위치를 생성합니다.
    /// </summary>
    /// <returns>NavMesh 위의 유효한 위치를 반환합니다. 적절한 위치를 찾지 못한 경우 현재 위치를 반환합니다.</returns>
    public Vector3 GenerateRandomPatrolPoint()
    {
        // 안전한 거리 범위를 설정 (전체 반경의 30% ~ 70% 사이)
        float minDistanceRatio = 0.3f; // 중심에서 최소 거리 비율
        float maxDistanceRatio = 0.7f; // 중심에서 최대 거리 비율
        float minDistance = PatrolRadius * minDistanceRatio;
        float maxDistance = PatrolRadius * maxDistanceRatio;
        
        // 최대 시도 횟수 설정
        int maxAttempts = 5;
        int attempts = 0;
        NavMeshHit hit;
        
        while (attempts < maxAttempts)
        {
            // 랜덤 방향 벡터 생성 (방향만 필요)
            Vector3 randomDirection = UnityEngine.Random.onUnitSphere;
            randomDirection.y = 0; // Y축은 수평으로 유지
            randomDirection.Normalize();
            
            // 랜덤 거리 생성 (minDistance와 maxDistance 사이)
            float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);
            
            // 최종 위치 계산
            Vector3 targetPosition = transform.position + randomDirection * randomDistance;
            
            // NavMesh 위의 유효한 위치 확인
            if (NavMesh.SamplePosition(targetPosition, out hit, PatrolRadius, 1))
            {
                // NavMesh 가장자리 체크
                NavMeshHit edgeHit;
                if (NavMesh.FindClosestEdge(hit.position, out edgeHit, 1))
                {
                    // 가장자리로부터의 거리
                    float distanceToEdge = edgeHit.distance;
                    
                    // 가장자리와 충분히 떨어진 경우 (최소 1유닛)
                    if (distanceToEdge >= 1.0f)
                    {
                        Debug.Log($"랜덤 위치 생성: {hit.position}, 가장자리까지 거리: {distanceToEdge}");
                        return hit.position;
                    }
                }
            }
            attempts++;
        }
        
        // 여러 번 시도해도 적절한 위치를 찾지 못한 경우의 대안
        Vector3 fallbackDir = UnityEngine.Random.insideUnitSphere * (PatrolRadius * 0.5f);
        fallbackDir.y = 0;
        fallbackDir += transform.position;
        
        if (NavMesh.SamplePosition(fallbackDir, out hit, PatrolRadius, 1))
        {
            Debug.Log("가장자리 회피 실패, 대안 위치 사용");
            return hit.position;
        }
        
        // 최종 대안으로 현재 위치 반환
        return transform.position;
    }
}

/// <summary>
/// EnemyAIController<T>는 EnemyAIController를 상속받아 특정 타입의 AIData를 사용하는 컨트롤러입니다.
/// </summary>
public abstract class EnemyAIController<T> : EnemyAIController where T : BaseEnemyAIData
{
    [SerializeField] protected new T enemyData;
    
    public override BaseEnemyAIData EnemyData => enemyData;
}

[System.Serializable]
public class EnemyVision
{
    public float sightRange = 10f;       // 시야 거리
    public float sightAngle = 120f;      // 시야각 (전방 기준)
    public LayerMask obstacleLayer;      // 장애물 레이어
    public LayerMask playerLayer;        // 플레이어 레이어
    public bool drawGizmos = true;       // 시야 시각화
    public Color sightColor = new Color(1, 0, 0, 0.3f);
}