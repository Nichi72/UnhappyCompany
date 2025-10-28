using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

/// <summary>
/// 렘페이지(Rampage) AI 컨트롤러.
/// 돌진, 패널 노출, 자폭 등 사양서에서 요구된 로직 관리.
/// </summary>
public class RampageAIController : EnemyAIController<RampageAIData>
{
    // 기본적인 컴포넌트
    private Rigidbody rb;
    public Collider baseCollider;
    
    [Header("Sound Management")]
    private EventInstance idleSoundInstance;      // 공회전 루프 사운드 인스턴스
    private EventInstance moveLoopInstance;       // 이동 루프 사운드 인스턴스
    private bool isIdleSoundPlaying = false;
    private bool isMoveLoopPlaying = false;
     // LineRenderer for visualizing detection
    [SerializeField] private LineRenderer lineRenderer;

    public new RampageAIData EnemyData => enemyData;

    [Header("Charge State")]
    public bool isCollided = false;       // 돌진 중 충돌 발생 여부
    public bool isCushionCollision = false; // 쿠션과 충돌했는지 여부 (패널 개수 결정용)
    public int chargeCount = 0;           // 돌진 횟수
    private int consecutiveCollisions = 0;// 연속 충돌 카운트
    private const int MAX_CONSECUTIVE_COLLISIONS = 10; // 최대 연속 충돌 횟수
    
    [Header("Charge Debug Info")]
    [HideInInspector] public Vector3 chargeStartPosition; // 돌진 시작 지점 (NavMesh → 물리 전환점)
    [HideInInspector] public Vector3 chargeDirection;     // 고정된 돌진 방향
    [HideInInspector] public Vector3 chargeTargetPoint;   // 계산된 돌진 목표 지점
    [HideInInspector] public bool hasChargeTarget = false; // 돌진 목표 설정됨 여부
    [HideInInspector] public float currentChargeSpeed = 0f; // 현재 돌진 속도 (디버그용)
    [HideInInspector] public float targetChargeSpeed = 0f; // 목표 돌진 속도 (디버그용)
    [HideInInspector] public float chargeStuckThreshold = 0f; // 돌진 멈춤 임계값 (디버그용)
    
    [Header("Panel State")]
    [SerializeField] private int currentPanelHealth;
    public int CurrentPanelHealth { get => currentPanelHealth; set => currentPanelHealth = value; }
    // 쿠션 충돌 시 노출될 패널 수 / 쿠션 없이 충돌 시 노출될 패널 수
    private int cushionPanelCount => enemyData.cushionPanelCount;
    private int noCushionPanelCount => enemyData.noCushionPanelCount;
    // 쿠션 없이 충돌 시 HP 감소
    private int hpLossOnNoCushion => enemyData.hpLossOnNoCushion;

    // 사양서에서 요구한 폭발 범위와 대미지
    public float ExplosionDamageRadius => enemyData.explosionDamageRadius;
    public int ExplosionDamage => enemyData.explosionMaxDamage;

   
    public List<RampagePanel> panels;
    
    [Header("Disabled Visual")]
    [Tooltip("패널이 모두 파괴되었을 때 활성화할 부서진 Rampage GameObject")]
    [SerializeField] private GameObject disabledRampageVisual;
    [Tooltip("패널이 모두 파괴되었을 때 비활성화할 정상 Rampage 비주얼 GameObject")]
    [SerializeField] private GameObject normalRampageVisual;
    
    [Header("Reward Drop")]
    [Tooltip("부서질 때 드랍할 선물상자 프리팹")]
    [SerializeField] private GameObject giftBoxPrefab;
    [Tooltip("선물상자 드랍 위치 오프셋 (Rampage 위치 기준)")]
    [SerializeField] private Vector3 giftBoxDropOffset = new Vector3(0f, 1f, 0f);
    
    [Header("Explode Effect")]
    [Tooltip("자폭 시 재생할 폭발 파티클")]
    [SerializeField] private ParticleSystem explodeParticle;
    [Tooltip("폭발 이펙트 위치 오프셋 (Rampage 중심 기준)")]
    [SerializeField] private Vector3 explodeEffectOffset = new Vector3(0f, 1f, 0f);
    
    [Header("Debug Visualization")]
    [Tooltip("씬 뷰에서 폭발 범위 시각화 표시")]
    [SerializeField] private bool showExplosionGizmos = true;
    
    // 폭발 파티클 접근자
    public ParticleSystem ExplodeParticle => explodeParticle;
    public Vector3 ExplodeEffectOffset => explodeEffectOffset;

    private float stuckTime = 0f;
    private Vector3 lastPosition;
    private float stuckThreshold = 0.1f; // 낑김으로 판단할 최소 이동 거리
    private float maxStuckTime = 3f; // 최대 낑김 허용 시간
    public bool onceReduceHP = true; // 충돌시 false로 바꾸고 ChargeCoroutine에서 충돌전 회전할때 true로 만듬.

