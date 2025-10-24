using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using System;

/// <summary>
/// RSP 타입 적의 AI 컨트롤러입니다.
/// 특정 이벤트들을 따로 구축해서 처리하는게 좋을거같음.
/// 
/// </summary>
public class EnemyAIRSP : EnemyAIController<RSPEnemyAIData> ,IInteractableF
{
    // RSP 전용 프로퍼티
    public float AttackCooldown => enemyData.attackCooldown;
    public float SpecialAttackCooldown => enemyData.specialAttackCooldown;
    public float SpecialAttackRange => enemyData.specialAttackRange;
    public bool IsStackZero => stack == 0;
    public bool isAnimationEnd = false;
    public bool isPlayerFound = false;
    
    public bool isCoolDown = false; // 홀드 이후에 쿨타임 처리
    public bool isGround = true; // 바닥에 있는지 여부를 저장하는 변수

    [SerializeField] private float groundCheckDistance = 0.3f; // 지면 체크 거리
    [SerializeField] private LayerMask groundLayer; // 지면으로 간주할 레이어
    [SerializeField] private Vector3 rayOffset = new Vector3(0, 0.1f, 0); // 레이캐스트 시작점 오프셋
    
    [Header("에미션 제어")]
    [Tooltip("비활성화 시 에미션을 끌 렌더러와 복구 값 리스트")]
    [SerializeField] private EmissionRendererData[] emissionRenderers;
    
    // 에미션 제어용
    private bool isEmissionEnabled = true;
    private const string EmissionKeyword = "_EMISSION";
    private Dictionary<Renderer, Color[]> originalEmissiveColors = new Dictionary<Renderer, Color[]>(); // 렌더러별 원본 색상 배열

    private string interactionText = "가위바위보 하기";
    public string InteractionTextF { get => interactionText; set => interactionText = value; }
    public bool IgnoreInteractionF { get => stack == 0; set => IgnoreInteractionF = value; }


    public RSPSystem rspSystem;
    public Animator animator;

    [SerializeField] private int stack = 0; // 반드시 플레이 해야하는 스택
    private Stack<EventInstance> soundInstances = new Stack<EventInstance>();

    

    public Action OnStackZero;

    public readonly string WinAnimationName = "RSP_Win";
    public readonly string LoseAnimationName = "RSP_Lose";
    public readonly string DrawAnimationName = "RSP_Draw";
    public readonly string IdleAnimationName = "RSP_Idle";
    public readonly string HoldingAnimationName = "RSP_Stop";
    
    [Tooltip("슬롯머신 UI 위치")]
    public Transform slotMachineUIPosition;

    protected override void Start()
    {
        base.Start();
        // RSP 특화 초기화
        rspSystem = GetComponent<RSPSystem>();
        Player player = GameManager.instance.currentPlayer;
        ChangeState(new RSPPatrolState(this));
        OnStackZero += () => 
        {
            isCoolDown = true;
            Debug.Log("RSP: 스택 0 - 비활성화 상태로 전환 및 쿨다운 시작");
            
            // 비활성화 상태로 전환
            ChangeState(new RSPDisableState(this));
            
            // enemyData의 비활성화 지속 시간 사용
            Invoke(nameof(ResetCoolDown), enemyData.disabledDuration);
        };
        
        // 에미션 원본 색상 캐시
        CacheOriginalEmissiveColors();
    }
    
