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
    
    [Header("Slime Settings")]
    public float slimeEmitInterval = 10f;
    private float lastSlimeEmitTime;
    
    [Header("Movement")]
    [HideInInspector] public float moveSpeed = 2f;
    
    [Header("Detection System")]
    [Tooltip("플레이어 달리기 소리 감지 범위")]
    public float soundDetectionRange = 5f;
    
    
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
    /// 상태 전환 없이 즉시 슬라임을 배출합니다. (도망 중 사용)
    /// </summary>
    public void EmitSlimeDirectly()
    {
        if (SlimePrefab != null)
        {
            GameObject slime = GameObject.Instantiate(SlimePrefab, transform.position, Quaternion.identity);
            GameObject.Destroy(slime, SlimeDuration);
            DebugManager.Log("Moo: 도망 중 슬라임 배출!", isShowDebug);
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
    /// Debug UI 그리기 (Base 클래스 사용)
    /// </summary>
    protected override void OnGUI()
    {
        // Base 클래스에서 모든 디버그 정보 처리
        base.OnGUI();
    }

    /// <summary>
    /// Moo만의 특수 디버그 정보 표시 (감지 범위 시각화)
    /// </summary>
    protected override void DrawCustomDebugInfo()
    {
        if (Camera.main == null) return;

        // 감지 범위 시각화
        DrawDetectionRanges();
    }

    /// <summary>
    /// HP/Stamina 바 그리기 (Moo용 오버라이드)
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

        // Stamina 바 그리기
        DrawDebugBar(startX, startY + baseBarHeight + barSpacing, baseBarWidth, baseBarHeight,
                     "Stamina", (int)currentStamina, (int)maxStamina, StaminaPercent, GetStaminaColor(StaminaPercent));

        // 상태 텍스트 (두 바 아래에 표시)
        DrawStateText(startX, startY + (baseBarHeight + barSpacing) * 2 + 5f, baseBarWidth);
    }

    /// <summary>
    /// 상태 텍스트 표시 (Moo용 오버라이드 - 감지 정보 포함)
    /// </summary>
    protected override void DrawStateText(float x, float y, float width)
    {
        // 현재 상태 표시
        string stateText = currentState != null ? $"State: {currentState.GetType().Name}" : "State: None";
        if (IsExhausted)
        {
            stateText += " [EXHAUSTED]";
        }
        
        GUIStyle stateStyle = new GUIStyle();
        stateStyle.alignment = TextAnchor.MiddleCenter;
        stateStyle.fontSize = (int)(10 * debugUIScale);
        stateStyle.normal.textColor = IsExhausted ? Color.red : Color.yellow;
        stateStyle.fontStyle = FontStyle.Bold;

        DrawTextWithOutline(x, y, width, 20, stateText, stateStyle);

        // 감지 정보 표시
        string detectionType;
        bool detected = DetectPlayerThreat(out detectionType);
        if (detected)
        {
            string detectionText = $"Detection: {detectionType}";
            
            GUIStyle detectionStyle = new GUIStyle();
            detectionStyle.alignment = TextAnchor.MiddleCenter;
            detectionStyle.fontSize = (int)(9 * debugUIScale);
            detectionStyle.normal.textColor = Color.red;
            detectionStyle.fontStyle = FontStyle.Bold;
            
            DrawTextWithOutline(x, y + 20, width, 20, detectionText, detectionStyle);
        }
    }

    /// <summary>
    /// Enemy 표시 이름 반환 (Moo 오버라이드)
    /// </summary>
    protected override string GetEnemyDisplayName()
    {
        return "Moo";
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

} 