    protected override void Start()
    {
        base.Start();
        
        // 디버그 변수 초기화
        hasChargeTarget = false;
        chargeStartPosition = Vector3.zero;
        chargeDirection = Vector3.zero;
        chargeTargetPoint = Vector3.zero;
        
        // 리지드바디 초기화
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        // 초기에는 리지드바디 고정
        FreezeRigidbody(true);
        
        // 초기 HP 세팅
        hp = enemyData.maxHP;
        CurrentPanelHealth = enemyData.maxPanelHealth;
        chargeCount = enemyData.maxChargeCount;
        // 초기 상태는 Idle이라고 가정
        ChangeState(new RampageIdleState(this,"Start에서 실행"));
        ResetPanelHealth(enemyData.cushionPanelCount);
        InitPannel();

        // LineRenderer 초기화
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        
        // 부서진 비주얼은 처음에 비활성화
        if (disabledRampageVisual != null)
        {
            disabledRampageVisual.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // 사운드 3D 위치 갱신
        UpdateSoundPositions();
        
        // 현재 상태에 따라 리지드바디 Freeze 상태 설정
        if (currentState is RampageChargeState)
        {
            FreezeRigidbody(false);
        }
        else
        {
            FreezeRigidbody(true);
        }
        
        if (CurrentPanelHealth <= 0 && !(currentState is RampageDisabledState))
        {
            ChangeState(new RampageDisabledState(this));
        }
        if (IsStuck())
        {
            EscapeFromStuck();  // 기존의 ChangeState 대신 EscapeFromStuck 호출
            Debug.Log("Rampage가 낑겼습니다. 경로를 재탐색합니다.");
        }
    }

    void LateUpdate()
    {
        if(isCollided)
        {
            consecutiveCollisions++;
            if(consecutiveCollisions >= MAX_CONSECUTIVE_COLLISIONS)
            {
                // HandleStuck();
                consecutiveCollisions = 0;
            }
            Debug.Log("isCollided false");
            isCollided = false;
        }
        else
        {
            consecutiveCollisions = 0;
        }
    }

    [ContextMenu("ChangeCenterAttackState")]
    public override void AttackCenter()
    {
        base.AttackCenter();
        ChangeState(new RampageCenterAttackState(this));
    }

    /// <summary>
    /// LineRenderer를 비활성화
    /// </summary>
    public void DisableLineRenderer()
    {
        lineRenderer.positionCount = 0;
    }

    public override void TakeDamage(int damage, DamageType damageType)
    {
        // Rampage는 기본적으로 무적입니다. 돌진에 의해 벽에 박았을 때만 데미지를 입습니다.
        // 따라서 플레이어에 의해 데미지는 입지 않습니다. 그래서 데미지 계산을 하지않습니다.
        AudioManager.instance.PlayOneShot(FMODEvents.instance.rampageHitBlock, transform, "Rampage_Hit 하지만 데미지 안받아서 둔탁한 소리가 나야함.");
    }

    /// <summary>
    /// HP 직접 감소 (패널 노출 없이 벽 충돌 시 사용)
    /// </summary>
    public void ReduceHP(int amount)
    {
        hp -= amount;
        Debug.Log("Rampage HP 감소 " + hp);
        if (hp <= 0)
        {
            ChangeState(new RampageExplodeState(this));
        }
    }
    /// <summary>
    /// 패널 Health 초기화
    /// </summary>
    public void ResetPanelHealth(int panelCount)
    {
        CurrentPanelHealth = enemyData.maxPanelHealth;
        // 필요하다면 패널 UI 표시, 애니메이션 트리거 등
    }

    /// <summary>
    /// 충돌 시 쿠션 여부 판정 로직(예시).
    /// 실제로는 쿠션 객체/타일 등을 검사해야 함(Physics나 Raycast, Tag, Layer 등).
    /// </summary>
    public bool IsCushionAtCollision(Collision collision)
    {
        // TODO: 실제 쿠션 판정 로직 구현
        // 현재는 임시로 collision.collider 태그가 "Cushion"이면 쿠션 있다고 가정
        return collision.collider.CompareTag("Cushion"); 
    }
    
    private void InitPannel()
    {
        foreach (var panel in panels)
        {
            panel.gameObject.SetActive(false);
        }
    }
    [ContextMenu("ChangeCenterAttackState")]
    public void ChangeCenterAttackState()
    {
        ChangeState(new RampageCenterAttackState(this));
    }

    #region == Debug ==
    /// <summary>
    /// LineRenderer를 사용하여 감지 범위 시각화
    /// </summary>
    // public void UpdateLineRenderer()
    // {
    //     Vector3 forward = transform.forward;
    //     Vector3 leftBoundary = Quaternion.Euler(0, -enemyData.detectAngle / 2, 0) * forward;
    //     Vector3 rightBoundary = Quaternion.Euler(0, enemyData.detectAngle / 2, 0) * forward;

    //     Vector3[] positions = new Vector3[]
    //     {
    //         transform.position,
    //         transform.position + leftBoundary * enemyData.detectRange,
    //         transform.position + rightBoundary * enemyData.detectRange,
    //         transform.position
    //     };

    //     lineRenderer.positionCount = positions.Length;
    //     lineRenderer.SetPositions(positions);

    //     // 플레이어 감지 여부에 따라 색상 변경
    //     if (CheckPlayerDetected())
    //     {
    //         lineRenderer.startColor = Color.red;
    //         lineRenderer.endColor = Color.red;
    //     }
    //     else
    //     {
    //         lineRenderer.startColor = Color.green;
    //         lineRenderer.endColor = Color.green;
    //     }
    // }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        // UpdateLineRenderer();

        if (vision == null) return;

        // 시야 범위 시각화 (와이어 구)
        Gizmos.color = CheckPlayerInSight() ? Color.red : new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, vision.sightRange);

