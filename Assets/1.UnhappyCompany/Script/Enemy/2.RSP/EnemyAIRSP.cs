using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

/// <summary>
/// RSP 타입 적의 AI 컨트롤러입니다.
/// </summary>
public class EnemyAIRSP : EnemyAIController<RSPEnemyAIData> ,IInteractable
{
    // RSP 전용 프로퍼티
    public float AttackCooldown => enemyData.attackCooldown;
    public float SpecialAttackCooldown => enemyData.specialAttackCooldown;
    public float SpecialAttackRange => enemyData.specialAttackRange;

    private string interactionText = "가위바위보 하기";
    public string InteractionText { get => interactionText; set => interactionText = value; }
    public RSPSystem rspSystem;
    public Animator animator;

    [SerializeField] private int compulsoryPlayStack = 0; // 반드시 플레이 해야하는 스택
    private Stack<EventInstance> soundInstances = new Stack<EventInstance>();

    public bool isAnimationEnd = false;

    public bool isPlayerFound = false;

    public readonly string WinAnimationName = "RSP_Win";
    public readonly string LoseAnimationName = "RSP_Lose";
    public readonly string DrawAnimationName = "RSP_Draw";
    public readonly string IdleAnimationName = "RSP_Idle";
    public readonly string HoldingAnimationName = "RSP_Stop";

    protected override void Start()
    {
        base.Start();
        // RSP 특화 초기화
        rspSystem = GetComponent<RSPSystem>();
        Player player = GameManager.instance.currentPlayer;
        ChangeState(new RSPPatrolState(this, utilityCalculator, 60f));
        // ChangeState(new RSPHoldingState(this, utilityCalculator, player, rspSystem));
        
        // rspStack 증가 코루틴 시작
        StartCoroutine(IncrementCompulsoryPlayStack());
        StartCoroutine(CheckCompulsoryPlayStack());
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
            if (compulsoryPlayStack >= 4)
            {
                ChangeState(new RSPRageState(this, utilityCalculator, GameManager.instance.GetNearestPlayer(transform)));
                Debug.Log("RSP: 추적 상태 (광란) 상태로 전환");
                break;
            }
            yield return null;
        }
    }

    public void IncrementStack()
    {
        compulsoryPlayStack++;
        PlaySoundBasedOnCompulsoryPlayStack(compulsoryPlayStack);
    }

    public void DecrementStack()
    {
        if (compulsoryPlayStack > 0)
        {
            compulsoryPlayStack--;
            if (soundInstances.Count > 0)
            {
                EventInstance lastInstance = soundInstances.Pop();
                if (lastInstance.isValid())
                {
                    Debug.Log("RSP: 음악 중지");
                    lastInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    lastInstance.release();
                }
            }
        }
    }

    public bool IsInRageState()
    {
        return currentState is RSPRageState;
    }

    public int GetCompulsoryPlayStack()
    {
        return compulsoryPlayStack;
    }

    #region Audio

    public void PlaySoundBasedOnCompulsoryPlayStack(int index)
    {
        if (compulsoryPlayStack == 0)
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
        compulsoryPlayStack = 4;
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
}