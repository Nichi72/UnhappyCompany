using UnityEngine;
using System.Collections;
using UnhappyCompany.Utility;

public class RampageExplodeState : IState
{
    private RampageAIController controller;
    private bool exploded = false;

    public RampageExplodeState(RampageAIController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        Debug.Log("Rampage: Explode(자폭) 상태 시작");
        DoExplode();
    }

    public void ExecuteMorning()
    {
        // 폭발 후 처리는 Enter에서 모두 완료됨
    }

    public void ExecuteAfternoon()
    {
        
    }

    public void Exit()
    {
        Debug.Log("Rampage: Explode 상태 종료");
    }

    public void ExecuteFixedMorning() { }
    public void ExecuteFixedAfternoon() { }

    private void DoExplode()
    {
        if (exploded)
            return;
            
        exploded = true;
        
        Debug.Log("[RampageExplodeState] 폭발 시작!");
        
        // 1. 폭발 이펙트 재생
        PlayExplodeEffect();
        
        // 2. 폭발 사운드 재생 (있으면)
        PlayExplodeSound();
        
        // 3. 폭발 범위 데미지 처리
        DealExplosionDamage();
        
        // 4. 일정 시간 후 제거 (이펙트가 재생될 시간 확보)
        controller.StartCoroutine(DestroyAfterDelay(0.1f));
    }
    
    /// <summary>
    /// 폭발 이펙트 재생 (EffectSpawnSystem 사용)
    /// </summary>
    private void PlayExplodeEffect()
    {
        if (controller.ExplodeParticle != null)
        {
            Vector3 explodePosition = controller.transform.position + controller.ExplodeEffectOffset;
            
            // 범용 이펙트 스폰 시스템 사용
            GameObject explodeEffect = EffectSpawnSystem.SpawnParticle(
                particlePrefab: controller.ExplodeParticle,
                position: explodePosition,
                rotation: Quaternion.identity,
                parent: null  // 독립적으로 생성
            );
            
            Debug.Log($"[RampageExplodeState] 폭발 이펙트 생성: {explodeEffect?.name}");
        }
        else
        {
            Debug.LogWarning("[RampageExplodeState] 폭발 파티클이 설정되지 않았습니다!");
        }
    }
    
