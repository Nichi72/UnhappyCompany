using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 렘페이지(Rampage) AI 컨트롤러.
/// 돌진, 패널 노출, 자폭 등 사양서에서 요구된 로직 관리.
/// </summary>
public class RampageAIController : EnemyAIController<RampageAIData>
{
    [Header("DEBUG")]
    public string currentStateName;
    

    [Header("Charge State")]
    public bool isCollided = false;      // 돌진 중 충돌 발생 여부
    public int chargeCount = 0;         // 돌진 횟수
    private int consecutiveCollisions = 0;// 연속 충돌 카운트
    private const int MAX_CONSECUTIVE_COLLISIONS = 10; // 최대 연속 충돌 횟수
    [Header("Panel State")]
    public int currentPanelHealth { get; set; }
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
    public List<RampagePanel> panels;

    protected override void Start()
    {
        base.Start();
        // 초기 HP 세팅
        Hp = enemyData.maxHP;
        currentPanelHealth = enemyData.maxPanelHealth;
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

    }

    protected override void Update()
    {
        base.Update();
        UpdateLineRenderer();
        currentStateName = currentState.GetType().Name;
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
        Hp -= damage;
        if (Hp <= 0)
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
        Hp -= amount;
        if (Hp <= 0)
        {
            ChangeState(new RampageExplodeState(this));
        }
    }
    /// <summary>
    /// 패널 Health 초기화
    /// </summary>
    public void ResetPanelHealth(int panelCount)
    {
        currentPanelHealth = enemyData.maxPanelHealth;
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
        
    }
    public bool onceReduceHP = true; // 충돌시 false로 바꾸고 ChargeCoroutine에서 충돌전 회전할때 true로 만듬.
    void OnCollisionStay(Collision collision)
    {
        // 현재 상태가 Charge 상태인지 확인
        if (currentState is RampageChargeState chargeState)
        {
            bool hasCushion = IsCushionAtCollision(collision);
            // int panelCount = hasCushion
            //     ? enemyData.cushionPanelCount
            //     : enemyData.noCushionPanelCount;

            if (!hasCushion && onceReduceHP) 
            {
                // HP 감소
                ReduceHP(enemyData.hpLossOnNoCushion);
                onceReduceHP = false;
            }


            Debug.Log("Rampage: Charge 상태에서 충돌 발생 collision " + collision.gameObject.tag + " ETag " + ETag.Wall.ToString());
            if (collision.collider.CompareTag(ETag.Wall.ToString()))
            {
                isCollided = true;
                Debug.Log("충돌 발생");
            }
        }
    }

    /// <summary>
    /// 벽에 끼었을 때 처리하는 함수
    /// </summary>
    private void HandleStuck()
    {
        Debug.Log("벽에 끼었습니다! 상태를 초기화합니다.");
        // 현재 위치에서 뒤로 약간 이동
        // transform.position -= transform.forward * 2f;
        // Idle 상태로 강제 전환
        ChangeState(new RampageIdleState(this,"HandleStuck에서 실행"));
        // 충돌 카운트 초기화
        chargeCount = 0;
    }

    private void InitPannel()
    {
        foreach (var panel in panels)
        {
            panel.gameObject.SetActive(false);
        }
    }

    #region == Debug ==
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
    #endregion
}