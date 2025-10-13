using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 무우(Moo) 몬스터의 AI 컨트롤러입니다.
/// </summary>
public class MooAIController : EnemyAIController<MooAIData>
{
    [Header("Debug")]
    [ReadOnly] [SerializeField] public string CurrentStateName = "";
    public new MooAIData EnemyData => enemyData;
    public bool isShowDebug = false;
    
    [Header("Slime Settings")]
    public float slimeEmitInterval = 10f;
    private float lastSlimeEmitTime;
    
    [Header("Movement")]
    [HideInInspector] public float moveSpeed = 2f;
    
    [Header("Detection System")]
    [Tooltip("플레이어 달리기 소리 감지 범위")]
    public float soundDetectionRange = 5f;
    
    [Header("Debug Visualization")]
    [HideInInspector] public Vector3? currentTargetPosition = null; // 현재 목표 지점
    [HideInInspector] public string currentTargetLabel = ""; // 목표 지점 라벨
    
    [Header("Stamina System")]
    [Tooltip("최대 기력")]
    public float maxStamina = 100f;
    [Tooltip("현재 기력")]
    [ReadOnly] [SerializeField] private float currentStamina = 100f;
    [Tooltip("도망칠 때 초당 기력 소모량")]
    public float staminaDrainRate = 20f;
    [Tooltip("배회할 때 초당 기력 회복량")]
    public float staminaRecoveryRate = 5f;
    [Tooltip("피격 시 즉시 소모되는 기력")]
    public float staminaLossOnHit = 30f;
    [Tooltip("기력이 이 값 이하면 지침 상태")]
    public float exhaustedThreshold = 10f;
    
    public bool IsExhausted => currentStamina <= exhaustedThreshold;
    public float StaminaPercent => currentStamina / maxStamina;
    
    [Header("Animation")]
    public Animator animator;
    [ReadOnly] [SerializeField] private float animatorSpeed = 2f;
    public readonly string idleAnimationName = "Moo_Idle";
    public readonly string walkAnimationName = "Moo_Walk";
    public readonly string hitReactionAnimationName = "Moo_HitReaction";
    public readonly string cryAnimationName = "Moo_Cry";

    [Header("Damage Settings")]
    public int damageAmount = 10;
    private float lastDamageTime = 0f;
    public float damageCooldown = 1f;

    /// <summary>
    /// 초기화 메서드입니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        
        agent = GetComponent<NavMeshAgent>();
        moveSpeed = enemyData.moveSpeed;
        agent.speed = moveSpeed;
        
        // 기력 초기화
        currentStamina = maxStamina;
        
        lastSlimeEmitTime = Time.time;
        
        ChangeState(new MooWanderState(this));
        StartCoroutine(SlimeEmitRoutine());
        StartCoroutine(StaminaUpdateRoutine());
        