    /// <summary>
    /// 폭발 사운드 재생
    /// </summary>
    private void PlayExplodeSound()
    {
        // TODO: FMODEvents에 폭발 사운드 추가 후 구현
        if (AudioManager.instance != null && FMODEvents.instance != null)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rampageExplode, controller.transform);
        }
    }
    
    /// <summary>
    /// 폭발 범위 데미지 처리 (거리 기반 데미지 감소)
    /// </summary>
    private void DealExplosionDamage()
    {
        Vector3 explosionCenter = controller.transform.position;
        
        // 1. 데미지 범위 처리
        ApplyExplosionDamage(explosionCenter);
        
        // 2. 물리 효과 처리
        ApplyExplosionForce(explosionCenter);
    }
    
    /// <summary>
    /// 거리 기반 데미지 적용
    /// </summary>
    private void ApplyExplosionDamage(Vector3 explosionCenter)
    {
        float damageRadius = controller.EnemyData.explosionDamageRadius;
        int maxDamage = controller.EnemyData.explosionMaxDamage;
        int minDamage = controller.EnemyData.explosionMinDamage;
        
        // 데미지 범위 내의 모든 Collider 탐지
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, damageRadius);
        
        Debug.Log($"[RampageExplodeState] 폭발 데미지 범위({damageRadius}m) 내 {hitColliders.Length}개 오브젝트 탐지");
        
        foreach (Collider hit in hitColliders)
        {
            // IDamageable 컴포넌트 확인
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable == null)
                continue;
            
            // 폭발 중심으로부터의 거리 계산
            float distance = Vector3.Distance(explosionCenter, hit.transform.position);
            
            // 거리 비율 (0 = 중심, 1 = 범위 끝)
            float distanceRatio = Mathf.Clamp01(distance / damageRadius);
            
            // 거리에 따른 데미지 계산 (중심에서 가까울수록 높음)
            int damage = Mathf.RoundToInt(Mathf.Lerp(maxDamage, minDamage, distanceRatio));
            
            // 데미지 적용 (물리 폭발 데미지)
            damageable.TakeDamage(damage, DamageType.Physical);
            
            Debug.Log($"[RampageExplodeState] {hit.name}에게 폭발 데미지 {damage} (거리: {distance:F2}m, 비율: {distanceRatio:P0})");
        }
    }
    
    /// <summary>
    /// 폭발 물리 효과 적용 (AddExplosionForce)
    /// </summary>
    private void ApplyExplosionForce(Vector3 explosionCenter)
    {
        float forceRadius = controller.EnemyData.explosionForceRadius;
        float explosionForce = controller.EnemyData.explosionForce;
        float upwardModifier = controller.EnemyData.explosionUpwardModifier;
        
        // 물리 효과 범위 내의 모든 Collider 탐지
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, forceRadius);
        
        Debug.Log($"[RampageExplodeState] 폭발 물리 범위({forceRadius}m) 내 {hitColliders.Length}개 오브젝트 탐지");
        
        foreach (Collider hit in hitColliders)
        {
            // Rigidbody가 있는 객체만 처리
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null || rb.isKinematic)
                continue;
            
            float distance = Vector3.Distance(explosionCenter, hit.transform.position);
            
            // 플레이어인 경우 특별 처리 (Freeze 해제)
            if (hit.CompareTag(ETag.Player.ToString()))
            {
                ApplyExplosionForceToPlayer(rb, explosionCenter, explosionForce, forceRadius, upwardModifier, distance);
            }
            else
            {
                // 일반 오브젝트는 바로 폭발 힘 적용
                rb.AddExplosionForce(explosionForce, explosionCenter, forceRadius, upwardModifier, ForceMode.Impulse);
                Debug.Log($"[RampageExplodeState] {hit.name}에게 폭발 힘 적용 (거리: {distance:F2}m)");
            }
        }
    }
    
    /// <summary>
    /// 플레이어에게 폭발 힘 적용 (Constraints 일시 해제)
    /// </summary>
    private void ApplyExplosionForceToPlayer(Rigidbody rb, Vector3 explosionCenter, float explosionForce, 
                                              float forceRadius, float upwardModifier, float distance)
    {
        // 원래 Constraint 저장
        RigidbodyConstraints originalConstraints = rb.constraints;
        
        // FirstPersonController 찾기
        var firstPersonController = rb.GetComponent<StarterAssets.FirstPersonController>();
        if (firstPersonController == null)
        {
            Debug.LogWarning("[RampageExplodeState] FirstPersonController를 찾을 수 없습니다.");
        }
        
        // Position Freeze 해제 (넉백 가능), 회전은 X, Z만 고정 (넘어지지 않음)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        
        // 폭발 힘 적용
        rb.AddExplosionForce(explosionForce, explosionCenter, forceRadius, upwardModifier, ForceMode.Impulse);
        
        Debug.Log($"[RampageExplodeState] 플레이어에게 폭발 힘 적용 (거리: {distance:F2}m, Constraints & Controller 일시 해제)");
        
        // 플레이어에게 복구 컴포넌트 추가 (독립적으로 복구 처리)
        RigidbodyConstraintRestore restoreComponent = rb.gameObject.AddComponent<RigidbodyConstraintRestore>();
        restoreComponent.Initialize(rb, originalConstraints, 0.5f, firstPersonController);
    }
    
    /// <summary>
    /// 지연 후 GameObject 제거
    /// </summary>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log("[RampageExplodeState] Rampage 제거");
        GameObject.Destroy(controller.gameObject);
    }
} 