using UnityEngine;
using System.Collections;
using UnhappyCompany.Utility;

public class RampageTrigger : MonoBehaviour
{
    private RampageAIController rampageAIController;
    private float pushStrength = 10f;
    private float damageCooldown = 2f;
    private bool canDamagePlayer = true;

    void Start()
    {
        rampageAIController = GetComponentInParent<RampageAIController>();
        Debug.Log("RampageTrigger 시작");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (rampageAIController.IsInChargeState())
        {
            // 플레이어 충돌 처리 (데미지 & 넉백)
            if(other.CompareTag(ETag.Player.ToString()))
            {
                if (canDamagePlayer)
                {
                    Debug.Log("플레이어와 충돌 발생 - 데미지 & 넉백 적용");
                    
                    // 사운드 재생
                    AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rampageCollisionPlayer, transform, 60f, "Rampage Player Collision");
                    
                    // 데미지 적용
                    other.GetComponent<IDamageable>()?.TakeDamage(rampageAIController.EnemyData.rushDamage, DamageType.Nomal);
                    
                    // 넉백 적용
                    ApplyKnockbackToPlayer(other);
                    
                    StartCoroutine(DamageCooldown());
                    
                    // SetCollided(true)를 호출하지 않아서 계속 돌진함
                }
            }
            // Pushable 오브젝트 충돌 처리
            else if(other.CompareTag(ETag.Pushable.ToString()))
            {
                Push(other);
                Debug.Log("Pushable 충돌 발생");
                AudioManager.instance.Play3DSoundByTransform(FMODEvents.instance.rampageCollisionObject, transform, 60f, "Rampage Object Collision");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (rampageAIController.IsInChargeState())
        {
            // 쿠션 체크 및 충격 이벤트 전달
            ItemCushion cushion = GetCushionFromCollision(other);
            if (cushion != null)
            {
                Debug.Log("쿠션에 맞아서 HP 감소 안됨 - 충격 흡수 연출 시작");
                
                // Phase 3: 쿠션에게 충격 이벤트 전달 (충돌 지점 사용)
                // Rampage의 정면(forward) 방향으로 충돌했다고 가정
                Vector3 contactPoint = other.ClosestPoint(rampageAIController.transform.position);
                cushion.OnImpactWithContact(rampageAIController.transform.position, contactPoint);
                
                // Rampage는 충돌 처리 (쿠션 충돌 플래그 설정)
                rampageAIController.isCushionCollision = true;
                rampageAIController.SetCollided(true);
                return;
            }

            

            Debug.Log("Rampage: Charge 상태에서 충돌 발생 " + other.tag + " " + other.name);
           
            if (other.CompareTag(ETag.Wall.ToString()))
            {
                // 벽 충돌 (쿠션이 아닌 일반 충돌)
                rampageAIController.isCushionCollision = false;
                rampageAIController.SetCollided(true);
                Debug.Log("충돌 발생");

                if (cushion == null && rampageAIController.onceReduceHP) 
                {
                    // HP 감소 전에 현재 HP 비율 확인하여 벽 충돌 사운드 재생
                    PlayWallHitSound();
                    
                    rampageAIController.ReduceHP(rampageAIController.EnemyData.hpLossOnNoCushion);
                    rampageAIController.onceReduceHP = false;
                }
            }
        }
    }

    // OnTriggerExit는 더 이상 사용하지 않음 (플레이어 처리는 OnTriggerEnter로 이동)

    private IEnumerator DamageCooldown()
    {
        canDamagePlayer = false;
        yield return new WaitForSeconds(damageCooldown);
        canDamagePlayer = true;
    }

    /// <summary>
    /// 충돌한 Collider에서 ItemCushion 컴포넌트 가져오기
    /// </summary>
    private ItemCushion GetCushionFromCollision(Collider other)
    {
        if (other.CompareTag(ETag.Item.ToString()))
        {
            // GetComponentInParent로 부모 오브젝트까지 검색
            ItemCushion itemCushion = other.GetComponentInParent<ItemCushion>();
            if (itemCushion != null)
            {
                return itemCushion;
            }
        }
        return null;
    }
    
    /// <summary>
    /// (Deprecated) 쿠션 체크 - GetCushionFromCollision 사용 권장
    /// </summary>
    private bool IsCushionAtCollision(Collider other)
    {
        return GetCushionFromCollision(other) != null;
    }

    private void Push(Collider other)
    {
        Rigidbody otherRb = other.GetComponent<Rigidbody>();

        if (otherRb == null)
        {
            otherRb = other.gameObject.AddComponent<Rigidbody>();
            otherRb.mass = 1f;
            otherRb.linearDamping = 0f;
            otherRb.angularDamping = 0.05f;
            otherRb.interpolation = RigidbodyInterpolation.Interpolate;
            otherRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (otherRb != null && !otherRb.isKinematic)
        {
            Vector3 pushDir = other.transform.position - transform.position;
            pushDir.y = 0;
            pushDir.Normalize();

            otherRb.AddForce(pushDir * pushStrength, ForceMode.Impulse);
        }
    }
    
    /// <summary>
    /// 플레이어에게 넉백 효과 적용 (AddForce 기반)
    /// RampageExplodeState의 폭발 넉백과 달리 방향성 있는 밀어내기
    /// </summary>
    private void ApplyKnockbackToPlayer(Collider playerCollider)
    {
        Rigidbody playerRb = playerCollider.GetComponent<Rigidbody>();
        if (playerRb == null)
        {
            Debug.LogWarning("[RampageTrigger] 플레이어에게 Rigidbody가 없습니다!");
            return;
        }
        
        // FirstPersonController 찾기
        var firstPersonController = playerRb.GetComponent<StarterAssets.FirstPersonController>();
        if (firstPersonController == null)
        {
            Debug.LogWarning("[RampageTrigger] FirstPersonController를 찾을 수 없습니다.");
        }
        
        // 원래 Constraints 저장
        RigidbodyConstraints originalConstraints = playerRb.constraints;
        
        // Position Freeze 해제 (넉백 가능), 회전은 모두 고정 (넘어지지 않음)
        playerRb.constraints = RigidbodyConstraints.FreezeRotationX | 
                                RigidbodyConstraints.FreezeRotationY | 
                                RigidbodyConstraints.FreezeRotationZ;
        
        // 넉백 방향 계산 (Rampage -> Player)
        Vector3 knockbackDirection = (playerCollider.transform.position - transform.position).normalized;
        
        // RampageAIData에서 넉백 파라미터 가져오기
        float knockbackForce = rampageAIController.EnemyData.playerKnockbackForce;
        float knockbackUpward = rampageAIController.EnemyData.playerKnockbackUpwardForce;
        float restoreDelay = rampageAIController.EnemyData.knockbackRestoreDelay;
        
        // 수평 방향 + 약간의 위쪽 힘
        Vector3 knockbackForceVector = new Vector3(
            knockbackDirection.x * knockbackForce,
            knockbackUpward,
            knockbackDirection.z * knockbackForce
        );
        
        // AddForce로 넉백 적용 (Impulse 모드)
        playerRb.AddForce(knockbackForceVector, ForceMode.Impulse);
        
        Debug.Log($"[RampageTrigger] 플레이어 넉백 적용 - 방향: {knockbackDirection}, 힘: {knockbackForce}");
        
        // 플레이어에게 복구 컴포넌트 추가 (독립적으로 복구 처리)
        RigidbodyConstraintRestore restoreComponent = playerRb.gameObject.AddComponent<RigidbodyConstraintRestore>();
        restoreComponent.Initialize(playerRb, originalConstraints, restoreDelay, firstPersonController);
    }
    
    /// <summary>
    /// 벽 충돌 시 HP에 따라 다른 사운드 재생
    /// </summary>
    private void PlayWallHitSound()
    {
        if (AudioManager.instance == null || FMODEvents.instance == null)
            return;
        
        // 현재 HP 비율 계산
        float hpPercent = (float)rampageAIController.hp / rampageAIController.EnemyData.maxHP;
        
        FMODUnity.EventReference soundToPlay;
        string debugMessage;
        
        if (hpPercent > 0.66f)
        {
            // Level 1: HP 많음 (66% 이상)
            soundToPlay = FMODEvents.instance.rampageWallHitLevel1;
            debugMessage = "벽 충돌 Level 1 (HP 많음)";
        }
        else if (hpPercent > 0.33f)
        {
            // Level 2: HP 중간 (33% ~ 66%)
            soundToPlay = FMODEvents.instance.rampageWallHitLevel2;
            debugMessage = "벽 충돌 Level 2 (HP 중간)";
        }
        else
        {
            // Level 3: HP 적음 (33% 미만)
            soundToPlay = FMODEvents.instance.rampageWallHitLevel3;
            debugMessage = "벽 충돌 Level 3 (HP 위험!)";
        }
        
        // 사운드 재생 (벽 충돌은 큰 소리이므로 50m)
        if (!soundToPlay.IsNull)
        {
            AudioManager.instance.Play3DSoundByTransform(soundToPlay, transform, 80f, debugMessage);
            Debug.Log($"[Rampage Wall Hit Sound] {debugMessage} - HP: {rampageAIController.hp}/{rampageAIController.EnemyData.maxHP} ({hpPercent:P0})");
        }
    }
}

