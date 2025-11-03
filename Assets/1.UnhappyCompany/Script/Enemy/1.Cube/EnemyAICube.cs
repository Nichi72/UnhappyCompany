using UnityEngine;
using FMODUnity;

/// <summary>
/// Cube 타입 적의 AI 컨트롤러입니다.
/// NavMesh 기반으로 이동하며, 공격 시 물리 기반 돌진 공격을 수행합니다.
/// Vision 구조체를 사용하여 감지 범위를 설정합니다.
/// - vision.proximityDetectionRange: 공격 범위 (AttackRadius)
/// - vision.sightRange: 추적/시야 범위 (ViewRadius)
/// </summary>
public class EnemyAICube : EnemyAIController<CubeEnemyAIData>
{
    // Cube 전용 프로퍼티 (Vision 구조체 기반)
    public float AttackRadius => vision.proximityDetectionRange;
    public float ViewRadius => vision.sightRange;
    public float AttackGiveUpRadius => enemyData.attackGiveUpRadius;
    public float RushForce => enemyData.rushForce;
    public float TorqueForce => enemyData.torqueForce;
    public float ChaseSpeedMultiplier => enemyData.chaseSpeedMultiplier;
    public float AttackCooldown => CurrentTimeOfDay == TimeOfDay.Morning ? enemyData.morningAttackCooldown : enemyData.afternoonAttackCooldown;
    public float AttackCastingTime => CurrentTimeOfDay == TimeOfDay.Morning ? enemyData.morningAttackCastingTime : enemyData.afternoonAttackCastingTime;
    public float ReEnableDelay => CurrentTimeOfDay == TimeOfDay.Morning ? enemyData.morningReEnableDelay : enemyData.afternoonReEnableDelay;
    public int RushDamage => enemyData.rushDamage;
    public int ContactDamage => enemyData.contactDamage;
    public float ContactDamageCooldown => enemyData.contactDamageCooldown;
    
    // 돌진 상태 체크
    private bool isRushing = false;
    private bool hasDealtRushDamage = false; // 돌진 데미지를 한 번만 주기 위한 플래그
    public bool IsRushing => isRushing;
    
    // 접촉 데미지 쿨다운
    private float lastContactDamageTime = 0f;
    
    // 충돌 사운드 쿨타임
    private float lastPlayerCollisionSoundTime = -999f;
    private float lastObjectCollisionSoundTime = -999f;
    private const float COLLISION_SOUND_COOLDOWN = 0.5f; // 충돌 사운드 쿨타임 (초)

    [Header("BlendShape Settings")]
    public Transform blendShapeTransform;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    [ReadOnly] [SerializeField] private float currentBlendShapeValue = 0f;
    
    // BlendShape 애니메이션
    private bool isAnimatingBlendShape = false;
    private float blendShapeTargetValue = 0f;
    private float blendShapeAnimationSpeed = 200f; // 초당 변화 속도 (0->100을 0.5초에)
    
    // BlendShapeTransform 회전 설정
    [Header("BlendShape Rotation Settings")]
    [Tooltip("속도에 따른 회전 속도 배율")]
    public float rotationSpeedMultiplier = 50f;
    [Tooltip("회전을 리셋하는 속도")]
    public float rotationResetSpeed = 180f;
    [Tooltip("회전이 시작되는 최소 속도")]
    public float minSpeedForRotation = 0.5f;
    [ReadOnly] [SerializeField] private float currentRotationX = 0f;
    
    // BlendShapeTransform 크기 설정
    [Header("BlendShape Scale Settings")]
    [Tooltip("공격 차징 시 줄어드는 크기 비율 (0.8 = 80% 크기)")]
    public float attackChargingScaleMultiplier = 0.8f;
    [Tooltip("크기 변화 속도 (높을수록 빠르게 변함)")]
    public float scaleChangeSpeed = 15f;
    [Tooltip("발사 몇 초 전에 크기를 줄일지 (0.2 = 발사 0.2초 전)")]
    public float scaleChargingStartBeforeRush = 0.2f;
    private Vector3 originalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private bool isScaleAnimating = false;
    
    // 공격 중 플래그
    private bool isAttacking = false;
    
