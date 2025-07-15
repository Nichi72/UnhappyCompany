using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DoorTrapFire : MonoBehaviour
{
    [Header("남은 발사 횟수")]
    public int count = 2;  // 남은 발사 횟수

    [Header("파티클 시스템")]
    public ParticleSystem fireMainParticle;
    public ParticleSystem fireSub1Particle;
    public ParticleSystem fireSub2Particle;
    public ParticleSystem fireSmokeParticle;
    public ParticleSystem fireLightParticle;  // 불빛 효과를 담당하는 파티클

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
        Debug.Log($"불 트랩 발사 횟수가 {newCount}회로 리셋되었습니다.");
    }

    private void Awake()
    {
        fireMainParticle.Stop();
        fireSub1Particle.Stop();
        fireSub2Particle.Stop();
        fireSmokeParticle.Stop();
        fireLightParticle.Stop();
        
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
                enemy.TakeDamage(100, DamageType.Fire);
            }
        }
        else if(other.CompareTag(ETag.Player.ToString()))
        {
            var player = other.gameObject.GetComponent<Player>();
            if(player != null)
            {
                player.playerStatus.TakeDamage(100, DamageType.Fire);
            }
        }
    }


    public void On()
    {
        // 이미 On() 함수가 실행 중이면 리턴
        if (isOnProcessing)
        {
            Debug.Log("불 트랩이 이미 실행 중입니다.");
            AudioManager.instance.PlayTestBeep("불 트랩이 이미 실행 중입니다.", fireMainParticle.gameObject.transform);
            return;
        }

        // 발사 횟수 제한 체크 (count가 0이면 무제한)
        if (count <= 0)
        {
            Debug.Log("불 트랩 발사 횟수를 초과했습니다."); 
            AudioManager.instance.PlayTestBeep("불 트랩 발사 횟수를 초과했습니다.", fireMainParticle.gameObject.transform);
            return;
        }

        isOnProcessing = true;
        count--;  // 사용 횟수 감소

        Debug.Log($"불 트랩 발사! (남은 발사 횟수: {count})");

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
        AudioManager.instance.PlayTestBeep("불타는 소리 시작. 엄청 화르륵 화력쌘 개무서운 불 소리. 그냥 이 불에 스치면 뒤질거같은 그런 느낌. 꺼질때까지 진행되어야함." , fireMainParticle.gameObject.transform);
        void ParticleControl()
        {
             // 모든 파티클 재생
            fireMainParticle.Play();
            fireSub1Particle.Play();
            fireSub2Particle.Play();
            fireSmokeParticle.Play();
            fireLightParticle.Play();

            // fireMainParticle emission rate 설정
            var emission1 = fireMainParticle.emission;
            var rate1 = emission1.rateOverTime;
            rate1.constant = maxEmissionRate;
            emission1.rateOverTime = rate1;

            // fireSub1Particle emission rate 설정
            var emission2 = fireSub1Particle.emission;
            var rate2 = emission2.rateOverTime;
            rate2.constant = maxEmissionRate;
            emission2.rateOverTime = rate2;

            // fireSub2Particle emission rate 설정
            var emission3 = fireSub2Particle.emission;
            var rate3 = emission3.rateOverTime;
            rate3.constant = maxEmissionRate;
            emission3.rateOverTime = rate3;

            // fireSmokeParticle emission rate 설정
            var emission4 = fireSmokeParticle.emission;
            var rate4 = emission4.rateOverTime;
            rate4.constant = maxEmissionRate;
            emission4.rateOverTime = rate4;

            // fireLightParticle emission rate 설정
            var emission5 = fireLightParticle.emission;
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
        
        // fireMainParticle emission rate를 0으로 설정
        var emission1 = fireMainParticle.emission;
        var rate1 = emission1.rateOverTime;
        rate1.constant = 0;
        emission1.rateOverTime = rate1;

        // fireSub1Particle emission rate를 0으로 설정
        var emission2 = fireSub1Particle.emission;
        var rate2 = emission2.rateOverTime;
        rate2.constant = 0;
        emission2.rateOverTime = rate2;

        // fireSub2Particle emission rate를 0으로 설정
        var emission3 = fireSub2Particle.emission;
        var rate3 = emission3.rateOverTime;
        rate3.constant = 0;
        emission3.rateOverTime = rate3;

        // fireSmokeParticle emission rate를 0으로 설정
        var emission4 = fireSmokeParticle.emission;
        var rate4 = emission4.rateOverTime;
        rate4.constant = 0;
        emission4.rateOverTime = rate4;

        // fireLightParticle emission rate를 0으로 설정
        var emission5 = fireLightParticle.emission;
        var rate5 = emission5.rateOverTime;
        rate5.constant = 0;
        emission5.rateOverTime = rate5;
    }
}
