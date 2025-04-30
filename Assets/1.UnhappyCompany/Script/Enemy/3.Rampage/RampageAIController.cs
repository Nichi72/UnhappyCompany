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
    private Rigidbody rb;

    public new RampageAIData EnemyData => enemyData;

    [Header("Charge State")]
    public bool isCollided = false;       // 돌진 중 충돌 발생 여부
    public int chargeCount = 0;           // 돌진 횟수
    private int consecutiveCollisions = 0;// 연속 충돌 카운트
    private const int MAX_CONSECUTIVE_COLLISIONS = 10; // 최대 연속 충돌 횟수
    [Header("Panel State")]
    [SerializeField] private int currentPanelHealth;
    public int CurrentPanelHealth { get => currentPanelHealth; set => currentPanelHealth = value; }
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

    private float stuckTime = 0f;
    private Vector3 lastPosition;
    private float stuckThreshold = 0.1f; // 낑김으로 판단할 최소 이동 거리
    private float maxStuckTime = 3f; // 최대 낑김 허용 시간

    public bool onceReduceHP = true; // 충돌시 false로 바꾸고 ChargeCoroutine에서 충돌전 회전할때 true로 만듬.

    protected override void Start()
    {
        base.Start();
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
    }

    protected override void Update()
    {
        base.Update();
        
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
        AudioManager.instance.PlayTestBeep("Rampage_Hit 하지만 데미지 안받아서 둔탁한 소리가 나야함.",transform);
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
    /// <summary>
    /// 물리 충돌 시 쿠션 여부 판단 후 상태 전환
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        /*
        if (currentState is RampageChargeState chargeState)
        {
            if(collision.collider.CompareTag(ETag.Pushable.ToString()))
            {
                Push(collision);
                Debug.Log("Pushable 충돌 발생");
                AudioManager.instance.PlayTestBeep("Pushable 충돌하는 소리",transform);
            }
        }
        */
    }
    
    void OnCollisionStay(Collision collision)
    {
        /*
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


            Debug.Log("Rampage: Charge 상태에서 충돌 발생 collision " + collision.gameObject.tag + " ETag " + collision.gameObject.name);
           
            if (collision.collider.CompareTag(ETag.Wall.ToString()))
            {
                isCollided = true;
                Debug.Log("충돌 발생");
                AudioManager.instance.PlayTestBeep("Rampage 벽에 처박히는 소리",transform);
            }

            if(collision.collider.CompareTag(ETag.Player.ToString()))
            {
                Debug.Log("플레이어와 충돌 발생");
                isCollided = true;
                AudioManager.instance.PlayTestBeep("Rampage 플레이어와 충돌 소리",transform);
                collision.transform.GetComponent<IDamageable>().TakeDamage(50,DamageType.Nomal);
                // Push(collision);

            }
            if(collision.collider.CompareTag(ETag.Cushion.ToString()))
            {
                Debug.Log("쿠션과 충돌 발생");
                isCollided = true;
                AudioManager.instance.PlayTestBeep("Rampage 쿠션과 충돌 소리",transform);
            }
        }
        */
    }

    private float pushStrength = 10f;    
    private void Push(Collision collision)
    {
        /*
        Rigidbody otherRb = collision.rigidbody;

        if (otherRb == null)
        {
            // Rigidbody가 없으면 추가
            otherRb = collision.gameObject.AddComponent<Rigidbody>();
            otherRb.mass = 1f;              // 기본 Mass
            otherRb.linearDamping = 0f;               // 기본 Drag
            otherRb.angularDamping = 0.05f;     // 기본 Angular Drag
            otherRb.interpolation = RigidbodyInterpolation.Interpolate; // 부드럽게
            otherRb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 충돌 튐 방지
        }

        if (otherRb != null && !otherRb.isKinematic)
        {
            Vector3 pushDir = collision.transform.position - transform.position;
            pushDir.y = 0;
            pushDir.Normalize();

            otherRb.AddForce(pushDir * pushStrength, ForceMode.Impulse);
        }
        */
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
}