        DebugManager.Log("MooAIController Start", isShowDebug);
    }

    /// <summary>
    /// 매 프레임 호출되는 업데이트 메서드입니다.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        UpdateAnimatorSpeed();
        
        // CurrentStateName 업데이트
        if (currentState != null)
        {
            CurrentStateName = currentState.GetType().Name;
        }
    }
    [ContextMenu("AttackCenter")]
    public override void AttackCenter()
    {
        base.AttackCenter();
        ChangeState(new MooCenterAttackState(this));
    }

    

    /// <summary>
    /// 애니메이터의 Speed 파라미터를 업데이트합니다.
    /// </summary>
    private void UpdateAnimatorSpeed()
    {
        if (agent != null && agent.enabled)
        {
            animatorSpeed = agent.velocity.magnitude / moveSpeed;
        animator.SetFloat("Speed", animatorSpeed);
            DebugManager.Log($"agent.velocity.magnitude : {agent.velocity.magnitude}, moveSpeed : {moveSpeed}, animatorSpeed: {animatorSpeed}", isShowDebug);
        }
    }

    /// <summary>
    /// 데미지를 받을 때 호출되는 메서드입니다.
    /// </summary>
    public override void TakeDamage(int damage, DamageType damageType)
    {
        base.TakeDamage(damage, damageType);
        PlayAnimation(hitReactionAnimationName);
        
        // 기력 소모
        ConsumeStamina(staminaLossOnHit);
        
        // 지쳐있지 않으면 도망
        if (!IsExhausted)
        {
        ChangeState(new MooFleeState(this));
        }
        else
        {
            DebugManager.Log("Moo가 너무 지쳐서 도망치지 못합니다!", isShowDebug);
            // 지침 상태 애니메이션 재생 (옵션)
            PlayAnimation(cryAnimationName);
        }
    }

    /// <summary>
    /// 지정된 애니메이션을 재생합니다.
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        animator.CrossFade(animationName, 0.2f);
    }

    public float SlimeDuration => enemyData.slimeDuration;
    public GameObject SlimePrefab => enemyData.slimePrefab;

    /// <summary>
    /// 플레이어가 위협적인지 감지 (시야 + 청각)
    /// </summary>
    /// <param name="detectionType">감지 타입 (Visual, Sound, Both)</param>
    /// <returns>위협 감지 여부</returns>
    public bool DetectPlayerThreat(out string detectionType)
    {
        detectionType = "None";
        
        if (playerTr == null)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTr.position);
        
        // 1. 시야 감지 (Base 클래스 메서드 사용)
        bool visualDetection = CheckPlayerInSight();
        if (visualDetection)
        {
            // 시야 내에서 가까이 다가옴
            if (distanceToPlayer <= vision.sightRange)
            {
                detectionType = "Visual";
                DebugManager.Log($"Moo: 시야로 플레이어 감지! 거리: {distanceToPlayer:F1}m", isShowDebug);
                return true;
            }
        }

        // 2. 청각 감지 (달리기 소리)
        if (distanceToPlayer <= soundDetectionRange)
        {
            PlayerStatus playerStatus = playerTr.GetComponent<PlayerStatus>();
            if (playerStatus != null && playerStatus.IsCurrentRun)
            {
                detectionType = "Sound";
                DebugManager.Log($"Moo: 달리기 소리 감지! 거리: {distanceToPlayer:F1}m", isShowDebug);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 시야 내 플레이어 감지 (기존 Base 메서드 사용)
    /// </summary>
    public bool IsPlayerInVisionRange()
    {
        return CheckPlayerInSight();
    }

    /// <summary>
    /// 청각 범위 내 플레이어 감지 (달리기 소리)
    /// </summary>
    public bool IsPlayerRunningSoundDetected(out float distance)
    {
        distance = 0f;
        
        if (playerTr == null)
            return false;

        distance = Vector3.Distance(transform.position, playerTr.position);
        
        if (distance > soundDetectionRange)
            return false;

        PlayerStatus playerStatus = playerTr.GetComponent<PlayerStatus>();
        if (playerStatus == null)
            return false;

        return playerStatus.IsCurrentRun;
    }

    /// <summary>
    /// 플레이어와 충돌 시 데미지를 줍니다.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // Debug.Break();
        //Debug.Log("MooAIController OnCollisionEnter");
        if (other.gameObject.tag == ETag.Player.ToString())
        {
            // 1초에 한 번만 데미지를 줄 수 있도록 체크
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                //DebugManager.Log("MooAIController OnCollisionEnter", isShowDebug);
                PlayerStatus player = other.gameObject.GetComponent<PlayerStatus>();
                if (player != null)
                {
                    DealDamage(damageAmount, player as IDamageable);
                    lastDamageTime = Time.time; // 마지막 데미지 시간 갱신
                }
            }
        }
    }
   
    /// <summary>
    /// 플레이어에게 데미지를 줍니다.
    /// </summary>
    public override void DealDamage(int damage, IDamageable target)
    {
        Debug.Log($"target {target.ToString()}DealDamage {damage} ");
        target.TakeDamage(damage, DamageType.Nomal);
    }

    /// <summary>
    /// 점액 배출 코루틴
    /// </summary>
    private IEnumerator SlimeEmitRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(slimeEmitInterval);
            ChangeState(new MooSlimeEmitState(this));
        }
    }

    /// <summary>
    /// 기력 소모
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        currentStamina = Mathf.Max(0, currentStamina - amount);
        DebugManager.Log($"Moo 기력 소모: {amount}, 남은 기력: {currentStamina}/{maxStamina} ({StaminaPercent:P0})", isShowDebug);
    }

    /// <summary>
    /// 기력 회복
    /// </summary>
    public void RecoverStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }

    /// <summary>
    /// 기력 업데이트 코루틴
    /// </summary>
    private IEnumerator StaminaUpdateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // 0.1초마다 체크
            
            // 상태에 따라 기력 변화
            if (currentState is MooFleeState)
            {
                // 도망 중: 기력 소모
                ConsumeStamina(staminaDrainRate * 0.1f);
            }
            else if (currentState is MooWanderState)
            {
                // 배회 중: 기력 회복 (일반 속도)
                RecoverStamina(staminaRecoveryRate * 0.1f);
            }
            else if (currentState is MooExhaustedState)
            {
                // 지침 중: 기력 회복 (느린 속도)
                RecoverStamina(staminaRecoveryRate * 0.5f * 0.1f); // 절반 속도로 회복
            }
            // MooSlimeEmitState, MooCenterAttackState는 기력 변화 없음
        }
    }


    /// <summary>
    /// Game 뷰에서 디버그 UI 표시 (OnGUI 사용)
    /// TODO: 나중에 EnemyAIController로 이동하여 모든 Enemy가 사용할 수 있게 함
    /// </summary>
    protected override void OnGUI()
    {
        // Base 클래스의 범위 시각화 (Patrol/Flee 범위)
        base.OnGUI();
        
        if (!isShowDebug) return;
        if (Camera.main == null) return;

        // 1. 감지 범위 시각화 (월드 공간에 그리기)
        DrawDetectionRanges();

        // 2. 목표 지점 시각화
        DrawTargetPoint();

        // 3. HP/Stamina 바 및 상태 텍스트
        DrawDebugBars();
    }

    /// <summary>
    /// 감지 범위 시각화 (시야각 + 청각 범위)
    /// </summary>
    private void DrawDetectionRanges()
    {
        // 청각 감지 범위 (원형 - 청록색) - Base 메서드 사용
        DrawWorldCircleGUI(transform.position, soundDetectionRange, new Color(0, 1, 1, 0.3f), 32);
        
        // 시야 감지 범위 (부채꼴 - 노란색)
        DrawWorldVisionCone(transform.position, transform.forward, vision.sightRange, vision.sightAngle, new Color(1, 1, 0, 0.3f), 32);
    }

    /// <summary>
    /// 목표 지점 시각화 (도망 목적지 등)
    /// </summary>
    private void DrawTargetPoint()
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
        float markerSize = 20f;
        GUI.color = new Color(1, 0.5f, 0, 0.8f); // 주황색
        GUI.DrawTexture(new Rect(targetScreen.x - markerSize / 2f, targetScreen.y - markerSize / 2f, markerSize, markerSize), Texture2D.whiteTexture);
        
        // 안쪽에 작은 원 (중심점)
        float innerSize = 8f;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(targetScreen.x - innerSize / 2f, targetScreen.y - innerSize / 2f, innerSize, innerSize), Texture2D.whiteTexture);

        // 텍스트 라벨 표시
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = (int)(11 * 1.4f);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        float labelWidth = 150f;
        float labelHeight = 40f;
        float labelY = targetScreen.y + markerSize / 2f + 5f;

        // 배경 박스
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(targetScreen.x - labelWidth / 2f, labelY, labelWidth, labelHeight), Texture2D.whiteTexture);

        // 텍스트 (Enemy 이름 + 포인트 타입)
        string labelText = $"Moo\n{currentTargetLabel}";
        DrawTextWithOutline(targetScreen.x - labelWidth / 2f, labelY, labelWidth, labelHeight, labelText, labelStyle);
        
        GUI.color = Color.white;
    }


    /// <summary>
    /// 월드 공간에 시야각 부채꼴 그리기 (OnGUI용)
    /// </summary>
    private void DrawWorldVisionCone(Vector3 center, Vector3 forward, float range, float angle, Color color, int segments)
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
    

    /// <summary>
    /// HP/Stamina 바 및 상태 텍스트 그리기
    /// </summary>
    private void DrawDebugBars()
    {
        // 월드 좌표를 스크린 좌표로 변환
        Vector3 worldPosition = transform.position + Vector3.up * 2.5f;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // 카메라 뒤에 있으면 표시 안 함
        if (screenPosition.z < 0) return;

        // 스크린 좌표는 좌하단이 (0,0)인데 GUI는 좌상단이 (0,0)이므로 Y좌표 반전
        screenPosition.y = Screen.height - screenPosition.y;

        // UI 크기 (1.4배 증가)
        float scaleFactor = 1.4f;
        float baseBarWidth = 100f * scaleFactor;
        float baseBarHeight = 15f * scaleFactor;
        float barSpacing = 5f * scaleFactor;
        
        float startX = screenPosition.x - baseBarWidth / 2f;
        float startY = screenPosition.y - 60f * scaleFactor;

        // HP 바 그리기
        float hpPercent = (float)hp / enemyData.hpMax;
        DrawDebugBar(startX, startY, baseBarWidth, baseBarHeight, 
                     "HP", hp, enemyData.hpMax, hpPercent, GetHPColor(hpPercent));

        // Stamina 바 그리기
        DrawDebugBar(startX, startY + baseBarHeight + barSpacing, baseBarWidth, baseBarHeight,
                     "Stamina", (int)currentStamina, (int)maxStamina, StaminaPercent, GetStaminaColor(StaminaPercent));

        // 상태 텍스트 (두 바 아래에 표시)
        DrawStateText(startX, startY + (baseBarHeight + barSpacing) * 2 + 5f, baseBarWidth);
    }

    /// <summary>
    /// 디버그 바 그리기 헬퍼 메서드
    /// TODO: 나중에 EnemyAIController로 이동
    /// </summary>
    private void DrawDebugBar(float x, float y, float width, float height, 
                             string label, int current, int max, float percent, Color barColor)
    {
        // 라벨 크기 (1.4배 스케일에 맞춰 증가)
        float labelWidth = 70f;
        float barStartX = x + labelWidth;
        float barWidth = width - labelWidth;

        // 라벨 텍스트
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.fontSize = (int)(11 * 1.4f);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        DrawTextWithOutline(x, y, labelWidth, height, label, labelStyle);

        // 배경 (검은색 외곽선)
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(barStartX - 1, y - 1, barWidth + 2, height + 2), Texture2D.whiteTexture);

        // 바 배경 (어두운 회색)
        GUI.color = new Color(0.3f, 0.3f, 0.3f);
        GUI.DrawTexture(new Rect(barStartX, y, barWidth, height), Texture2D.whiteTexture);

        // 현재 값 바
        GUI.color = barColor;
        GUI.DrawTexture(new Rect(barStartX, y, barWidth * percent, height), Texture2D.whiteTexture);

        // 텍스트 표시
        GUIStyle valueStyle = new GUIStyle();
        valueStyle.alignment = TextAnchor.MiddleCenter;
        valueStyle.fontSize = (int)(11 * 1.4f);
        valueStyle.normal.textColor = Color.white;
        valueStyle.fontStyle = FontStyle.Bold;

        string valueText = $"{current}/{max}";
        DrawTextWithOutline(barStartX, y, barWidth, height, valueText, valueStyle);
    }

    /// <summary>
    /// 외곽선이 있는 텍스트 그리기 헬퍼 메서드
    /// TODO: 나중에 EnemyAIController로 이동
    /// </summary>
    private void DrawTextWithOutline(float x, float y, float width, float height, string text, GUIStyle style)
    {
        // 외곽선 (검은색)
        GUI.color = Color.black;
        GUI.Label(new Rect(x - 1, y - 1, width, height), text, style);
        GUI.Label(new Rect(x + 1, y - 1, width, height), text, style);
        GUI.Label(new Rect(x - 1, y + 1, width, height), text, style);
        GUI.Label(new Rect(x + 1, y + 1, width, height), text, style);

        // 텍스트 (흰색)
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, width, height), text, style);
    }

    /// <summary>
    /// 상태 텍스트 그리기
    /// TODO: 나중에 EnemyAIController로 이동
    /// </summary>
    private void DrawStateText(float x, float y, float width)
    {
        GUIStyle stateStyle = new GUIStyle();
        stateStyle.alignment = TextAnchor.MiddleCenter;
        stateStyle.fontSize = (int)(12 * 1.4f);
        stateStyle.normal.textColor = Color.white;
        stateStyle.fontStyle = FontStyle.Bold;

        string stateText = $"State: {CurrentStateName}";
        
        // 특수 상태 표시
        if (IsExhausted)
        {
            stateText += " [EXHAUSTED]";
        }

        DrawTextWithOutline(x, y, width, 20, stateText, stateStyle);

        // 감지 정보 표시
        string detectionType;
        bool detected = DetectPlayerThreat(out detectionType);
        if (detected)
        {
            string detectionText = $"Detection: {detectionType}";
            GUIStyle detectionStyle = new GUIStyle();
            detectionStyle.alignment = TextAnchor.MiddleCenter;
            detectionStyle.fontSize = (int)(11 * 1.4f);
            detectionStyle.normal.textColor = detectionType == "Visual" ? Color.yellow : Color.cyan;
            detectionStyle.fontStyle = FontStyle.Bold;
            
            DrawTextWithOutline(x, y + 20, width, 20, detectionText, detectionStyle);
        }
    }

    /// <summary>
    /// HP 퍼센트에 따른 색상 반환
    /// TODO: 나중에 EnemyAIController로 이동
    /// </summary>
    private Color GetHPColor(float hpPercent)
    {
        if (hpPercent > 0.6f)
            return Color.green;
        else if (hpPercent > 0.3f)
            return Color.yellow;
        else
            return Color.red;
    }

    /// <summary>
    /// 기력 퍼센트에 따른 색상 반환
    /// TODO: 나중에 EnemyAIController로 이동
    /// </summary>
    private Color GetStaminaColor(float staminaRatio)
    {
        if (staminaRatio > 0.5f)
            return Color.Lerp(Color.yellow, Color.green, (staminaRatio - 0.5f) * 2f);
        else
            return Color.Lerp(Color.red, Color.yellow, staminaRatio * 2f);
    }
} 