    // 재활성화 타이머 (디버그용)
    [ReadOnly] [SerializeField] private bool isWaitingForReEnable = false;
    [ReadOnly] [SerializeField] private bool isWaitingForLanding = false;
    [ReadOnly] [SerializeField] private float reEnableTimer = 0f;
    [ReadOnly] [SerializeField] private float reEnableMaxTime = 0f;
    
    /// <summary>
    /// 돌진 상태 설정 (AttackState에서 호출)
    /// </summary>
    public void SetRushingState(bool rushing)
    {
        isRushing = rushing;
        if (rushing)
        {
            // 돌진 시작 시 데미지 플래그 초기화
            hasDealtRushDamage = false;
        }
    }
    
    /// <summary>
    /// 공격 상태 설정 (AttackState에서 호출)
    /// </summary>
    public void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;
    }
    
    /// <summary>
    /// 재활성화 타이머 시작 (AttackState에서 호출)
    /// </summary>
    public void StartReEnableTimer(float maxTime, bool waitingForLanding)
    {
        isWaitingForReEnable = true;
        isWaitingForLanding = waitingForLanding;
        reEnableTimer = 0f;
        reEnableMaxTime = maxTime;
    }
    
    /// <summary>
    /// 재활성화 타이머 업데이트 (AttackState에서 호출)
    /// </summary>
    public void UpdateReEnableTimer(float deltaTime)
    {
        if (isWaitingForReEnable && !isWaitingForLanding)
        {
            reEnableTimer += deltaTime;
        }
    }
    
    /// <summary>
    /// 착지 감지 시 호출
    /// </summary>
    public void OnLandingDetected()
    {
        isWaitingForLanding = false;
    }
    
    /// <summary>
    /// 재활성화 타이머 종료
    /// </summary>
    public void StopReEnableTimer()
    {
        isWaitingForReEnable = false;
        isWaitingForLanding = false;
        reEnableTimer = 0f;
        reEnableMaxTime = 0f;
    }

    protected override void Update()
    {
        base.Update();
        
        // BlendShape 애니메이션 업데이트 (조건이 만족될 때만 실행)
        if (isAnimatingBlendShape && skinnedMeshRenderer != null)
        {
            UpdateBlendShapeAnimation();
        }
        
        // BlendShapeTransform 회전 업데이트 (속도 기반)
        if (blendShapeTransform != null)
        {
            UpdateBlendShapeRotation();
            
            // BlendShapeTransform 크기 애니메이션 업데이트
            if (isScaleAnimating)
            {
                UpdateBlendShapeScale();
            }
        }
    }
    
    /// <summary>
    /// BlendShape 애니메이션 업데이트 (매 프레임)
    /// </summary>
    private void UpdateBlendShapeAnimation()
    {
        // 목표 값을 향해 부드럽게 이동
        float step = blendShapeAnimationSpeed * Time.deltaTime;
        currentBlendShapeValue = Mathf.MoveTowards(currentBlendShapeValue, blendShapeTargetValue, step);
        
        // SkinnedMeshRenderer에 적용
        skinnedMeshRenderer.SetBlendShapeWeight(0, currentBlendShapeValue);
        
        // 목표 값에 도달하면 애니메이션 중단
        if (Mathf.Approximately(currentBlendShapeValue, blendShapeTargetValue))
        {
            isAnimatingBlendShape = false;
            Debug.Log($"Cube: BlendShape 애니메이션 완료 (값: {currentBlendShapeValue})");
        }
    }
    
    /// <summary>
    /// BlendShapeTransform 회전 업데이트 (속도 기반 - 계속 회전)
    /// </summary>
    private void UpdateBlendShapeRotation()
    {
        float currentSpeed = 0f;
        
        // NavMeshAgent가 활성화되어 있으면 Agent 속도 사용
        if (agent != null && agent.enabled)
        {
            currentSpeed = agent.velocity.magnitude;
        }
        // Agent가 비활성화되어 있으면 Rigidbody 속도 사용 (돌진 중)
        else
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                currentSpeed = rb.linearVelocity.magnitude;
            }
        }
        
        // 최소 속도 이상일 때만 회전
        if (currentSpeed >= minSpeedForRotation)
        {
            // 속도에 비례한 회전 속도 계산
            float rotationSpeed = currentSpeed * rotationSpeedMultiplier;
            
            // 이번 프레임의 회전량 계산
            float deltaRotation = rotationSpeed * Time.deltaTime;
            
            // Rotate 메서드로 회전 (부드러운 누적)
            blendShapeTransform.Rotate(deltaRotation, 0f, 0f, Space.Self);
            
            // 디버그용 누적 각도 (표시만)
            currentRotationX += deltaRotation;
            if (currentRotationX >= 360f)
                currentRotationX -= 360f;
        }
    }
    
    /// <summary>
    /// BlendShapeTransform 회전을 0으로 부드럽게 리셋
    /// </summary>
    private void ResetBlendShapeRotation()
    {
        Vector3 currentEuler = blendShapeTransform.localEulerAngles;
        
        // X축 각도를 -180 ~ 180 범위로 정규화
        float normalizedX = currentEuler.x;
        if (normalizedX > 180f)
            normalizedX -= 360f;
        
        // 0에 가까우면 바로 0으로 설정
        if (Mathf.Abs(normalizedX) < 0.1f)
        {
            blendShapeTransform.localEulerAngles = new Vector3(0f, currentEuler.y, currentEuler.z);
            currentRotationX = 0f;
            return;
        }
        
        // 부드럽게 0으로 보간
        float resetAmount = rotationResetSpeed * Time.deltaTime;
        float targetX = Mathf.MoveTowards(normalizedX, 0f, resetAmount);
        
        blendShapeTransform.localEulerAngles = new Vector3(targetX, currentEuler.y, currentEuler.z);
        
        // 디버그용 각도도 리셋
        currentRotationX = Mathf.MoveTowards(currentRotationX, 0f, resetAmount);
        if (currentRotationX >= 360f)
            currentRotationX -= 360f;
    }
    
    /// <summary>
    /// BlendShape를 목표 값으로 애니메이션 시작
    /// </summary>
    public void AnimateBlendShapeTo(float targetValue)
    {
        blendShapeTargetValue = Mathf.Clamp(targetValue, 0f, 100f);
        isAnimatingBlendShape = true;
        Debug.Log($"Cube: BlendShape 애니메이션 시작 ({currentBlendShapeValue} -> {blendShapeTargetValue})");
    }
    
    /// <summary>
    /// 이동 시작 시 호출 - BlendShape 0 -> 100
    /// </summary>
    public void StartMovementBlendShape()
    {
        AnimateBlendShapeTo(100f);
    }
    
    /// <summary>
    /// 공격 시작 시 호출 - BlendShape 100 -> 0
    /// </summary>
    public void StartAttackBlendShape()
    {
        AnimateBlendShapeTo(0f);
    }
    
    /// <summary>
    /// BlendShape 애니메이션이 진행 중인지 확인
    /// </summary>
    public bool IsBlendShapeAnimating => isAnimatingBlendShape;
    
    /// <summary>
    /// BlendShapeTransform 크기 애니메이션 업데이트
    /// </summary>
    private void UpdateBlendShapeScale()
    {
        // 목표 크기로 부드럽게 보간
        blendShapeTransform.localScale = Vector3.Lerp(
            blendShapeTransform.localScale, 
            targetScale, 
            Time.deltaTime * scaleChangeSpeed
        );
        
        // 목표 크기에 거의 도달하면 애니메이션 중단
        if (Vector3.Distance(blendShapeTransform.localScale, targetScale) < 0.001f)
        {
            blendShapeTransform.localScale = targetScale;
            isScaleAnimating = false;
        }
    }
    
    /// <summary>
    /// 공격 차징 시작 - 크기 줄이기
    /// </summary>
    public void StartAttackChargingScale()
    {
        targetScale = originalScale * attackChargingScaleMultiplier;
        isScaleAnimating = true;
        Debug.Log($"Cube: 공격 차징 - 크기 축소 시작 (목표: {attackChargingScaleMultiplier * 100}%)");
    }
    
    /// <summary>
    /// 공격 러쉬 시작 - 크기 복원
    /// </summary>
    public void StartAttackRushScale()
    {
        targetScale = originalScale;
        isScaleAnimating = true;
        Debug.Log($"Cube: 공격 러쉬 - 크기 복원 시작");
    }
    
    /// <summary>
    /// 크기를 즉시 원래대로 복원 (상태 종료 시)
    /// </summary>
    public void ResetScale()
    {
        if (blendShapeTransform != null)
        {
            blendShapeTransform.localScale = originalScale;
            targetScale = originalScale;
            isScaleAnimating = false;
        }
    }


    protected override void Start()
    {
        base.Start();
        
        // Cube Vision 설정
        vision.enableProximityDetection = true;
        vision.proximityDetectionRange = 5f;  // 근접 감지 범위 (360도)
        vision.sightRange = 15f;               // 시야 범위
        vision.sightAngle = 120f;              // 시야각 (전방 120도)
        
        Debug.Log($"Cube Vision 설정 - 공격 범위: {AttackRadius}m, 추적 범위: {ViewRadius}m, 공격 포기 범위: {AttackGiveUpRadius}m");
        
        // BlendShapeTransform의 원본 크기 저장
        if (blendShapeTransform != null)
        {
            originalScale = blendShapeTransform.localScale;
            targetScale = originalScale;
        }
        
        // Cube 특화 초기화 - NavMesh 기반 순찰 시작
        ChangeState(new CubePatrolState(this, utilityCalculator));
    }

    protected override void HandleTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        base.HandleTimeOfDayChanged(newTimeOfDay);
        // Cube 특화 시간 변경 처리
        Debug.Log($"Cube: 시간이 {newTimeOfDay}로 변경되었습니다.");
    }

    /// <summary>
    /// 데미지를 받을 때 호출되는 메서드입니다.
    /// </summary>
    public override void TakeDamage(int damage, DamageType damageType)
    {
        base.TakeDamage(damage, damageType);
        
        Debug.Log($"Cube: {damage} 데미지를 받았습니다! (남은 HP: {hp})");
        
        // 피격 시 시각적 피드백 (BlendShape 깜빡임)
        if (skinnedMeshRenderer != null)
        {
            StartBlendShapeFlash();
        }
        
        // 피격 사운드 재생
        if (!FMODEvents.instance.cubeHit.IsNull)
        {
            AudioManager.instance.Play3DSoundAtPosition(
                FMODEvents.instance.cubeHit, 
                transform.position, 
                30f, 
                "Cube Hit"
            );
        }
    }
    
    /// <summary>
    /// BlendShape 깜빡임 효과 (피격 피드백)
    /// </summary>
    private void StartBlendShapeFlash()
    {
        // 짧은 깜빡임 효과
        StartCoroutine(BlendShapeFlashCoroutine());
    }
    
    private System.Collections.IEnumerator BlendShapeFlashCoroutine()
    {
        // 빠르게 100으로
        float duration = 0.1f;
        float elapsed = 0f;
        float startValue = currentBlendShapeValue;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentBlendShapeValue = Mathf.Lerp(startValue, 100f, t);
            
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(0, currentBlendShapeValue);
            }
            
            yield return null;
        }
        
        // 빠르게 0으로
        duration = 0.15f;
        elapsed = 0f;
        startValue = currentBlendShapeValue;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentBlendShapeValue = Mathf.Lerp(startValue, 0f, t);
            
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(0, currentBlendShapeValue);
            }
            
            yield return null;
        }
    }

    /// <summary>
    /// Gizmos에 공격 포기 범위 표시
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // 공격 포기 범위 (노란색)
        Color attackGiveUpColor = new Color(1f, 0.92f, 0.016f, 0.5f); // 노란색
        MyUtility.UtilityGizmos.DrawCircle(transform.position, AttackGiveUpRadius, attackGiveUpColor);
    }

    /// <summary>
    /// 게임뷰에 범위 시각화 (공격 포기 범위 추가)
    /// </summary>
    protected override void DrawRangeVisualization()
    {
        base.DrawRangeVisualization();
        
        // 공격 포기 범위 시각화 (노란색)
        Color attackGiveUpColor = new Color(1f, 0.92f, 0.016f, 0.4f); // 노란색
        DrawWorldCircleGUI(transform.position, AttackGiveUpRadius, attackGiveUpColor, 32);
    }
    
    /// <summary>
    /// 커스텀 디버그 정보 표시 (재활성화 타이머)
    /// </summary>
    protected override void DrawCustomDebugInfo()
    {
        base.DrawCustomDebugInfo();
        
        // 재활성화 타이머 표시
        if (isWaitingForReEnable)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
            screenPos.y = Screen.height - screenPos.y; // GUI 좌표계 변환
            
            // 배경
            Rect bgRect = new Rect(screenPos.x - 100, screenPos.y - 30, 200, 60);
            GUI.Box(bgRect, "");
            
            // 상태 텍스트
            GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.fontSize = 14;
            statusStyle.fontStyle = FontStyle.Bold;
            statusStyle.alignment = TextAnchor.MiddleCenter;
            
            if (isWaitingForLanding)
            {
                // 안정화 대기 중
                statusStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(screenPos.x - 100, screenPos.y - 25, 200, 25), "안정화 대기 중...", statusStyle);
            }
            else
            {
                // 타이머 진행 중
                statusStyle.normal.textColor = Color.cyan;
                float remainingTime = reEnableMaxTime - reEnableTimer;
                GUI.Label(new Rect(screenPos.x - 100, screenPos.y - 25, 200, 25), 
                    $"재활성화: {remainingTime:F1}초", statusStyle);
                
                // 프로그레스 바
                Rect progressBg = new Rect(screenPos.x - 80, screenPos.y + 5, 160, 15);
                GUI.Box(progressBg, "");
                
                float progress = reEnableTimer / reEnableMaxTime;
                Rect progressFill = new Rect(screenPos.x - 78, screenPos.y + 7, 156 * progress, 11);
                
                // 진행도에 따라 색상 변경
                Color progressColor = Color.Lerp(Color.red, Color.green, progress);
                GUI.color = progressColor;
                GUI.Box(progressFill, "");
                GUI.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// 플레이어와 충돌 시 호출 (돌진 데미지 - 한 번만)
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(ETag.Player.ToString()))
        {
            // 플레이어 충돌 사운드 재생 (쿨타임 체크)
            if (Time.time - lastPlayerCollisionSoundTime >= COLLISION_SOUND_COOLDOWN)
            {
                PlayCollisionSound(FMODEvents.instance.cubeCollisionPlayer, "Player");
                lastPlayerCollisionSoundTime = Time.time;
            }
            
            // 돌진 중이고 아직 돌진 데미지를 주지 않았다면
            if (isRushing && !hasDealtRushDamage)
            {
                IDamageable damageable = collision.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(RushDamage, DamageType.Physical);
                    hasDealtRushDamage = true; // 한 번만 데미지
                    Debug.Log($"Cube: 플레이어에게 돌진 데미지 {RushDamage} 적용!");
                }
            }
        }
        else
        {
            // 공격 상태(돌진 중)일 때 다른 오브젝트와 충돌
            if (isAttacking || isRushing)
            {
                // 적이 아닌 경우 오브젝트 충돌 사운드 (쿨타임 체크)
                if (!collision.collider.CompareTag(ETag.Enemy.ToString()))
                {
                    if (Time.time - lastObjectCollisionSoundTime >= COLLISION_SOUND_COOLDOWN)
                    {
                        PlayCollisionSound(FMODEvents.instance.cubeCollisionObject, "Object");
                        lastObjectCollisionSoundTime = Time.time;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 충돌 사운드 재생
    /// </summary>
    private void PlayCollisionSound(EventReference soundEvent, string collisionType)
    {
        if (!soundEvent.IsNull)
        {
            AudioManager.instance.Play3DSoundAtPosition(
                soundEvent,
                transform.position,
                30f,
                $"Cube Collision {collisionType}"
            );
            
            Debug.Log($"Cube: {collisionType}와 충돌 사운드 재생");
        }
    }
    
    /// <summary>
    /// 플레이어와 접촉 중일 때 지속 데미지 (Moo와 동일한 구조)
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag(ETag.Player.ToString()))
        {
            // 돌진 중이 아닐 때만 지속 데미지
            if (!isRushing)
            {
                // 쿨다운 체크 - 초당 데미지
                if (Time.time - lastContactDamageTime >= ContactDamageCooldown)
                {
                    IDamageable damageable = collision.collider.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(ContactDamage, DamageType.Physical);
                        lastContactDamageTime = Time.time;
                        Debug.Log($"Cube: 플레이어에게 접촉 데미지 {ContactDamage} 적용 (지속)");
                        
                        // 사운드 재생 (필요시)
                        // AudioManager.instance.PlayOneShot(FMODEvents.instance.cubeContactHit, transform, "Cube 접촉 데미지");
                    }
                }
            }
        }
    }
} 