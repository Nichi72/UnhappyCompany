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
            // 쿠션 체크 되면 다른 충돌 무시        
            bool hasCushion = IsCushionAtCollision(other);
            if (hasCushion)
            {
                Debug.Log("쿠션에 맞아서 처리 안됨");
                return;
            }

            

            Debug.Log("Rampage: Charge 상태에서 충돌 발생 " + other.tag + " " + other.name);
           
            if (other.CompareTag(ETag.Wall.ToString()))
            {
                rampageAIController.SetCollided(true);
                Debug.Log("충돌 발생");
                // 벽 충돌 소리 제거됨 (사용자 요청)

                if (hasCushion == false && rampageAIController.onceReduceHP) 
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

    private bool IsCushionAtCollision(Collider other)
    {
        if (other.CompareTag(ETag.Item.ToString()))
        {
            // GetComponentInParent로 부모 오브젝트까지 검색
            ItemCushion itemCushion = other.GetComponentInParent<ItemCushion>();
            if (itemCushion != null)
            {
                Debug.Log($"[RampageTrigger] 쿠션 감지 성공! {other.gameObject.name}");
                return true;
            }
        }
        return false;
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
