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
    [SerializeField] private bool isShowDebug = false;
    // 점액 배출 간격
    public float slimeEmitInterval = 10f;
    private float lastSlimeEmitTime;
    public Animator animator;
    [Header("Wall Check")]
    public float wallCheckDistance = 10f;

    // 애니메이션 이름 상수
    public readonly string idleAnimationName = "Moo_Idle";
    public readonly string walkAnimationName = "Moo_Walk";
    public readonly string hitReactionAnimationName = "Moo_HitReaction";
    public readonly string bumpAnimationName = "Moo_Bump";
    public readonly string cryAnimationName = "Moo_Cry";
    [ReadOnly] [SerializeField] private float animatorSpeed = 2f;

    // 이동 및 스트레스 관련 변수
    public float moveSpeed = 2f;
    public float stressThreshold = 5f;
    public float stress = 0f;
    public float stressIncreaseAmount = 1f;
    public float stressResetDistance = 10f;
    private Vector3 moveDirection;
    private bool isStressed = false;
    private bool isBumping = false; // 부딪힘 상태 플래그

    // 데미지 관련 변수
    public int damageAmount = 10; // 플레이어에게 줄 데미지 양

    public CharacterController characterController;

    /// <summary>
    /// 초기화 메서드입니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        ChangeState(new MooIdleState(this));
        lastSlimeEmitTime = Time.time;
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // 초기에는 NavMeshAgent를 사용하지 않음
        agent.speed = moveSpeed;
        ChangeDirection();
        if(characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        StartCoroutine(SlimeEmitRoutine());
        StartCoroutine(MovementRoutine());
    }

    /// <summary>
    /// 매 프레임 호출되는 업데이트 메서드입니다.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        UpdateAnimatorSpeed();
        CurrentStateName = currentState.GetType().Name;
    }

    // private void AfternoonUpdate()
    // {
    //     if(TimeManager.instance.IsDay)
    //     {
    //         ChangeState(new MooAfternoonState(this));
    //     }
    // }

    /// <summary>
    /// 애니메이터의 Speed 파라미터를 업데이트합니다.
    /// </summary>
    private void UpdateAnimatorSpeed()
    {
        DebugManager.Log($"characterController.velocity.magnitude : {characterController.velocity.magnitude}, moveSpeed : {moveSpeed}", isShowDebug);
        animatorSpeed = characterController.velocity.magnitude / moveSpeed;
        animator.SetFloat("Speed", animatorSpeed);
    }

    /// <summary>
    /// 데미지를 받을 때 호출되는 메서드입니다.
    /// </summary>
    public override void TakeDamage(int damage, DamageType damageType)
    {
        base.TakeDamage(damage, damageType);
        PlayAnimation(hitReactionAnimationName);
        ChangeState(new MooFleeState(this));
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
    /// 스트레스 상태로 전환합니다.
    /// </summary>
    private void EnterStressState()
    {
        isStressed = true;
        agent.enabled = true;
        Vector3 randomDirection = Random.insideUnitSphere * stressResetDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, stressResetDistance, 1))
        {
            agent.SetDestination(hit.position);
        }
        PlayAnimation(cryAnimationName);
        // 울음 사운드 재생
    }

    /// <summary>
    /// 스트레스 상태를 종료합니다.
    /// </summary>
    private void ExitStressState()
    {
        isStressed = false;
        agent.enabled = false;
        stress = 0f;
        ChangeDirection();
        // 울음 애니메이션 및 사운드 중지
    }

    /// <summary>
    /// 무작위 방향으로 이동 방향을 변경합니다.
    /// </summary>
    private void ChangeDirection()
    {
        moveDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }

    /// <summary>
    /// 벽에 부딪혔을 때 처리하는 코루틴입니다.
    /// </summary>
    private IEnumerator HandleBump()
    {
        isBumping = true;
        PlayAnimation(bumpAnimationName);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isBumping = false;
    }

    /// <summary>
    /// 이동 방향으로 회전합니다.
    /// </summary>
    private void RotateTowardsMoveDirection()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
        }
    }

    /// <summary>
    /// 플레이어와 충돌 시 데미지를 줍니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DebugManager.Log("MooAIController OnCollisionEnter", isShowDebug);
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                DealDamage(damageAmount, player as IDamageable);
            }
        }
    }

    

    /// <summary>
    /// 플레이어에게 데미지를 줍니다.
    /// </summary>
    public override void DealDamage(int damage, IDamageable target)
    {
        target.TakeDamage(damage, DamageType.Physical);
    }

    // 새로운 코루틴: 점액 배출 처리
    private IEnumerator SlimeEmitRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(slimeEmitInterval);
            ChangeState(new MooSlimeEmitState(this));
        }
    }

    // 새로운 코루틴: 이동 및 스트레스 처리
    private IEnumerator MovementRoutine()
    {
        while (true)
        {
            if (!isStressed && !isBumping)
            {
                characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

                // 이동 방향으로 회전
                RotateTowardsMoveDirection();

                // 벽에 부딪혔을 때 스트레스 증가
                RaycastHit hit;
                if (Physics.Raycast(transform.position, moveDirection, out hit, wallCheckDistance))
                {
                    if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                    {
                        stress += stressIncreaseAmount;
                        moveDirection = Vector3.Reflect(moveDirection, hit.normal);
                        StartCoroutine(HandleBump());
                    }
                }

                // 레이캐스트를 시각적으로 표현
                Debug.DrawRay(transform.position, moveDirection * wallCheckDistance, Color.red, 0.1f);

                // 스트레스 임계치 도달 시 스트레스 상태로 전환
                if (stress >= stressThreshold)
                {
                    EnterStressState();
                }
            }
            else if (isStressed)
            {
                // 스트레스 상태에서 목적지에 도달하면 스트레스 상태 종료
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    ExitStressState();
                }
            }
            yield return null;
        }
    }
} 