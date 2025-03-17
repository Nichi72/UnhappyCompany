using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 렘페이지(Rampage) AI 컨트롤러.
/// 돌진, 패널 노출, 자폭 등 사양서에서 요구된 로직 관리.
/// </summary>
public class RampageAIController : EnemyAIController<RampageAIData>
{
    // 내부적으로 사용할 현재 HP
    private int currentHP;
    // 패널 체력(상태마다 갱신)
    public int currentPanelHealth { get; set; }

    [Header("Charge State")]
    public bool isCollided = false;  // 돌진 중 충돌 발생 여부
    private int chargeCount = 0;     // 돌진 횟수
    private int maxChargeCount = 3;  // 최대 돌진 횟수

    // 쿠션 충돌 시 노출될 패널 수 / 쿠션 없이 충돌 시 노출될 패널 수
    private int cushionPanelCount => enemyData.cushionPanelCount;
    private int noCushionPanelCount => enemyData.noCushionPanelCount;
    // 쿠션 없이 충돌 시 HP 감소
    private int hpLossOnNoCushion => enemyData.hpLossOnNoCushion;

    // 사양서에서 요구한 폭발 범위와 대미지
    public float ExplodeRadius => enemyData.explodeRadius;
    public int ExplodeDamage => enemyData.explodeDamage;

    // LineRenderer for visualizing detection
    [SerializeField] private LineRenderer lineRenderer;

    protected override void Start()
    {
        base.Start();
        // 초기 HP 세팅
        currentHP = enemyData.maxHP;
        // 초기 상태는 Idle이라고 가정
        ChangeState(new RampageIdleState(this));

        // LineRenderer 초기화
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
    }

    protected override void Update()
    {
        base.Update();
        UpdateLineRenderer();
    }

    /// <summary>
    /// LineRenderer를 사용하여 감지 범위 시각화
    /// </summary>
    public void UpdateLineRenderer()
    {
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -enemyData.detectAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, enemyData.detectAngle / 2, 0) * forward;

        Vector3[] positions = new Vector3[]
        {
            transform.position,
            transform.position + leftBoundary * enemyData.detectRange,
            transform.position + rightBoundary * enemyData.detectRange,
            transform.position
        };

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);

        // 플레이어 감지 여부에 따라 색상 변경
        if (CheckPlayerDetected())
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
        else
        {
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
        }
    }

    /// <summary>
    /// LineRenderer를 비활성화
    /// </summary>
    public void DisableLineRenderer()
    {
        lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// 플레이어 감지 여부 확인
    /// </summary>
    private bool CheckPlayerDetected()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, toPlayer);

        return distance <= enemyData.detectRange && angle <= enemyData.detectAngle;
    }

    /// <summary>
    /// HP 감소 처리.
    /// HP가 0 이하가 되면 폭발(Explode) 상태로 전환하거나, 패널 노출 여부를 확인
    /// </summary>
    public override void TakeDamage(int damage, DamageType damageType)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            // HP가 0 이하로 떨어졌다면 자폭으로 전환
            ChangeState(new RampageExplodeState(this));
        }
        else
        {
            // 패널이 열려있다면 panelHealth에 영향이 있을 수도 있으니
            // RampagePanelOpenState에서 takeDamage를 확인하도록 구성해도 됩니다.
            // 상황에 따라 원하는 로직 추가
        }
    }

    /// <summary>
    /// HP 직접 감소 (패널 노출 없이 벽 충돌 시 사용)
    /// </summary>
    public void ReduceHP(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            ChangeState(new RampageExplodeState(this));
        }
    }

    /// <summary>
    /// 현재 렘페이지의 HP 가져오기(디버깅용)
    /// </summary>
    public int GetCurrentHP()
    {
        return currentHP;
    }

    /// <summary>
    /// 패널 Health 초기화
    /// </summary>
    public void ResetPanelHealth(int panelCount)
    {
        currentPanelHealth = enemyData.panelHealth;
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

    /// <summary>
    /// 물리 충돌 시 쿠션 여부 판단 후 상태 전환
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 현재 상태가 Charge 상태인지 확인
        if (currentState is RampageChargeState chargeState)
        {
            Debug.Log("Rampage: Charge 상태에서 충돌 발생");
            if (isCollided) return;
            isCollided = true;

            bool hasCushion = IsCushionAtCollision(collision);
            int panelCount = hasCushion 
                ? enemyData.cushionPanelCount
                : enemyData.noCushionPanelCount;

            if (!hasCushion)
            {
                // HP 감소
                ReduceHP(enemyData.hpLossOnNoCushion);
            }

            // 플레이어가 여전히 순찰 범위 내에 있는지 확인
            if (chargeState.CheckPlayerInPatrolRange())
            {
                // Charge 상태로 재진입
                ChangeState(new RampageChargeState(this));
            }
            else
            {
                // 돌진 횟수 증가 및 상태 전환
                IncrementChargeCount();
            }
        }
    }

    /// <summary>
    /// 돌진 횟수 증가 및 상태 전환 로직
    /// </summary>
    public void IncrementChargeCount()
    {
        chargeCount++;
        if (chargeCount >= maxChargeCount)
        {
            chargeCount = 0; // 돌진 횟수 초기화
            ChangeState(new RampageIdleState(this)); // Idle 상태로 전환
        }
        else
        {
            ChangeState(new RampagePatrolState(this)); // Patrol 상태로 전환
        }
    }
} 