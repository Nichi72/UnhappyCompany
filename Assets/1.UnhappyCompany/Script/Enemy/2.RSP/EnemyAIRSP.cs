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

    [SerializeField] private float coolDownTime = 30f;

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
            Invoke(nameof(ResetCoolDown), coolDownTime);
        };
    }

    private void ResetCoolDown()
    {
        isCoolDown = false;
        Debug.Log("RSP: 쿨타임 초기화");
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
            Debug.Log("RSP: 지면에 있지 않음");
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
}