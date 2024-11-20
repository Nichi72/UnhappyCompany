using System.Collections;
using UnityEngine;

public class RollingCubeController : MonoBehaviour
{
    public RollingCubeManager pathManager; // 경로 관리자를 참조
    public float moveForce = 10f; // 이동 시 힘의 크기
    public float maxSpeed = 5f; // 최대 속도
    public float rotationForce = 20f; // 회전 효과
    public float chargeForce = 50f; // 돌진 힘
    public float chargeCooldown = 3f; // 돌진 쿨타임

    private Rigidbody rb;
    private bool isCharging = false;
    private float chargeTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (pathManager == null)
            return;

        Vector3 nextWaypoint = pathManager.GetNextWaypoint();
        MoveToWaypoint(nextWaypoint);

        // 돌진 로직
        chargeTimer -= Time.fixedDeltaTime;
        if (chargeTimer <= 0f && !isCharging)
        {
            //StartCoroutine(ChargeTowards(nextWaypoint));
        }
    }

    private void MoveToWaypoint(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;

        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(direction * moveForce, ForceMode.Force);
        }
        else
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed; // 최대 속도로 제한
        }
    }

    private IEnumerator ChargeTowards(Vector3 target)
    {
        isCharging = true;
        Vector3 chargeDirection = (target - transform.position).normalized;

        rb.AddForce(chargeDirection * chargeForce, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * rotationForce);

        yield return new WaitForSeconds(1f); // 돌진 시간
        isCharging = false;
        chargeTimer = chargeCooldown;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collided with {collision.gameObject.name}");
    }
}
