using UnityEngine;

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
    public int RushDamage => enemyData.rushDamage;
    public int ContactDamage => enemyData.contactDamage;
    public float ContactDamageCooldown => enemyData.contactDamageCooldown;
    
    // 돌진 상태 체크
    private bool isRushing = false;
    private bool hasDealtRushDamage = false; // 돌진 데미지를 한 번만 주기 위한 플래그
    public bool IsRushing => isRushing;
    
    // 접촉 데미지 쿨다운
    private float lastContactDamageTime = 0f;

    public SkinnedMeshRenderer skinnedMeshRenderer;
    public int blendShapeValue = 0;
    
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

    protected override void Update()
    {
        base.Update();
        if(skinnedMeshRenderer != null)
        {
            SetBlendShapeByIndex(0, blendShapeValue);
        }
    }
        void SetBlendShapeByIndex(int index, float value)
    {
        if (skinnedMeshRenderer != null)
        {
            // value 범위: 0 ~ 100
            skinnedMeshRenderer.SetBlendShapeWeight(index, value);
        }
    }


    protected override void Start()
    {
        base.Start();
        
        // Cube Vision 설정 (360도 감지)
        vision.enableProximityDetection = true;
        vision.proximityDetectionRange = 5f;  // 공격 범위
        vision.sightRange = 15f;               // 추적 범위
        vision.sightAngle = 360f;              // 전방향 감지
        
        Debug.Log($"Cube Vision 설정 - 공격 범위: {AttackRadius}m, 추적 범위: {ViewRadius}m, 공격 포기 범위: {AttackGiveUpRadius}m");
        
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
    /// 플레이어와 충돌 시 호출 (돌진 데미지 - 한 번만)
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(ETag.Player.ToString()))
        {
            // 돌진 중이고 아직 돌진 데미지를 주지 않았다면
            if (isRushing && !hasDealtRushDamage)
            {
                IDamageable damageable = collision.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(RushDamage, DamageType.Physical);
                    hasDealtRushDamage = true; // 한 번만 데미지
                    Debug.Log($"Cube: 플레이어에게 돌진 데미지 {RushDamage} 적용!");
                    
                    // 사운드 재생 (필요시)
                    // AudioManager.instance.PlayOneShot(FMODEvents.instance.cubeRushHit, transform, "Cube 돌진 충돌");
                }
            }
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