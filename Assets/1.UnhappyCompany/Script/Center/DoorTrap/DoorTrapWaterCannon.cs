using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorTrapWaterCannon : MonoBehaviour
{
    [Header("남은 발사 횟수")]
    public int count = 3;  // 남은 발사 횟수

    [Header("파티클 시스템")]
    public ParticleSystem waterMainParticle;
    public ParticleSystem waterMistParticle;
    public ParticleSystem waterDropletParticle;
    public ParticleSystem waterSubAParticle;
    public ParticleSystem waterSubBParticle;  // 물줄기 효과를 담당하는 파티클


    [Header("콜라이더")]
    public Collider trapCollider;  // 트랩 콜라이더

    [Header("파티클 최대 발생 속도")]
    public float maxEmissionRate = 100f;

    [Header("파티클 상태")]
    private bool isParticleActive = false;
    private bool isOnProcessing = false;  // On() 함수가 실행 중인지 확인하는 플래그
    private Coroutine autoOffCoroutine;   // 자동 끄기 코루틴 참조

    // 외부에서 On() 함수 실행 상태를 확인할 수 있도록 하는 프로퍼티
    public bool IsOnProcessing => isOnProcessing;

    // 남은 발사 횟수를 확인할 수 있는 프로퍼티
    public int RemainingCount => count;

    private void Start()
    {
        // 세이브 시스템에 의거하여 이후에 새롭게 다시 초기화 해야함.
        ResetCount(0);
    }

    // 발사 횟수를 리셋하는 메서드
    public void ResetCount(int newCount = 2)
    {
        count = newCount;
        Debug.Log($"물 대포 트랩 발사 횟수가 {newCount}회로 리셋되었습니다.");
    }

    private void Awake()
    {
        waterMainParticle.Stop();
        waterSubAParticle.Stop();
        waterMistParticle.Stop();
        waterDropletParticle.Stop();
        waterSubBParticle.Stop();
        
        // 콜라이더가 설정되지 않았다면 자동으로 찾기
        if (trapCollider == null)
        {
            trapCollider = GetComponent<Collider>();
        }
        
        // 초기에는 콜라이더 비활성화
        if (trapCollider != null)
        {
            trapCollider.enabled = false;
        }
        
        ResetCount();
    }

    private void Update()
    {
       
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(ETag.Enemy.ToString()))
        {
            var enemy = other.gameObject.GetComponent<EnemyAIController>();
            if(enemy != null)
            {
                enemy.TakeDamage(100, DamageType.Water);
            }
        }
        else if(other.CompareTag(ETag.Player.ToString()))
        {
            var player = other.gameObject.GetComponent<Player>();
            if(player != null)
            {
                player.playerStatus.TakeDamage(100, DamageType.Water);
            }
        }
    }


    public void On()
    {
        // 이미 On() 함수가 실행 중이면 리턴
        if (isOnProcessing)
        {
            Debug.Log("물 대포 트랩이 이미 실행 중입니다.");
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.trapWaterCannonAlreadyActive, waterMainParticle.gameObject.transform, 20f, "물 대포 트랩이 이미 실행 중입니다.");
            return;
        }

        // 발사 횟수 제한 체크 (count가 0이면 무제한)
        if (count <= 0)
        {
            Debug.Log("물 대포 트랩 발사 횟수를 초과했습니다.");
            AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.trapWaterCannonLimitExceeded, waterMainParticle.gameObject.transform, 20f, "물 대포 트랩 발사 횟수를 초과했습니다.");
            return;
        }

        isOnProcessing = true;
        count--;  // 사용 횟수 감소

        Debug.Log($"물 대포 트랩 발사! (남은 발사 횟수: {count})");

        // 기존 자동 끄기 코루틴이 있다면 중지
        if (autoOffCoroutine != null)
        {
            StopCoroutine(autoOffCoroutine);
        }
        
        // 콜라이더 활성화
        if (trapCollider != null)
        {
            trapCollider.enabled = true;
        }
        
        ParticleControl();
        // 2초 후 자동으로 끄기
        autoOffCoroutine = StartCoroutine(AutoOffAfterDelay(2f));
        AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.trapWaterCannonFire, waterMainParticle.gameObject.transform, 40f, "강력한 물 대포 소리. 고압으로 분사되는 물줄기 소리. 촤아아악 하는 물 분사음과 튕기는 물 소리. 꺼질때까지 진행되어야함.");
        
        void ParticleControl()
        {
             // 모든 파티클 재생
            waterMainParticle.Play();
            waterSubAParticle.Play();
            waterMistParticle.Play();
            waterDropletParticle.Play();
            waterSubBParticle.Play();

            // waterMainParticle emission rate 설정
            var emission1 = waterMainParticle.emission;
            var rate1 = emission1.rateOverTime;
            rate1.constant = maxEmissionRate;
            emission1.rateOverTime = rate1;

            // waterSplashParticle emission rate 설정
            var emission2 = waterSubAParticle.emission;
            var rate2 = emission2.rateOverTime;
            rate2.constant = maxEmissionRate;
            emission2.rateOverTime = rate2;

            // waterMistParticle emission rate 설정
            var emission3 = waterMistParticle.emission;
            var rate3 = emission3.rateOverTime;
            rate3.constant = 200f;
            emission3.rateOverTime = rate3;

            // waterDropletParticle emission rate 설정
            var emission4 = waterDropletParticle.emission;
            var rate4 = emission4.rateOverTime;
            rate4.constant = 100f;
            emission4.rateOverTime = rate4;

            // waterStreamParticle emission rate 설정
            var emission5 = waterSubBParticle.emission;
            var rate5 = emission5.rateOverTime;
            rate5.constant = maxEmissionRate;
            emission5.rateOverTime = rate5;
        }
    }
    
    private IEnumerator AutoOffAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Off();
        isOnProcessing = false;  // On() 함수 실행 완료
        autoOffCoroutine = null;
    }
    
    private void Off()
    {
        // 콜라이더 비활성화
        if (trapCollider != null)
        {
            trapCollider.enabled = false;
        }
        
        // waterMainParticle emission rate를 0으로 설정
        var emission1 = waterMainParticle.emission;
        var rate1 = emission1.rateOverTime;
        rate1.constant = 0;
        emission1.rateOverTime = rate1;

        // waterSplashParticle emission rate를 0으로 설정
        var emission2 = waterSubAParticle.emission;
        var rate2 = emission2.rateOverTime;
        rate2.constant = 0;
        emission2.rateOverTime = rate2;

        // waterMistParticle emission rate를 0으로 설정
        var emission3 = waterMistParticle.emission;
        var rate3 = emission3.rateOverTime;
        rate3.constant = 0;
        emission3.rateOverTime = rate3;

        // waterDropletParticle emission rate를 0으로 설정
        var emission4 = waterDropletParticle.emission;
        var rate4 = emission4.rateOverTime;
        rate4.constant = 0;
        emission4.rateOverTime = rate4;

        // waterStreamParticle emission rate를 0으로 설정
        var emission5 = waterSubBParticle.emission;
        var rate5 = emission5.rateOverTime;
        rate5.constant = 0;
        emission5.rateOverTime = rate5;
    }
}