    /// <summary>
    /// 렌더러에서 현재 에미션 색상을 캐시합니다.
    /// </summary>
    private void CacheOriginalEmissiveColors()
    {
        if (emissionRenderers == null || emissionRenderers.Length == 0)
            return;
            
        foreach (EmissionRendererData data in emissionRenderers)
        {
            if (data.renderer == null)
                continue;
            
            // 렌더러에서 메테리얼 가져오기 (인스턴스 자동 생성)
            Material[] materials = data.renderer.materials;
            Color[] colors = new Color[materials.Length];
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].HasProperty("_EmissiveColor"))
                {
                    // 현재 에미션 색상 저장 (설정된 intensity 적용)
                    Color currentColor = materials[i].GetColor("_EmissiveColor");
                    
                    // 색상 방향 정규화
                    float magnitude = Mathf.Sqrt(currentColor.r * currentColor.r + 
                                                 currentColor.g * currentColor.g + 
                                                 currentColor.b * currentColor.b);
                    
                    Color baseDirection;
                    if (magnitude > 0.001f)
                    {
                        baseDirection = new Color(currentColor.r / magnitude, 
                                                 currentColor.g / magnitude, 
                                                 currentColor.b / magnitude, 
                                                 currentColor.a);
                    }
                    else
                    {
                        baseDirection = Color.white;
                    }
                    
                    // 설정된 인텐시티로 색상 계산
                    colors[i] = baseDirection * data.emissionIntensity;
                }
                else
                {
                    colors[i] = Color.black;
                }
            }
            
            originalEmissiveColors[data.renderer] = colors;
            Debug.Log($"RSP: 렌더러 '{data.renderer.name}' 에미션 캐시 완료 (메테리얼 {materials.Length}개, intensity: {data.emissionIntensity})");
        }
    }

    private void ResetCoolDown()
    {
        isCoolDown = false;
        Debug.Log("RSP: 쿨타임 초기화");
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // 메테리얼 인스턴스는 Renderer가 자동으로 관리하므로 Dictionary만 클리어
        originalEmissiveColors.Clear();
        
        Debug.Log("RSP: 에미션 데이터 정리 완료");
    }

    protected override void Update()
    {
        base.Update();
        
        // agent의 현재 속도를 기반으로 애니메이터 파라미터 업데이트
        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }

        // 지면 체크 수행
        CheckGround();

        // 테스트용: F2 키로 스택 증가
        if (Input.GetKeyDown(KeyCode.F2))
        {
            IncrementStack();
            Debug.Log($"RSP: 스택 증가 테스트 (현재 스택: {stack})");
        }
    }

    /// <summary>
    /// 레이캐스트를 사용하여 RSP가 지면 위에 있는지 확인하는 메서드
    /// </summary>
    private void CheckGround()
    {
        // 현재 위치에서 아래쪽으로 레이캐스트 발사
        RaycastHit[] hits;
        Vector3 rayOrigin = transform.position + rayOffset; // 오프셋 적용된 시작점
        
        // 디버그 레이 표시 (에디터에서만 보임)
        Debug.DrawRay(rayOrigin, Vector3.down * (groundCheckDistance + 0.1f), Color.red);
        
        // 전체를 검사할 수 있는 레이캐스트로 지면 체크
        hits = Physics.RaycastAll(rayOrigin, Vector3.down, groundCheckDistance);
        bool groundDetected = false;
        
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                groundDetected = true;
                Debug.DrawLine(rayOrigin, hit.point, Color.green);
                break;
            }
        }
        
        if (groundDetected)
        {
            isGround = true;
        }
        else
        {
            isGround = false;
            // Debug.Log("RSP: 지면에 있지 않음");
        }
    }

    [ContextMenu("ChangeCenterAttackState")]
    public override void AttackCenter()
    {
        base.AttackCenter();
        ChangeState(new RSPCenterAttackState(this, utilityCalculator));
    }

    protected override void HandleTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        base.HandleTimeOfDayChanged(newTimeOfDay);
        // RSP 특화 시간 변경 처리
        Debug.Log($"RSP: 시간이 {newTimeOfDay}로 변경되었습니다.");
    }

    public void HitEventInteractionF(Player rayOrigin)
    {
        ChangeState(new RSPHoldingState(this, utilityCalculator, rayOrigin, rspSystem));
    }

    private IEnumerator IncrementCompulsoryPlayStack()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // 60초 대기
            IncrementStack();
        }
    }

    public void StartCheckCompulsoryPlayStack()
    {
        StartCoroutine(CheckCompulsoryPlayStack());
    }

    private IEnumerator CheckCompulsoryPlayStack()
    {
        while (true)
        {
            if (stack >= 4)
            {
                ChangeState(new RSPRageState(this, utilityCalculator, GameManager.instance.GetNearestPlayer(transform)));
                Debug.Log("RSP: 추적 상태 (광란) 상태로 전환");
                break;
            }
            yield return null;
        }
        Debug.Log("광란 체크 종료");
    }

    public void IncrementStack()
    {
        stack++;
        PlaySoundBasedOnCompulsoryPlayStack(stack);
    }

    public void DecrementStack()
    {
        if (stack > 0)
        {
            stack--;
            if (soundInstances.Count > 0)
            {
                EventInstance lastInstance = soundInstances.Pop();
                if (lastInstance.isValid())
                {
                    Debug.Log("RSP: 음악 중지");
                    lastInstance.stop(STOP_MODE.IMMEDIATE);
                    lastInstance.release();
                }
            }
            
            // 스택이 감소한 후 0이 되었는지 확인
            if (stack == 0)
            {
                Debug.Log("RSP: 스택이 0이 되었습니다.");
                OnStackZero?.Invoke();
                // ChangeState(new RSPPatrolState(this));
            }
        }
    }

    public bool IsInRageState()
    {
        return currentState is RSPRageState;
    }

    public int GetCompulsoryPlayStack()
    {
        return stack;
    }

    #region Audio

    public void PlaySoundBasedOnCompulsoryPlayStack(int index)
    {
        if (stack == 0)
        {
            return; // compulsoryPlayStack가 0이면 재생안함
        }

        EventInstance instance = default; // 기본값으로 초기화
        switch (index)
        {
            case 1:
                instance = AudioManager.instance.PlayOneShot(FMODEvents.instance.rspStack[0], transform);
                break;
            case 2:
                instance = AudioManager.instance.PlayOneShot(FMODEvents.instance.rspStack[1], transform);
                break;
            case 3:
                instance = AudioManager.instance.PlayOneShot(FMODEvents.instance.rspStack[2], transform);
                break;
            case 4:
                instance = AudioManager.instance.PlayOneShot(FMODEvents.instance.rspStack[3], transform);
                break;
        }

        if (instance.isValid())
        {
            soundInstances.Push(instance);
        }
    }

    #endregion

    #region Animation
    public void PlayAnimation(string animationName, float transitionTime = 0.2f )
    {
        animator.CrossFade(animationName, transitionTime);
    }

    public void AniEvt_AnimationEnd()
    {
        isAnimationEnd = true;
    }
    #endregion

    #region Emission Control
    
    /// <summary>
    /// 에미션을 활성화하고 저장된 인텐시티 값으로 복원합니다.
    /// </summary>
    public void EnableEmission()
    {
        if (isEmissionEnabled) return;
        
        if (emissionRenderers == null || emissionRenderers.Length == 0)
        {
            Debug.LogWarning("RSP: 에미션 렌더러가 설정되지 않았습니다.");
            return;
        }
        
        foreach (EmissionRendererData data in emissionRenderers)
        {
            if (data.renderer == null)
                continue;
            
            // 캐시된 색상 배열 가져오기
            if (originalEmissiveColors.TryGetValue(data.renderer, out Color[] cachedColors))
            {
                // 렌더러에서 메테리얼 가져오기
                Material[] materials = data.renderer.materials;
                
                for (int i = 0; i < Mathf.Min(materials.Length, cachedColors.Length); i++)
                {
                    if (materials[i].HasProperty("_EmissiveColor"))
                    {
                        materials[i].SetColor("_EmissiveColor", cachedColors[i]);
                    }
                }
                
                Debug.Log($"RSP: '{data.renderer.name}' 에미션 활성화 (intensity: {data.emissionIntensity})");
            }
            else
            {
                Debug.LogWarning($"RSP: '{data.renderer.name}' 캐시된 색상을 찾을 수 없습니다.");
            }
        }
        
        isEmissionEnabled = true;
        Debug.Log("RSP: 에미션 활성화 완료");
    }
    
    /// <summary>
    /// 에미션을 비활성화합니다 (인텐시티를 0으로).
    /// </summary>
    public void DisableEmission()
    {
        if (!isEmissionEnabled) return;
        
        if (emissionRenderers == null || emissionRenderers.Length == 0)
        {
            Debug.LogWarning("RSP: 에미션 렌더러가 설정되지 않았습니다.");
            return;
        }
        
        foreach (EmissionRendererData data in emissionRenderers)
        {
            if (data.renderer == null)
                continue;
            
            // 렌더러에서 메테리얼 가져오기
            Material[] materials = data.renderer.materials;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].HasProperty("_EmissiveColor"))
                {
                    // 에미션 인텐시티를 0으로 (Color.black)
                    materials[i].SetColor("_EmissiveColor", Color.black);
                }
                else
                {
                    Debug.LogWarning($"RSP: '{data.renderer.name}' 에미션 속성을 찾을 수 없습니다.");
                }
            }
            
            Debug.Log($"RSP: '{data.renderer.name}' 에미션 비활성화 (intensity: 0)");
        }
        
        isEmissionEnabled = false;
        Debug.Log("RSP: 에미션 비활성화 완료");
    }
    
    #endregion

    #region Test
    [ContextMenu("TestForRage")]
    public void TestForRage()
    {
        stack = 4;
        Debug.Log("RSP: 추적 상태 (광란) 상태로 전환");
    }
    [ContextMenu("AddForRage")]
    public void AddForRage()
    {
        IncrementStack();
        Debug.Log("RSP: 추적 상태 (광란) 상태로 전환");
    }
    [ContextMenu("SubForRage")]
    public void SubForRage()
    {
        DecrementStack();
        Debug.Log("RSP: 추적 상태 (광란) 상태로 전환");
    }
    
    [ContextMenu("Test - Disable Emission")]
    public void TestDisableEmission()
    {
        DisableEmission();
    }
    
    [ContextMenu("Test - Enable Emission")]
    public void TestEnableEmission()
    {
        EnableEmission();
    }
    #endregion

    /// <summary>
    /// 에디터에서 지면 체크 시각화
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // 지면 체크 레이 시각화
        Vector3 rayOrigin = transform.position + rayOffset;
        Gizmos.color = isGround ? Color.green : Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(rayOrigin + Vector3.down * groundCheckDistance, 0.1f);
    }

    /// <summary>
    /// Scene 뷰에서 RSP 정보 시각화
    /// </summary>
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (vision == null) return;

        // Disable 상태 확인
        bool isDisabled = currentState is RSPDisableState;

        // Disable 상태일 때 회색 반투명 구로 표시
        if (isDisabled)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Gizmos.DrawSphere(transform.position + Vector3.up, 1.5f);
            
            // 비활성화 표시 링
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, vision.sightRange * 0.5f);
        }

        // 시야 범위 시각화 (와이어 구) - Disable 상태면 흐리게
        if (isDisabled)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        }
        else
        {
            Gizmos.color = CheckPlayerInSight() ? Color.red : Color.green;
        }
        Gizmos.DrawWireSphere(transform.position, vision.sightRange);

        // 특수 공격 범위 시각화 (Disable 상태가 아닐 때만)
        if (!isDisabled && SpecialAttackRange > 0)
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, SpecialAttackRange);
        }

        // 스택이 높을 때 위험 범위 표시 (Disable 상태가 아닐 때만)
        if (!isDisabled && GetCompulsoryPlayStack() >= 3)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 5f);
        }

        // 플레이어를 향한 선 그리기 (Disable 상태가 아닐 때만)
        if (!isDisabled && playerTr != null)
        {
            Gizmos.color = CheckPlayerInSight() ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, playerTr.position + Vector3.up);
        }
    }

    #region Debug UI Override

    /// <summary>
    /// HP/스택 바 그리기 (RSP용 오버라이드)
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
        float hpPercent = (float)hp / enemyData.hpMax;
        DrawDebugBar(startX, startY, baseBarWidth, baseBarHeight, 
                     "HP", hp, enemyData.hpMax, hpPercent, GetHPColor(hpPercent));

        // 스택 바 그리기 (최대 4)
        int currentStack = GetCompulsoryPlayStack();
        int maxStack = 4;
        float stackPercent = (float)currentStack / maxStack;
        Color stackColor = GetStackColor(stackPercent);
        DrawDebugBar(startX, startY + baseBarHeight + barSpacing, baseBarWidth, baseBarHeight,
                     "Stack", currentStack, maxStack, stackPercent, stackColor);

        // 쿨다운 바 그리기 (쿨다운 중일 때만)
        if (isCoolDown)
        {
            // 쿨다운 진행도를 표시 (정확한 진행도를 보여주려면 쿨다운 시작 시간 추적 필요)
            DrawDebugBar(startX, startY + (baseBarHeight + barSpacing) * 2, baseBarWidth, baseBarHeight,
                         "Cooldown", 0, 1, 0f, new Color(0.5f, 0.5f, 1f));
        }

        // 상태 텍스트 (바들 아래에 표시)
        float textYOffset = isCoolDown ? (baseBarHeight + barSpacing) * 3 + 5f : (baseBarHeight + barSpacing) * 2 + 5f;
        DrawStateText(startX, startY + textYOffset, baseBarWidth);
    }

    /// <summary>
    /// 스택 퍼센트에 따른 색상 반환
    /// </summary>
    private Color GetStackColor(float stackPercent)
    {
        if (stackPercent >= 1f)
            return Color.red; // 스택 최대 (위험)
        else if (stackPercent > 0.5f)
            return Color.yellow; // 스택 절반 이상
        else if (stackPercent > 0f)
            return Color.cyan; // 스택 있음
        else
            return Color.green; // 스택 0 (안전)
    }

    /// <summary>
    /// 상태 텍스트 표시 (RSP용 오버라이드 - 쿨다운 정보 포함)
    /// </summary>
    protected override void DrawStateText(float x, float y, float width)
    {
        // 현재 상태 표시
        string stateText = currentState != null ? $"State: {currentState.GetType().Name}" : "State: None";
        
        // Disable 상태 확인
        bool isDisabled = currentState is RSPDisableState;
        
        if (isDisabled)
        {
            stateText += " [DISABLED]";
        }
        if (isCoolDown)
        {
            stateText += " [COOLDOWN]";
        }
        if (!isGround)
        {
            stateText += " [AIRBORNE]";
        }
        if (!isEmissionEnabled)
        {
            stateText += " [NO EMISSION]";
        }
        
        GUIStyle stateStyle = new GUIStyle();
        stateStyle.alignment = TextAnchor.MiddleCenter;
        stateStyle.fontSize = (int)(10 * debugUIScale);
        
        // Disable 상태면 회색, 쿨다운이면 청록색, 아니면 노란색
        if (isDisabled)
        {
            stateStyle.normal.textColor = Color.gray;
        }
        else if (isCoolDown)
        {
            stateStyle.normal.textColor = Color.cyan;
        }
        else
        {
            stateStyle.normal.textColor = Color.yellow;
        }
        stateStyle.fontStyle = FontStyle.Bold;

        DrawTextWithOutline(x, y, width, 20, stateText, stateStyle);

        // 스택 0일 때 추가 정보
        if (IsStackZero && !isCoolDown)
        {
            string interactText = "Interaction Available";
            
            GUIStyle interactStyle = new GUIStyle();
            interactStyle.alignment = TextAnchor.MiddleCenter;
            interactStyle.fontSize = (int)(9 * debugUIScale);
            interactStyle.normal.textColor = Color.green;
            interactStyle.fontStyle = FontStyle.Bold;
            
            DrawTextWithOutline(x, y + 20, width, 20, interactText, interactStyle);
        }
        
        // Disable 상태일 때 쿨다운 타이머 표시
        if (isDisabled && isCoolDown)
        {
            string disabledText = "⏸ DISABLED - Waiting for reactivation...";
            
            GUIStyle disabledStyle = new GUIStyle();
            disabledStyle.alignment = TextAnchor.MiddleCenter;
            disabledStyle.fontSize = (int)(9 * debugUIScale);
            disabledStyle.normal.textColor = Color.red;
            disabledStyle.fontStyle = FontStyle.Bold;
            
            DrawTextWithOutline(x, y + 20, width, 20, disabledText, disabledStyle);
        }
    }

    /// <summary>
    /// Enemy 표시 이름 반환 (RSP 오버라이드)
    /// </summary>
    protected override string GetEnemyDisplayName()
    {
        return "RSP";
    }

    /// <summary>
    /// RSP만의 특수 디버그 정보 표시 (감지 범위, 이동 방향, 지면 체크 등)
    /// </summary>
    protected override void DrawCustomDebugInfo()
    {
        if (Camera.main == null) return;

        // 1. 감지 범위 시각화 (시야각)
        DrawDetectionRange();

        // 2. 이동 방향 시각화
        DrawMovementDirection();

        // 3. 플레이어와의 거리 표시
        DrawPlayerDistance();

        // 4. 지면 체크 레이 시각화 (게임뷰)
        DrawGroundCheck();

        // 5. 스택 위험도 시각화
        if (GetCompulsoryPlayStack() >= 3)
        {
            DrawStackWarning();
        }
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

        // 특수 공격 범위 (원형 - 보라색)
        if (SpecialAttackRange > 0)
        {
            DrawWorldCircleGUI(transform.position, SpecialAttackRange, new Color(0.5f, 0, 1, 0.2f), 24);
        }
    }

    /// <summary>
    /// 이동 방향 시각화 (속도 벡터)
    /// </summary>
    private void DrawMovementDirection()
    {
        if (agent == null || agent.velocity.magnitude < 0.1f) return;

        Vector3 currentPos = transform.position + Vector3.up * 0.5f;
        Vector3 targetPos = currentPos + agent.velocity.normalized * 2f;

        Vector2 currentScreen = WorldToGUIPoint(currentPos);
        Vector2 targetScreen = WorldToGUIPoint(targetPos);

        if (currentScreen == Vector2.zero || targetScreen == Vector2.zero) return;

        // 속도에 따라 색상 변경
        float speedRatio = Mathf.Clamp01(agent.velocity.magnitude / agent.speed);
        Color directionColor = Color.Lerp(Color.green, Color.yellow, speedRatio);

        // 이동 방향 선 그리기
        DrawGUILine(currentScreen, targetScreen, directionColor, 3f);

        // 화살표 끝 마커
        float markerSize = 10f * debugUIScale;
        GUI.color = directionColor;
        GUI.DrawTexture(new Rect(targetScreen.x - markerSize / 2f, targetScreen.y - markerSize / 2f, markerSize, markerSize), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    /// <summary>
    /// 플레이어와의 거리 표시
    /// </summary>
    private void DrawPlayerDistance()
    {
        if (playerTr == null) return;

        float distance = Vector3.Distance(transform.position, playerTr.position);
        Vector3 midPoint = (transform.position + playerTr.position) / 2f + Vector3.up;
        Vector2 midScreen = WorldToGUIPoint(midPoint);

        if (midScreen == Vector2.zero) return;

        // 거리 텍스트 스타일
        GUIStyle distanceStyle = new GUIStyle();
        distanceStyle.alignment = TextAnchor.MiddleCenter;
        distanceStyle.fontSize = (int)(10 * debugUIScale);
        distanceStyle.normal.textColor = Color.white;
        distanceStyle.fontStyle = FontStyle.Bold;

        // 거리에 따른 배경 색상 (가까우면 빨강, 멀면 초록)
        float dangerRatio = Mathf.Clamp01(1f - (distance / vision.sightRange));
        Color bgColor = Color.Lerp(new Color(0, 0.5f, 0, 0.7f), new Color(0.5f, 0, 0, 0.7f), dangerRatio);

        float labelWidth = 100f * debugUIScale;
        float labelHeight = 25f * debugUIScale;

        // 배경
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(midScreen.x - labelWidth / 2f, midScreen.y - labelHeight / 2f, labelWidth, labelHeight), Texture2D.whiteTexture);

        // 텍스트
        string distanceText = $"Player\n{distance:F1}m";
        DrawTextWithOutline(midScreen.x - labelWidth / 2f, midScreen.y - labelHeight / 2f, labelWidth, labelHeight, distanceText, distanceStyle);

        GUI.color = Color.white;
    }

    /// <summary>
    /// 지면 체크 레이 시각화 (게임뷰)
    /// </summary>
    private void DrawGroundCheck()
    {
        Vector3 rayOrigin = transform.position + rayOffset;
        Vector3 rayEnd = rayOrigin + Vector3.down * groundCheckDistance;

        Vector2 originScreen = WorldToGUIPoint(rayOrigin);
        Vector2 endScreen = WorldToGUIPoint(rayEnd);

        if (originScreen == Vector2.zero || endScreen == Vector2.zero) return;

        Color rayColor = isGround ? new Color(0, 1, 0, 0.8f) : new Color(1, 0, 0, 0.8f);
        DrawGUILine(originScreen, endScreen, rayColor, 2f);

        // 지면 상태 표시
        if (!isGround)
        {
            GUIStyle airborneStyle = new GUIStyle();
            airborneStyle.alignment = TextAnchor.MiddleCenter;
            airborneStyle.fontSize = (int)(9 * debugUIScale);
            airborneStyle.normal.textColor = Color.yellow;
            airborneStyle.fontStyle = FontStyle.Bold;

            float labelWidth = 80f * debugUIScale;
            float labelHeight = 20f * debugUIScale;

            // 배경
            GUI.color = new Color(0.5f, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(endScreen.x - labelWidth / 2f, endScreen.y, labelWidth, labelHeight), Texture2D.whiteTexture);

            // 텍스트
            DrawTextWithOutline(endScreen.x - labelWidth / 2f, endScreen.y, labelWidth, labelHeight, "AIRBORNE!", airborneStyle);
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// 스택 위험도 시각화 (스택이 3 이상일 때)
    /// </summary>
    private void DrawStackWarning()
    {
        // 위험 반경 표시 (빨간 원)
        float warningRadius = 5f;
        DrawWorldCircleGUI(transform.position, warningRadius, new Color(1, 0, 0, 0.2f), 32);

        // 깜빡이는 효과
        float pulseSpeed = 5f;
        float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        
        Vector3 warningPos = transform.position + Vector3.up * 3f;
        Vector2 warningScreen = WorldToGUIPoint(warningPos);

        if (warningScreen == Vector2.zero) return;

        GUIStyle warningStyle = new GUIStyle();
        warningStyle.alignment = TextAnchor.MiddleCenter;
        warningStyle.fontSize = (int)(12 * debugUIScale);
        warningStyle.normal.textColor = Color.Lerp(Color.red, Color.yellow, pulse);
        warningStyle.fontStyle = FontStyle.Bold;

        float labelWidth = 120f * debugUIScale;
        float labelHeight = 30f * debugUIScale;

        // 배경 (깜빡임)
        GUI.color = new Color(1, 0, 0, 0.6f * pulse);
        GUI.DrawTexture(new Rect(warningScreen.x - labelWidth / 2f, warningScreen.y - labelHeight / 2f, labelWidth, labelHeight), Texture2D.whiteTexture);

        // 텍스트
        string warningText = GetCompulsoryPlayStack() >= 4 ? "!! RAGE !!" : "! DANGER !";
        DrawTextWithOutline(warningScreen.x - labelWidth / 2f, warningScreen.y - labelHeight / 2f, labelWidth, labelHeight, warningText, warningStyle);

        GUI.color = Color.white;
    }

    #endregion
}