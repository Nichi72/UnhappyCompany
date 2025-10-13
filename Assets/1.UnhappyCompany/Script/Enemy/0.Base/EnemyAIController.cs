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
    
    // Range 설정
    public float PatrolDistanceMin => EnemyData.patrolRadius * EnemyData.patrolDistanceMinRatio;
    public float PatrolDistanceMax => EnemyData.patrolRadius * EnemyData.patrolDistanceMaxRatio;
    public float FleeDistanceMin => EnemyData.patrolRadius * EnemyData.fleeDistanceMinRatio;
    public float FleeDistanceMax => EnemyData.patrolRadius * EnemyData.fleeDistanceMaxRatio;
    
    // 시각화 설정
    public Color PatrolRangeColor => EnemyData.patrolRangeColor;
    public Color FleeRangeColor => EnemyData.fleeRangeColor;
    public bool ShowRangesInGame => EnemyData.showRangesInGame;
   

    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public UtilityCalculator UtilityCalculator { get => utilityCalculator; set => utilityCalculator = value; }
    private int _hp = 0;
    public int hp { get => _hp; set => _hp = value; }

    [Header("Debug Settings")]
    // public bool enableDebugUI = true;
    // public GameObject debugUIPrefab;  // Inspector에서 디버그 UI 프리팹 할당
    // [SerializeField] protected EnemyStateDebugUI debugUI;
    [ReadOnly] public EnemyBudgetFlag budgetFlag;

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
    }

    protected virtual void InitializeDebugUI()
    {
       
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
        // Patrol 범위
        Color patrolMinColor = new Color(PatrolRangeColor.r, PatrolRangeColor.g, PatrolRangeColor.b, 0.3f);
        Color patrolMaxColor = new Color(PatrolRangeColor.r, PatrolRangeColor.g, PatrolRangeColor.b, 0.7f);
        
        MyUtility.UtilityGizmos.DrawCircle(transform.position, PatrolDistanceMin, patrolMinColor);
        MyUtility.UtilityGizmos.DrawCircle(transform.position, PatrolDistanceMax, patrolMaxColor);
        
        // Flee 범위 (도망 가능한 Enemy만 표시)
        if (FleeDistanceMin > 0)
        {
            Color fleeMinColor = new Color(FleeRangeColor.r, FleeRangeColor.g, FleeRangeColor.b, 0.3f);
            Color fleeMaxColor = new Color(FleeRangeColor.r, FleeRangeColor.g, FleeRangeColor.b, 0.7f);
            
            MyUtility.UtilityGizmos.DrawCircle(transform.position, FleeDistanceMin, fleeMinColor);
            MyUtility.UtilityGizmos.DrawCircle(transform.position, FleeDistanceMax, fleeMaxColor);
        }
    }

    // 에디터에서도 항상 기즈모 표시
    protected virtual void OnDrawGizmos()
    {
        // 시야 기즈모 그리기
        DrawVisionGizmos();
    }

    /// <summary>
    /// 게임뷰에서 범위 시각화 (OnGUI)
    /// </summary>
    protected virtual void OnGUI()
    {
        if (!ShowRangesInGame) return;
        if (Camera.main == null) return;

        // Patrol 범위 시각화
        Color patrolMinColor = new Color(PatrolRangeColor.r, PatrolRangeColor.g, PatrolRangeColor.b, 0.2f);
        Color patrolMaxColor = new Color(PatrolRangeColor.r, PatrolRangeColor.g, PatrolRangeColor.b, 0.5f);
        
        DrawWorldCircleGUI(transform.position, PatrolDistanceMin, patrolMinColor, 32);
        DrawWorldCircleGUI(transform.position, PatrolDistanceMax, patrolMaxColor, 32);

        // Flee 범위 시각화 (Flee 기능이 있는 Enemy만)
        if (FleeDistanceMin > 0)
        {
            Color fleeMinColor = new Color(FleeRangeColor.r, FleeRangeColor.g, FleeRangeColor.b, 0.2f);
            Color fleeMaxColor = new Color(FleeRangeColor.r, FleeRangeColor.g, FleeRangeColor.b, 0.5f);
            
            DrawWorldCircleGUI(transform.position, FleeDistanceMin, fleeMinColor, 32);
            DrawWorldCircleGUI(transform.position, FleeDistanceMax, fleeMaxColor, 32);
        }
    }

    /// <summary>
    /// 월드 공간에 원 그리기 (OnGUI용)
    /// </summary>
    protected void DrawWorldCircleGUI(Vector3 center, float radius, Color color, int segments)
    {
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        Vector2 prevScreenPoint = WorldToGUIPoint(prevPoint);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 360f / segments * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Vector2 newScreenPoint = WorldToGUIPoint(newPoint);
            
            // 둘 다 카메라 앞에 있을 때만 그리기
            if (prevScreenPoint != Vector2.zero && newScreenPoint != Vector2.zero)
            {
                DrawGUILine(prevScreenPoint, newScreenPoint, color, 2f);
            }
            
            prevPoint = newPoint;
            prevScreenPoint = newScreenPoint;
        }
    }

    /// <summary>
    /// 월드 좌표를 GUI 좌표로 변환
    /// </summary>
    protected Vector2 WorldToGUIPoint(Vector3 worldPoint)
    {
        if (Camera.main == null) return Vector2.zero;
        
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPoint);
        
        // 카메라 뒤에 있으면 제외
        if (screenPoint.z < 0) return Vector2.zero;
        
        // GUI 좌표계로 변환 (Y축 반전)
        return new Vector2(screenPoint.x, Screen.height - screenPoint.y);
    }

    /// <summary>
    /// GUI에 선 그리기
    /// </summary>
    protected void DrawGUILine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        
        if (distance < 0.01f) return; // 거리가 너무 짧으면 스킵
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        Matrix4x4 matrixBackup = GUI.matrix;
        GUI.color = color;
        
        // 회전 및 이동 변환
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - thickness / 2f, distance, thickness), Texture2D.whiteTexture);
        
        GUI.matrix = matrixBackup;
        GUI.color = Color.white;
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

    /// <summary>
    /// 이 적이 피해를 받았을 때 호출되는 메서드
    /// HP를 감소시키고, HP가 0 이하가 되면 사망 처리를 수행합니다.
    /// </summary>
    /// <param name="damage">받을 피해량</param>
    /// <param name="damageType">피해 타입</param>
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

    /// <summary>
    /// 이 적이 다른 대상에게 피해를 입힐 때 호출되는 메서드
    /// 타겟의 TakeDamage를 호출하여 피해를 전달합니다.
    /// </summary>
    /// <param name="damage">입힐 피해량</param>
    /// <param name="target">피해를 받을 대상 (IDamageable 인터페이스를 구현한 객체)</param>
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
        agent.SetDestination(targetPoint);
        Debug.Log($"순찰 위치 설정: {targetPoint}");
    }
    
    /// <summary>
    /// NavMesh 위에서 안전한 랜덤 순찰 위치를 생성합니다.
    /// </summary>
    /// <returns>NavMesh 위의 유효한 위치를 반환합니다. 적절한 위치를 찾지 못한 경우 현재 위치를 반환합니다.</returns>
    public Vector3 GenerateRandomPatrolPoint()
    {
        // 새로운 Range 설정 사용 (퍼센트 기반)
        float minDistance = PatrolDistanceMin;
        float maxDistance = PatrolDistanceMax;
        
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