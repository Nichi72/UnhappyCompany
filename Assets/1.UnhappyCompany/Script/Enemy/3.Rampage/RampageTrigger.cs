using UnityEngine;
using System.Collections;

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
            if(other.CompareTag(ETag.Pushable.ToString()))
            {
                Push(other);
                Debug.Log("Pushable 충돌 발생");
                AudioManager.instance.PlayOneShot(FMODEvents.instance.rampageCollisionObject, transform, "Pushable 충돌하는 소리");
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
                
                // Phase 3: 쿠션에게 충격 이벤트 전달
                cushion.OnImpact(rampageAIController.transform.position);
                
                // Rampage는 충돌 처리
                rampageAIController.SetCollided(true);
                return;
            }

            

            Debug.Log("Rampage: Charge 상태에서 충돌 발생 " + other.tag + " " + other.name);
           
            if (other.CompareTag(ETag.Wall.ToString()))
            {
                rampageAIController.SetCollided(true);
                Debug.Log("충돌 발생");
                // 벽 충돌 소리 제거됨 (사용자 요청)

                if (cushion == null && rampageAIController.onceReduceHP) 
                {
                    rampageAIController.ReduceHP(rampageAIController.EnemyData.hpLossOnNoCushion);
                    rampageAIController.onceReduceHP = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag(ETag.Player.ToString()))
        {
            if (canDamagePlayer)
            {
                Debug.Log("플레이어와 충돌 발생");
                rampageAIController.SetCollided(true);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.rampageCollisionPlayer, transform, "Rampage 플레이어와 충돌 소리");
                other.GetComponent<IDamageable>()?.TakeDamage(rampageAIController.EnemyData.rushDamage, DamageType.Nomal);
                StartCoroutine(DamageCooldown());
            }
        }
    }

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
}