        // 돌진 감지 범위 (주황색 와이어 구)
        if (enemyData != null)
        {
            float chargeDetectionRange = vision.sightRange * 0.7f;
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawWireSphere(transform.position, chargeDetectionRange);
        }

        // 플레이어를 향한 선 그리기
        if (playerTr != null)
        {
            Gizmos.color = CheckPlayerInSight() ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, playerTr.position + Vector3.up);
        }

        // 시야각 방향 표시
        if (vision != null)
        {
            Vector3 forward = transform.forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -vision.sightAngle / 2, 0) * forward * vision.sightRange;
            Vector3 rightBoundary = Quaternion.Euler(0, vision.sightAngle / 2, 0) * forward * vision.sightRange;

            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
    #endregion

    private bool IsStuck()
    {
        if (currentState is RampageStunnedState)
        {
            return false;
        }
        if (currentState is RampagePanelOpenState)
        {
            return false;
        }
        if(currentState is RampageChargeState)
        {
            return false;
        }

        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        if (distanceMoved < stuckThreshold)
        {
            stuckTime += Time.deltaTime;
            if (stuckTime >= maxStuckTime)
            {
                return true;
            }
        }
        else
        {
            stuckTime = 0f;
        }
        lastPosition = transform.position;
        return false;
    }

    private void EscapeFromStuck()
    {
        // 네비메시 에이전트 일시 정지
        agent.isStopped = true;
        
        // SetRandomPatrolDestination을 사용하여 새로운 목적지 설정
        SetRandomPatrolDestination();
        
        // 에이전트 재시작
        agent.isStopped = false;
        
        // 속도 일시적으로 증가
        StartCoroutine(TemporarySpeedBoost());
        
        // 낑김 관련 변수 초기화
        stuckTime = 0f;
        lastPosition = transform.position;
    }

    private IEnumerator TemporarySpeedBoost()
    {
        float originalSpeed = agent.speed;
        agent.speed *= 1.5f;
        
        yield return new WaitForSeconds(1.5f);
        
        agent.speed = originalSpeed;
    }

    public bool IsInChargeState()
    {
        return currentState is RampageChargeState;
    }

    public void SetCollided(bool value)
    {
        isCollided = value;
    }

    /// <summary>
    /// 부서진 Rampage 비주얼로 교체
    /// </summary>
    public void SwitchToDisabledVisual()
    {
        // 정상 비주얼 비활성화
        if (normalRampageVisual != null)
        {
            normalRampageVisual.SetActive(false);
            Debug.Log("정상 Rampage 비주얼 비활성화");
        }
        else
        {
            Debug.LogWarning("normalRampageVisual이 할당되지 않았습니다. 인스펙터에서 할당해주세요.");
        }

        // 부서진 비주얼 활성화
        if (disabledRampageVisual != null)
        {
            disabledRampageVisual.SetActive(true);
            Debug.Log("부서진 Rampage 비주얼 활성화");
        }
        else
        {
            Debug.LogWarning("disabledRampageVisual이 할당되지 않았습니다. 인스펙터에서 할당해주세요.");
        }
    }

    /// <summary>
    /// 선물상자 드랍
    /// </summary>
    public void DropGiftBox()
    {
        if (giftBoxPrefab == null)
        {
            Debug.LogWarning("[RampageAIController] 선물상자 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 드랍 위치 계산
        Vector3 dropPosition = transform.position + giftBoxDropOffset;

        // 선물상자 스폰
        GameObject giftBox = Instantiate(giftBoxPrefab, dropPosition, Quaternion.identity);

        // 약간의 위로 튀는 힘 추가
        Rigidbody rb = giftBox.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
        }

        Debug.Log($"[RampageAIController] 선물상자 드랍: {dropPosition}");
    }

    #region Gameplay Feedback Methods

    /// <summary>
    /// 추적 → 돌진 전환 시 플레이어에게 경고 피드백 제공
    /// 이 메서드에서 원하는 피드백을 추가하세요:
    /// - VFX 파티클 생성
    /// - 색상 변경
    /// - 카메라 쉐이크
    /// - 애니메이션 트리거
    /// </summary>
    public void TriggerChargeWarningFeedback()
    {
        // TODO: 여기에 추가 피드백 구현
        // 예시:
        // - Instantiate(파티클프리팹, transform.position, Quaternion.identity);
        // - GetComponent<Renderer>().material.color = 경고색;
        // - animator.SetTrigger("ChargeWarning");
        // - CameraShake.Instance?.Shake(강도, 지속시간);
    }

    /// <summary>
    /// 돌진 종료 시 피드백 리셋 (정리 작업)
    /// TriggerChargeWarningFeedback()에서 추가한 효과들을 여기서 정리하세요
    /// </summary>
    public void ResetChargeWarningFeedback()
    {
        // TODO: 여기에 피드백 리셋 구현
        // 예시:
        // - GetComponent<Renderer>().material.color = 원래색;
        // - 코루틴 정지
        // - 파티클 정지
    }

    #endregion

    /// <summary>
    /// 리지드바디의 Freeze 상태를 설정
    /// </summary>
    public void FreezeRigidbody(bool freeze)
    {
        if (rb != null)
        {
            if (freeze)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                // Y축은 항상 고정, 나머지 축은 해제
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    #region Debug UI Override

    /// <summary>
    /// Debug UI 그리기 (Base 클래스 사용)
    /// </summary>
    protected override void OnGUI()
    {
        // Base 클래스에서 모든 디버그 정보 처리
        base.OnGUI();
    }

    /// <summary>
    /// HP/패널 체력/돌진 횟수 바 그리기 (Rampage용 오버라이드)
    /// </summary>
    protected override void DrawDebugBars()
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
        float hpPercent = (float)hp / enemyData.maxHP;
        DrawDebugBar(startX, startY, baseBarWidth, baseBarHeight, 
                     "HP", hp, enemyData.maxHP, hpPercent, GetHPColor(hpPercent));

        // 패널 체력 바 그리기
        float panelPercent = (float)CurrentPanelHealth / enemyData.maxPanelHealth;
        Color panelColor = GetPanelHealthColor(panelPercent);
        DrawDebugBar(startX, startY + baseBarHeight + barSpacing, baseBarWidth, baseBarHeight,
                     "Panel", CurrentPanelHealth, enemyData.maxPanelHealth, panelPercent, panelColor);

        // 돌진 횟수 바 그리기
        float chargePercent = (float)chargeCount / enemyData.maxChargeCount;
        Color chargeColor = Color.Lerp(Color.red, Color.cyan, chargePercent);
        DrawDebugBar(startX, startY + (baseBarHeight + barSpacing) * 2, baseBarWidth, baseBarHeight,
                     "Charge", chargeCount, enemyData.maxChargeCount, chargePercent, chargeColor);

        // 상태 텍스트 (세 바 아래에 표시)
        DrawStateText(startX, startY + (baseBarHeight + barSpacing) * 3 + 5f, baseBarWidth);
    }

    /// <summary>
    /// 패널 체력 퍼센트에 따른 색상 반환
    /// </summary>
    private Color GetPanelHealthColor(float panelPercent)
    {
        if (panelPercent > 0.6f)
            return new Color(0.5f, 0.5f, 1f); // 파란색
        else if (panelPercent > 0.3f)
            return new Color(1f, 0.5f, 0f); // 주황색
        else
            return Color.red;
    }

    /// <summary>
    /// 상태 텍스트 표시 (Rampage용 오버라이드 - 충돌 정보 포함)
    /// </summary>
    protected override void DrawStateText(float x, float y, float width)
    {
        // 현재 상태 표시
        string stateText = currentState != null ? $"State: {currentState.GetType().Name}" : "State: None";
        if (isCollided)
        {
            stateText += " [COLLIDED]";
        }
        
        GUIStyle stateStyle = new GUIStyle();
        stateStyle.alignment = TextAnchor.MiddleCenter;
        stateStyle.fontSize = (int)(20 * debugUIScale);  // 2배 증가 (10 → 20)
        stateStyle.normal.textColor = isCollided ? Color.red : Color.yellow;
        stateStyle.fontStyle = FontStyle.Bold;

        DrawTextWithOutline(x, y, width, 40, stateText, stateStyle);  // 2배 증가 (20 → 40)
        
        // 속도 정보 표시 (ChargeState일 때만)
        if (currentState is RampageChargeState && currentChargeSpeed > 0)
        {
            float speedPercent = targetChargeSpeed > 0 ? (currentChargeSpeed / targetChargeSpeed) : 0f;
            float stopThreshold = enemyData.chargeStopSpeedThreshold;
            string speedText = $"Speed: {currentChargeSpeed:F2} / {targetChargeSpeed:F2} m/s ({speedPercent:P0})";
            
            // 속도에 따라 색상 변경
            Color speedColor = Color.white;
            if (currentChargeSpeed <= stopThreshold)
            {
                speedColor = Color.red; // 임계값 이하 - 멈춤!
            }
            else if (speedPercent > 0.8f)
            {
                speedColor = Color.green; // 정상 속도
            }
            else if (speedPercent > 0.5f)
            {
                speedColor = Color.yellow; // 감속 중
            }
            else
            {
                speedColor = Color.red; // 느림
            }
            
            GUIStyle speedStyle = new GUIStyle();
            speedStyle.alignment = TextAnchor.MiddleCenter;
            speedStyle.fontSize = (int)(18 * debugUIScale);  // 2배 증가 (9 → 18)
            speedStyle.normal.textColor = speedColor;
            speedStyle.fontStyle = FontStyle.Normal;
            
            DrawTextWithOutline(x, y + 44, width, 36, speedText, speedStyle);  // 2배 증가 (22 → 44, 18 → 36)
            
            // 임계값 표시
            string thresholdText = $"Stop Threshold: {stopThreshold:F2} m/s";
            GUIStyle thresholdStyle = new GUIStyle();
            thresholdStyle.alignment = TextAnchor.MiddleCenter;
            thresholdStyle.fontSize = (int)(14 * debugUIScale);
            thresholdStyle.normal.textColor = Color.cyan;
            thresholdStyle.fontStyle = FontStyle.Italic;
            
            DrawTextWithOutline(x, y + 84, width, 28, thresholdText, thresholdStyle);
        }
    }

    /// <summary>
    /// Enemy 표시 이름 반환 (Rampage 오버라이드)
    /// </summary>
    protected override string GetEnemyDisplayName()
    {
        return "Rampage";
    }

    /// <summary>
    /// Rampage만의 특수 디버그 정보 표시 (이동 방향 시각화)
    /// </summary>
    protected override void DrawCustomDebugInfo()
    {
        if (Camera.main == null) return;

        // 1. 감지 범위 시각화
        DrawDetectionRange();

        // 2. 이동 방향 시각화 (현재 속도 방향)
        DrawMovementDirection();

        // 3. 돌진 타겟 시각화 (플레이어 방향)
        if (currentState is RampageChargeState)
        {
            DrawChargeTarget();
        }

        // 4. 플레이어와의 거리 표시
        DrawPlayerDistance();
    }

    /// <summary>
    /// 이동 방향 시각화 (속도 벡터)
    /// </summary>
    private void DrawMovementDirection()
    {
        if (agent == null || agent.velocity.magnitude < 0.1f) return;

        Vector3 currentPos = transform.position + Vector3.up * 0.5f;
        Vector3 targetPos = currentPos + agent.velocity.normalized * 3f;

        Vector2 currentScreen = WorldToGUIPoint(currentPos);
        Vector2 targetScreen = WorldToGUIPoint(targetPos);

        if (currentScreen == Vector2.zero || targetScreen == Vector2.zero) return;

        // 속도에 따라 색상 변경 (느림: 초록, 빠름: 빨강)
        float speedRatio = Mathf.Clamp01(agent.velocity.magnitude / agent.speed);
        Color directionColor = Color.Lerp(Color.green, Color.red, speedRatio);

        // 이동 방향 선 그리기
        DrawGUILine(currentScreen, targetScreen, directionColor, 4f);

        // 화살표 끝 마커
        float markerSize = 15f * debugUIScale;
        GUI.color = directionColor;
        GUI.DrawTexture(new Rect(targetScreen.x - markerSize / 2f, targetScreen.y - markerSize / 2f, markerSize, markerSize), Texture2D.whiteTexture);

        // 속도 텍스트 표시
        GUIStyle speedStyle = new GUIStyle();
        speedStyle.alignment = TextAnchor.MiddleCenter;
        speedStyle.fontSize = (int)(9 * debugUIScale);
        speedStyle.normal.textColor = Color.white;
        speedStyle.fontStyle = FontStyle.Bold;

        string speedText = $"{agent.velocity.magnitude:F1} m/s";
        float labelWidth = 80f * debugUIScale;
        float labelHeight = 20f * debugUIScale;

        // 배경
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(targetScreen.x - labelWidth / 2f, targetScreen.y + markerSize / 2f + 5f, labelWidth, labelHeight), Texture2D.whiteTexture);

        // 텍스트
        DrawTextWithOutline(targetScreen.x - labelWidth / 2f, targetScreen.y + markerSize / 2f + 5f, labelWidth, labelHeight, speedText, speedStyle);

        GUI.color = Color.white;
    }

    /// <summary>
    /// 돌진 타겟 시각화 (고정된 돌진 목표)
    /// </summary>
    private void DrawChargeTarget()
    {
        if (!hasChargeTarget) return;

        // 1. 돌진 시작점 표시 (초록색 원)
        Vector2 startScreen = WorldToGUIPoint(chargeStartPosition);
        if (startScreen != Vector2.zero)
        {
            float startMarkerSize = 20f * debugUIScale;
            GUI.color = new Color(0f, 1f, 0f, 0.8f); // 초록색
            GUI.DrawTexture(new Rect(startScreen.x - startMarkerSize / 2f, startScreen.y - startMarkerSize / 2f, startMarkerSize, startMarkerSize), Texture2D.whiteTexture);
            
            // 시작점 텍스트
            GUIStyle startStyle = new GUIStyle();
            startStyle.alignment = TextAnchor.MiddleCenter;
            startStyle.fontSize = (int)(9 * debugUIScale);
            startStyle.normal.textColor = Color.white;
            startStyle.fontStyle = FontStyle.Bold;
            
            DrawTextWithOutline(startScreen.x - 30f, startScreen.y - startMarkerSize / 2f - 20f, 60f, 15f, "START", startStyle);
        }

        // 2. 고정된 돌진 목표 지점 표시 (빨간색 십자가)
        Vector2 targetScreen = WorldToGUIPoint(chargeTargetPoint);
        if (targetScreen == Vector2.zero) return;

        // 현재 위치에서 고정 목표까지 선 그리기
        Vector2 currentScreen = WorldToGUIPoint(transform.position);
        if (currentScreen != Vector2.zero)
        {
            DrawGUILine(currentScreen, targetScreen, new Color(1f, 0f, 0f, 0.8f), 4f); // 빨간색 선
        }
        
        // 돌진 시작점에서 목표까지 선 그리기 (점선 효과)
        if (startScreen != Vector2.zero)
        {
            DrawGUILine(startScreen, targetScreen, new Color(1f, 1f, 0f, 0.6f), 2f); // 노란색 점선
        }

        // 타겟 마커 (십자가)
        float markerSize = 25f * debugUIScale;
        GUI.color = new Color(1f, 0f, 0f, 0.9f);

        // 십자가 가로선
        GUI.DrawTexture(new Rect(targetScreen.x - markerSize / 2f, targetScreen.y - 2f, markerSize, 4f), Texture2D.whiteTexture);
        // 십자가 세로선
        GUI.DrawTexture(new Rect(targetScreen.x - 2f, targetScreen.y - markerSize / 2f, 4f, markerSize), Texture2D.whiteTexture);

        // 중심점
        float centerSize = 8f * debugUIScale;
        GUI.color = Color.yellow;
        GUI.DrawTexture(new Rect(targetScreen.x - centerSize / 2f, targetScreen.y - centerSize / 2f, centerSize, centerSize), Texture2D.whiteTexture);

        // 텍스트 라벨
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = (int)(10 * debugUIScale);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        float labelWidth = 140f * debugUIScale;
        float labelHeight = 50f * debugUIScale;
        float labelY = targetScreen.y + markerSize / 2f + 5f;

        // 배경 박스
        GUI.color = new Color(0.5f, 0f, 0f, 0.8f);
        GUI.DrawTexture(new Rect(targetScreen.x - labelWidth / 2f, labelY, labelWidth, labelHeight), Texture2D.whiteTexture);

        // 거리 계산 (시작점에서 목표까지)
        float totalDistance = Vector3.Distance(chargeStartPosition, chargeTargetPoint);
        float remainingDistance = Vector3.Distance(transform.position, chargeTargetPoint);
        string labelText = $"Rampage\nFixed Target\nTotal: {totalDistance:F1}m\nRemain: {remainingDistance:F1}m";
        DrawTextWithOutline(targetScreen.x - labelWidth / 2f, labelY, labelWidth, labelHeight, labelText, labelStyle);

        GUI.color = Color.white;
    }

    /// <summary>
    /// 감지 범위 시각화 (시야각)
    /// </summary>
    private void DrawDetectionRange()
    {
        if (vision == null) return;

        // 시야 감지 범위 (부채꼴 - 녹색/빨간색)
        Color rangeColor = CheckPlayerInSight() ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.3f);
        DrawWorldVisionCone(transform.position, transform.forward, vision.sightRange, vision.sightAngle, rangeColor, 32);

        // 돌진 감지 범위 (원형 - 주황색)
        if (enemyData != null)
        {
            // 추적에서 돌진으로 전환되는 거리 범위 (예시로 시야의 70% 거리)
            float chargeDetectionRange = vision.sightRange * 0.7f;
            DrawWorldCircleGUI(transform.position, chargeDetectionRange, new Color(1, 0.5f, 0, 0.2f), 24);
        }

        // 폭발 범위 시각화 (빨간색 - 위험 범위)
        if (enemyData != null && enemyData.explosionDamageRadius > 0)
        {
            DrawWorldCircleGUI(transform.position, enemyData.explosionDamageRadius, new Color(1, 0, 0, 0.15f), 32);
        }
    }

    /// <summary>
    /// 플레이어와의 거리 표시
    /// </summary>
    private void DrawPlayerDistance()
    {
        if (playerTr == null) return;

        float distance = Vector3.Distance(transform.position, playerTr.position);
        Vector3 midPoint = (transform.position + playerTr.position) / 2f + Vector3.up * 1.5f;
        Vector2 midScreen = WorldToGUIPoint(midPoint);

        if (midScreen == Vector2.zero) return;

        // 거리 텍스트 스타일
        GUIStyle distanceStyle = new GUIStyle();
        distanceStyle.alignment = TextAnchor.MiddleCenter;
        distanceStyle.fontSize = (int)(11 * debugUIScale);
        distanceStyle.normal.textColor = Color.white;
        distanceStyle.fontStyle = FontStyle.Bold;

        // 거리에 따른 배경 색상 (가까우면 빨강, 멀면 초록)
        float dangerRatio = vision != null ? Mathf.Clamp01(1f - (distance / vision.sightRange)) : 0;
        Color bgColor = Color.Lerp(new Color(0, 0.5f, 0, 0.7f), new Color(0.8f, 0, 0, 0.8f), dangerRatio);

        float labelWidth = 120f * debugUIScale;
        float labelHeight = 30f * debugUIScale;

        // 배경
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(midScreen.x - labelWidth / 2f, midScreen.y - labelHeight / 2f, labelWidth, labelHeight), Texture2D.whiteTexture);

        // 거리 상태 텍스트
        string statusText = "";
        if (currentState is RampageChargeState)
        {
            statusText = " [CHARGING!]";
        }
        else if (CheckPlayerInSight())
        {
            statusText = " [DETECTED]";
        }

        // 텍스트
        string distanceText = $"Player\n{distance:F1}m{statusText}";
        DrawTextWithOutline(midScreen.x - labelWidth / 2f, midScreen.y - labelHeight / 2f, labelWidth, labelHeight, distanceText, distanceStyle);

        GUI.color = Color.white;
    }

    #endregion
    
    #region Sound Management Methods
    
    /// <summary>
    /// Idle 사운드 재생 (루프)
    /// </summary>
    public void PlayIdleSound()
    {
        if (isIdleSoundPlaying) return;
        
        if (!FMODEvents.instance.rampageIdle.IsNull)
        {
            try
            {
                idleSoundInstance = RuntimeManager.CreateInstance(FMODEvents.instance.rampageIdle);
                idleSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
                idleSoundInstance.start();
                isIdleSoundPlaying = true;
                Debug.Log("[Rampage Sound] Idle 사운드 시작");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Rampage Sound] Idle 사운드를 생성할 수 없습니다.\nPath: {FMODEvents.instance.rampageIdle.Path}\nError: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Idle 사운드 정지
    /// </summary>
    public void StopIdleSound()
    {
        if (!isIdleSoundPlaying) return;
        
        try
        {
            if (idleSoundInstance.isValid())
            {
                idleSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                idleSoundInstance.release();
            }
            isIdleSoundPlaying = false;
            Debug.Log("[Rampage Sound] Idle 사운드 정지");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Rampage Sound] Idle 사운드를 정지할 수 없습니다. Error: {e.Message}");
            isIdleSoundPlaying = false;
        }
    }
    
    /// <summary>
    /// 이동 루프 사운드 재생
    /// </summary>
    public void PlayMoveLoopSound()
    {
        if (isMoveLoopPlaying) return;
        
        if (!FMODEvents.instance.rampageMoveLoop.IsNull)
        {
            try
            {
                moveLoopInstance = RuntimeManager.CreateInstance(FMODEvents.instance.rampageMoveLoop);
                moveLoopInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
                moveLoopInstance.start();
                isMoveLoopPlaying = true;
                Debug.Log("[Rampage Sound] MoveLoop 사운드 시작");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Rampage Sound] MoveLoop 사운드를 생성할 수 없습니다.\nPath: {FMODEvents.instance.rampageMoveLoop.Path}\nError: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 이동 루프 사운드 정지
    /// </summary>
    public void StopMoveLoopSound()
    {
        if (!isMoveLoopPlaying) return;
        
        try
        {
            if (moveLoopInstance.isValid())
            {
                moveLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                moveLoopInstance.release();
            }
            isMoveLoopPlaying = false;
            Debug.Log("[Rampage Sound] MoveLoop 사운드 정지");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Rampage Sound] MoveLoop 사운드를 정지할 수 없습니다. Error: {e.Message}");
            isMoveLoopPlaying = false;
        }
    }
    
    /// <summary>
    /// 출발 사운드 재생 (One-Shot)
    /// </summary>
    public void PlayStartSound()
    {
        if (!FMODEvents.instance.rampageStart.IsNull)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rampageStart, transform, "Rampage 출발 (RPM 상승)");
            Debug.Log("[Rampage Sound] Start 사운드 재생");
        }
    }
    
    /// <summary>
    /// 드리프트 사운드 재생 (One-Shot)
    /// </summary>
    public void PlayDriftSound()
    {
        if (!FMODEvents.instance.rampageDrift.IsNull)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rampageDrift, transform, "Rampage 드리프트");
            Debug.Log("[Rampage Sound] Drift 사운드 재생");
        }
    }
    
    /// <summary>
    /// 모든 루프 사운드 정지
    /// </summary>
    public void StopAllLoopSounds()
    {
        StopIdleSound();
        StopMoveLoopSound();
    }
    
    /// <summary>
    /// Update에서 3D 위치 갱신
    /// </summary>
    private void UpdateSoundPositions()
    {
        if (isIdleSoundPlaying && idleSoundInstance.isValid())
        {
            try
            {
                idleSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Rampage Sound] Idle 사운드 위치 갱신 실패. Error: {e.Message}");
                isIdleSoundPlaying = false;
            }
        }
        
        if (isMoveLoopPlaying && moveLoopInstance.isValid())
        {
            try
            {
                moveLoopInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Rampage Sound] MoveLoop 사운드 위치 갱신 실패. Error: {e.Message}");
                isMoveLoopPlaying = false;
            }
        }
    }
    
    /// <summary>
    /// 오브젝트 파괴 시 사운드 정리
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopAllLoopSounds();
    }
    
    #endregion

    #region Debug Methods - State Testing

    /// <summary>
    /// 테스트용: 즉시 폭발 상태로 전이
    /// </summary>
    [ContextMenu("Test - Explode Now")]
    private void TestExplodeNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[RampageAIController] 플레이 모드에서만 실행 가능합니다!");
            return;
        }
        
        Debug.Log("[RampageAIController] 테스트: 폭발 상태로 강제 전이!");
        ChangeState(new RampageExplodeState(this));
    }
    
    /// <summary>
    /// 테스트용: 무력화 상태로 전이
    /// </summary>
    [ContextMenu("Test - Disabled Now")]
    private void TestDisabledNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[RampageAIController] 플레이 모드에서만 실행 가능합니다!");
            return;
        }
        
        Debug.Log("[RampageAIController] 테스트: 무력화 상태로 강제 전이!");
        ChangeState(new RampageDisabledState(this));
    }
    
    /// <summary>
    /// 테스트용: 돌진 상태로 전이
    /// </summary>
    [ContextMenu("Test - Charge Now")]
    private void TestChargeNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[RampageAIController] 플레이 모드에서만 실행 가능합니다!");
            return;
        }
        
        Debug.Log("[RampageAIController] 테스트: 돌진 상태로 강제 전이!");
        ChangeState(new RampageChargeState(this, "TestButton"));
    }

    #endregion

    #region Gizmos - Explosion Range Visualization

    /// <summary>
    /// 폭발 범위 시각화 (씬 뷰)
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        if (enemyData == null || !showExplosionGizmos)
            return;

        Vector3 explosionCenter = transform.position;

        // 데미지 범위 시각화 (빨간색)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 투명한 빨강
        Gizmos.DrawSphere(explosionCenter, enemyData.explosionDamageRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(explosionCenter, enemyData.explosionDamageRadius);

        // 물리 효과 범위 시각화 (노란색)
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f); // 투명한 노랑
        Gizmos.DrawSphere(explosionCenter, enemyData.explosionForceRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(explosionCenter, enemyData.explosionForceRadius);

        // 중심점 표시 (주황색)
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawSphere(explosionCenter, 0.3f);

        // 범위 표시를 위한 수직선
        Gizmos.color = Color.red;
        Gizmos.DrawLine(explosionCenter, explosionCenter + Vector3.up * enemyData.explosionDamageRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(explosionCenter, explosionCenter + Vector3.right * enemyData.explosionForceRadius);

#if UNITY_EDITOR
        // 씬 뷰에 텍스트 라벨 표시
        // 데미지 범위 라벨
        Vector3 damageTextPos = explosionCenter + Vector3.up * enemyData.explosionDamageRadius + Vector3.up * 0.5f;
        UnityEditor.Handles.Label(damageTextPos, 
            $"Damage Range\n{enemyData.explosionDamageRadius:F1}m\n({enemyData.explosionMaxDamage}~{enemyData.explosionMinDamage} dmg)",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.red },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            });

        // 물리 범위 라벨
        Vector3 forceTextPos = explosionCenter + Vector3.right * enemyData.explosionForceRadius + Vector3.right * 0.5f;
        UnityEditor.Handles.Label(forceTextPos, 
            $"Force Range\n{enemyData.explosionForceRadius:F1}m\n(Force: {enemyData.explosionForce})",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.yellow },
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            });
#endif
    }

    #endregion
}