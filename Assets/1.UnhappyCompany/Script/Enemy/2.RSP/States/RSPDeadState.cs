using System.Collections;
using UnityEngine;

/// <summary>
/// RSP 적의 죽음 상태를 관리하는 클래스입니다.
/// HP(코인)가 0이 되면 이 상태로 전환되며, 죽음 처리를 담당합니다.
/// </summary>
public class RSPDeadState : IState
{
    private EnemyAIRSP controller;
    private bool deathProcessStarted = false;

    public RSPDeadState(EnemyAIRSP controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("=== RSP: 죽음 상태 진입 ===");
        
        // NavMeshAgent 완전 정지
        if (controller.agent != null && controller.agent.enabled)
        {
            controller.agent.isStopped = true;
            controller.agent.ResetPath();
            controller.agent.velocity = Vector3.zero;
        }
        
        // Speed 파라미터를 0으로 설정 (Blend Tree에서 Idle로)
        controller.SetSpeed(0f);
        
        // Defeated 애니메이션으로 전이 (CrossFade)
        if (controller.animator != null)
        {
            controller.PlayAnimation(controller.DefeatedStateName, 0.3f);
        }
        
        // 죽음 처리 시작
        if (!deathProcessStarted)
        {
            deathProcessStarted = true;
            controller.StartCoroutine(HandleDeathProcess());
        }
    }

    /// <summary>
    /// 죽음 처리 코루틴 (State 내부에서 처리)
    /// </summary>
    private IEnumerator HandleDeathProcess()
    {
        // 1. UI에 죽음 상태 표시
        if (controller.rspSystem != null && controller.rspSystem.rspUI != null)
        {
            controller.rspSystem.rspUI.ShowDeadState();
        }
        Debug.Log("RSP: UI에 죽음 상태 표시");
        
        // 2. 죽음 사운드 재생
        PlayDeathSound();
        
        // 3. 죽음 이펙트 생성
        SpawnDeathEffect();
        
        // 4. 죽음 애니메이션 대기 시간
        float deathAnimationDuration = controller.GetDeathAnimationDuration();
        Debug.Log($"RSP: 죽음 애니메이션 재생 중... ({deathAnimationDuration}초)");
        yield return new WaitForSeconds(deathAnimationDuration);
        
        // 5. 선물상자 생성
        SpawnGiftBox();
        
        // 6. 지정된 오브젝트들 비활성화
        DisableObjectsOnDeath();
        
        // 7. 지정된 오브젝트 활성화
        EnableObjectsOnDeath();
        
        // 8. 에미션 끄기
        controller.DisableEmission();
        
        Debug.Log("=== RSP: 죽음 처리 완료 ===");
        
        // 9. 3초 대기 후 GameObject 삭제
        yield return new WaitForSeconds(0.1f);
        Debug.Log("RSP: GameObject 삭제 시작");
        Object.Destroy(controller.gameObject);
    }
    
    private void PlayDeathSound()
    {
        if (AudioManager.instance != null && FMODEvents.instance != null)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspLose, controller.transform, 40f, "RSP Death");
            Debug.Log("RSP: 죽음 사운드 재생");
        }
    }
    
    private void SpawnDeathEffect()
    {
        GameObject deathEffectPrefab = controller.GetDeathEffectPrefab();
        if (deathEffectPrefab != null)
        {
            GameObject effect = Object.Instantiate(deathEffectPrefab, controller.transform.position, Quaternion.identity);
            Debug.Log("RSP: 죽음 이펙트 생성");
            
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Object.Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }
    }
    
    private void SpawnGiftBox()
    {
        ItemData giftBoxItemData = controller.GetGiftBoxItemData();
        if (giftBoxItemData == null)
        {
            Debug.LogWarning("RSP: 선물상자 ItemData가 설정되지 않았습니다!");
            return;
        }
        
        Transform giftBoxSpawnTransform = controller.GetGiftBoxSpawnTransform();
        Vector3 spawnPos = giftBoxSpawnTransform != null 
            ? giftBoxSpawnTransform.position 
            : controller.transform.position + Vector3.up * 1.5f;
        
        GameObject giftBox = Object.Instantiate(giftBoxItemData.prefab, spawnPos, Quaternion.identity);
        
        Debug.Log($"RSP: 선물상자 생성 완료! 위치: {spawnPos}");
        
        if (AudioManager.instance != null && FMODEvents.instance != null)
        {
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rspWin, controller.transform, 40f, "Gift Box Spawn");
        }
    }
    
    private void DisableObjectsOnDeath()
    {
        var objectsToDisable = controller.GetObjectsToDisableOnDeath();
        if (objectsToDisable != null && objectsToDisable.Count > 0)
        {
            Debug.Log($"RSP: {objectsToDisable.Count}개의 오브젝트 비활성화");
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
    
    private void EnableObjectsOnDeath()
    {
        var objectToEnable = controller.GetObjectsToEnableOnDeath();
        if (objectToEnable != null)
        {
            Debug.Log($"RSP: 오브젝트 활성화 - {objectToEnable.name}");
            objectToEnable.SetActive(true);
        }
    }

    public void ExecuteMorning()
    {
        // 죽음 상태에서는 아무것도 하지 않음 (영구 상태)
    }

    public void ExecuteAfternoon()
    {
        // 죽음 상태에서는 아무것도 하지 않음 (영구 상태)
    }

    public void Exit()
    {
        Debug.Log("RSP: 죽음 상태 종료 (부활?)");
        // 죽음 상태는 일반적으로 종료되지 않음
    }

    public void ExecuteFixedMorning()
    {
        // 물리 업데이트 로직 없음
    }

    public void ExecuteFixedAfternoon()
    {
        // 물리 업데이트 로직 없음
    }
}

