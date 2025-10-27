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
    
    // Flee 설정 (Moo 전용 - 다른 Enemy는 0 반환)
    public virtual float FleeDistanceMin => 0f;
    public virtual float FleeDistanceMax => 0f;
    
    // 시각화 설정
    public Color PatrolRangeColor => EnemyData.patrolRangeColor;
    public virtual Color FleeRangeColor => Color.red;
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

    [Header("Debug UI Settings")]
    [Tooltip("게임뷰에서 디버그 정보 표시 여부")]
    public bool isShowDebug = false;
    [Tooltip("디버그 UI 크기 배율")]
    public float debugUIScale = 1.4f;
    [HideInInspector] public Vector3? currentTargetPosition = null; // 현재 목표 지점
    [HideInInspector] public string currentTargetLabel = ""; // 목표 지점 라벨

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
    /// 게임뷰에서 범위 시각화 및 디버그 정보 표시 (OnGUI)
    /// </summary>
    protected virtual void OnGUI()
    {
        if (Camera.main == null) return;

        // 1. 범위 시각화
        if (ShowRangesInGame)
        {
            DrawRangeVisualization();
        }
        
        // 2. 디버그 정보 표시
        if (isShowDebug)
        {
            DrawEnemyDebugInfo();
        }
    }

    /// <summary>
    /// 범위 시각화 그리기 (Patrol/Flee 범위)
    /// </summary>
    protected virtual void DrawRangeVisualization()
    {
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
        
        // 시야 범위 시각화 (시야각 부채꼴)
        if (vision.sightRange > 0)
        {
            Color sightColor = new Color(vision.sightColor.r, vision.sightColor.g, vision.sightColor.b, 0.3f);
            DrawWorldVisionCone(transform.position, transform.forward, vision.sightRange, vision.sightAngle, sightColor, 32);
        }
        
        // 근접 감지 범위 시각화 (원형)
        if (vision.enableProximityDetection && vision.proximityDetectionRange > 0)
        {
            Color proximityColor = new Color(vision.proximityColor.r, vision.proximityColor.g, vision.proximityColor.b, 0.3f);
            DrawWorldCircleGUI(transform.position, vision.proximityDetectionRange, proximityColor, 32);
        }
    }

    /// <summary>
    /// Enemy 디버그 정보 표시 (HP바, 상태 등)
    /// 상속받는 클래스에서 override하여 추가 정보 표시 가능
    /// </summary>
    protected virtual void DrawEnemyDebugInfo()
    {
        // 1. HP/Status 바 그리기
        DrawDebugBars();
        
        // 2. 목표 지점 시각화
        DrawTargetPoint();
        
        // 3. 상속받는 클래스별 커스텀 정보 (virtual 메서드)
        DrawCustomDebugInfo();
    }

    /// <summary>
    /// 상속받는 클래스에서 override하여 추가 디버그 정보 표시
    /// (감지 범위, 특수 상태 등)
    /// </summary>
    protected virtual void DrawCustomDebugInfo()
    {
        // 기본적으로는 아무것도 그리지 않음
        // 각 Enemy에서 override하여 구현
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

    /// <summary>
    /// 월드 공간에 시야각 부채꼴 그리기 (OnGUI용)
    /// </summary>
    protected void DrawWorldVisionCone(Vector3 center, Vector3 forward, float range, float angle, Color color, int segments)
    {
        Vector2 centerScreen = WorldToGUIPoint(center);
        if (centerScreen == Vector2.zero) return;

        // 부채꼴의 왼쪽과 오른쪽 경계
        Vector3 leftBoundary = Quaternion.Euler(0, -angle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, angle / 2f, 0) * forward;

        // 중심에서 왼쪽 경계로 선
        Vector2 leftEdgeScreen = WorldToGUIPoint(center + leftBoundary * range);
        if (leftEdgeScreen != Vector2.zero)
        {
            DrawGUILine(centerScreen, leftEdgeScreen, color, 2f);
        }

        // 중심에서 오른쪽 경계로 선
        Vector2 rightEdgeScreen = WorldToGUIPoint(center + rightBoundary * range);
        if (rightEdgeScreen != Vector2.zero)
        {
            DrawGUILine(centerScreen, rightEdgeScreen, color, 2f);
        }

        // 호 그리기
        Vector3 prevPoint = center + leftBoundary * range;
        Vector2 prevScreenPoint = WorldToGUIPoint(prevPoint);
        
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle / 2f + (angle * i / segments);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 newPoint = center + direction * range;
            Vector2 newScreenPoint = WorldToGUIPoint(newPoint);
            
            if (prevScreenPoint != Vector2.zero && newScreenPoint != Vector2.zero)
            {
                DrawGUILine(prevScreenPoint, newScreenPoint, color, 2f);
            }
            
            prevPoint = newPoint;
            prevScreenPoint = newScreenPoint;
        }
    }

    #region Debug UI Methods
    
    /// <summary>
    /// HP 바 및 상태 텍스트 그리기
    /// 상속받는 클래스에서 override하여 커스터마이즈 가능
    /// </summary>
    protected virtual void DrawDebugBars()
    {
        // 월드 좌표를 스크린 좌표로 변환
        Vector3 worldPosition = transform.position + Vector3.up * 2.5f;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // 카메라 뒤에 있으면 표시하지 않음
        if (screenPosition.z <= 0) return;

        // GUI 좌표계로 변환
        float scaleFactor = debugUIScale;
        float baseBarWidth = 120f * scaleFactor;
        float baseBarHeight = 20f * scaleFactor;
        float barSpacing = 5f * scaleFactor;

        float startX = screenPosition.x - baseBarWidth / 2f;
        float startY = Screen.height - screenPosition.y - 40f * scaleFactor;

        // HP 바 그리기
        int hp = Mathf.RoundToInt(EnemyData.hpMax); // 기본적으로 최대 HP로 표시 (각 Enemy에서 override)
        float hpPercent = 1f; // 기본적으로 100%
        DrawDebugBar(startX, startY, baseBarWidth, baseBarHeight, 
                     "HP", hp, hp, hpPercent, GetHPColor(hpPercent));

        // 상태 텍스트 (HP 바 아래에 표시)
        DrawStateText(startX, startY + baseBarHeight + barSpacing, baseBarWidth);
    }

    /// <summary>
    /// 개별 디버그 바 그리기 (HP, Stamina 등)
    /// </summary>
    protected virtual void DrawDebugBar(float x, float y, float width, float height, string label, int current, int max, float percent, Color barColor)
    {
        float labelWidth = width * 0.3f;
        float barWidth = width * 0.7f;
        float barStartX = x + labelWidth + 5f;

        // 라벨 텍스트
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.fontSize = (int)(10 * debugUIScale);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        DrawTextWithOutline(x, y, labelWidth, height, label, labelStyle);

        // 배경 (검은색 외곽선)
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(barStartX - 1, y - 1, barWidth + 2, height + 2), Texture2D.whiteTexture);

        // 배경 (회색)
        GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        GUI.DrawTexture(new Rect(barStartX, y, barWidth, height), Texture2D.whiteTexture);

        // 바 (색상)
        GUI.color = barColor;
        GUI.DrawTexture(new Rect(barStartX, y, barWidth * percent, height), Texture2D.whiteTexture);

        // 값 텍스트
        GUIStyle valueStyle = new GUIStyle();
        valueStyle.alignment = TextAnchor.MiddleCenter;
        valueStyle.fontSize = (int)(9 * debugUIScale);
        valueStyle.normal.textColor = Color.white;
        valueStyle.fontStyle = FontStyle.Normal;

        string valueText = $"{current}/{max}";
        DrawTextWithOutline(barStartX, y, barWidth, height, valueText, valueStyle);
        
        GUI.color = Color.white;
    }

    /// <summary>
    /// 상태 텍스트 표시 (현재 상태, 감지 정보 등)
    /// 상속받는 클래스에서 override하여 커스터마이즈 가능
    /// </summary>
    protected virtual void DrawStateText(float x, float y, float width)
    {
        // 현재 상태 표시
        string stateText = currentState != null ? $"State: {currentState.GetType().Name}" : "State: None";
        
        GUIStyle stateStyle = new GUIStyle();
        stateStyle.alignment = TextAnchor.MiddleCenter;
        stateStyle.fontSize = (int)(10 * debugUIScale);
        stateStyle.normal.textColor = Color.yellow;
        stateStyle.fontStyle = FontStyle.Bold;

        DrawTextWithOutline(x, y, width, 20, stateText, stateStyle);
    }

    /// <summary>
    /// 목표 지점 시각화 (이동 목적지 등)
    /// </summary>
    protected virtual void DrawTargetPoint()
    {
        if (currentTargetPosition == null) return;

        Vector3 targetPos = currentTargetPosition.Value;
        Vector2 targetScreen = WorldToGUIPoint(targetPos);
        
        if (targetScreen == Vector2.zero) return;

        // 현재 위치에서 목표 지점까지 선 그리기
        Vector2 currentScreen = WorldToGUIPoint(transform.position);
        if (currentScreen != Vector2.zero)
        {
            DrawGUILine(currentScreen, targetScreen, new Color(1, 0.5f, 0, 0.6f), 3f); // 주황색 선
        }

        // 목표 지점에 원 마커 그리기
        float markerSize = 20f * debugUIScale;
        GUI.color = new Color(1, 0.5f, 0, 0.8f); // 주황색
        GUI.DrawTexture(new Rect(targetScreen.x - markerSize / 2f, targetScreen.y - markerSize / 2f, markerSize, markerSize), Texture2D.whiteTexture);
        
        // 안쪽에 작은 원 (중심점)
        float innerSize = 8f * debugUIScale;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(targetScreen.x - innerSize / 2f, targetScreen.y - innerSize / 2f, innerSize, innerSize), Texture2D.whiteTexture);

        // 텍스트 라벨 표시
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = (int)(11 * debugUIScale);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        float labelWidth = 150f * debugUIScale;
        float labelHeight = 40f * debugUIScale;
        float labelY = targetScreen.y + markerSize / 2f + 5f;

        // 배경 박스
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(targetScreen.x - labelWidth / 2f, labelY, labelWidth, labelHeight), Texture2D.whiteTexture);

        // 텍스트 (Enemy 이름 + 포인트 타입)
        string enemyName = GetEnemyDisplayName();
        string labelText = $"{enemyName}\n{currentTargetLabel}";
        DrawTextWithOutline(targetScreen.x - labelWidth / 2f, labelY, labelWidth, labelHeight, labelText, labelStyle);
        
        GUI.color = Color.white;
    }

    /// <summary>
    /// Enemy 표시 이름 반환 (상속받는 클래스에서 override)
    /// </summary>
    protected virtual string GetEnemyDisplayName()
    {
        return this.GetType().Name.Replace("AIController", "");
    }

    /// <summary>
    /// 외곽선이 있는 텍스트 그리기 헬퍼 메서드
    /// </summary>
    protected void DrawTextWithOutline(float x, float y, float width, float height, string text, GUIStyle style)
    {
        // 외곽선 (검은색)
        GUI.color = Color.black;
        GUI.Label(new Rect(x - 1, y - 1, width, height), text, style);
        GUI.Label(new Rect(x + 1, y - 1, width, height), text, style);
        GUI.Label(new Rect(x - 1, y + 1, width, height), text, style);
        GUI.Label(new Rect(x + 1, y + 1, width, height), text, style);

        // 본문 텍스트 (흰색)
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, width, height), text, style);
    }

    /// <summary>
    /// HP 퍼센트에 따른 색상 반환
    /// </summary>
    protected Color GetHPColor(float hpPercent)
    {
        if (hpPercent > 0.6f)
            return Color.green;
        else if (hpPercent > 0.3f)
            return Color.yellow;
        else
            return Color.red;
    }

    /// <summary>
    /// Stamina 퍼센트에 따른 색상 반환 (Enemy별로 override 가능)
    /// </summary>
    protected virtual Color GetStaminaColor(float staminaRatio)
    {
        if (staminaRatio > 0.5f)
            return Color.Lerp(Color.yellow, Color.green, (staminaRatio - 0.5f) * 2f);
        else
            return Color.Lerp(Color.red, Color.yellow, staminaRatio * 2f);
    }

    #endregion
    
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
        
        // 근접 감지 범위 (원형) - 활성화된 경우만
        if (vision.enableProximityDetection && vision.proximityDetectionRange > 0)
        {
            Gizmos.color = vision.proximityColor;
            
            // 원형으로 근접 감지 범위 그리기
            int circleSegments = 32;
            float angleStepCircle = 360f / circleSegments;
            Vector3 prevCirclePoint = position + new Vector3(vision.proximityDetectionRange, 0, 0);
            
            for (int i = 1; i <= circleSegments; i++)
            {
                float angle = angleStepCircle * i;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 circlePoint = position + new Vector3(
                    Mathf.Cos(rad) * vision.proximityDetectionRange,
                    0,
                    Mathf.Sin(rad) * vision.proximityDetectionRange
                );
                
                Gizmos.DrawLine(prevCirclePoint, circlePoint);
                prevCirclePoint = circlePoint;
            }
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
            // Debug.Log("플레이어가 없습니다.");
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
    /// 근접 감지: 시야각 무시하고 거리만으로 플레이어 감지
    /// 플레이어가 적의 뒤에 있어도 일정 거리 안에 들어오면 감지
    /// </summary>
    public bool CheckPlayerInProximity()
    {
        if (playerTr == null)
            return false;
        
        if (!vision.enableProximityDetection)
            return false;
        
        // 1. 거리만 체크 (각도 무시)
        Vector3 directionToPlayer = playerTr.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        if (distanceToPlayer > vision.proximityDetectionRange)
            return false;
        
        // 2. 장애물 체크 (레이캐스트)
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
        
        // 거리 조건 + 장애물 없음 = 근접 감지 성공
        playerDetected = true;
        lastKnownPlayerPosition = playerTr.position;
        return true;
    }

    /// <summary>
    /// 시각 감지 또는 근접 감지로 플레이어 탐지
    /// </summary>
    public bool DetectPlayer()
    {
        return CheckPlayerInSight() || CheckPlayerInProximity();
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
    [Header("시각 감지 (각도 체크)")]
    public float sightRange = 10f;       // 시야 거리
    public float sightAngle = 120f;      // 시야각 (전방 기준)
    
    [Header("근접 감지 (거리만 체크)")]
    [Tooltip("활성화 시 시야각 무시하고 거리만으로 플레이어 감지")]
    public bool enableProximityDetection = false;
    [Tooltip("근접 감지 거리 (시야각 무시, 전방향 감지)")]
    public float proximityDetectionRange = 3f;
    
    [Header("레이어 설정")]
    public LayerMask obstacleLayer;      // 장애물 레이어
    public LayerMask playerLayer;        // 플레이어 레이어
    
    [Header("시각화")]
    public bool drawGizmos = true;       // 시야 시각화
    public Color sightColor = new Color(1, 0, 0, 0.3f);
    public Color proximityColor = new Color(1, 0.5f, 0, 0.2f); // 근접 범위 색상
}