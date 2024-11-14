using UnityEngine;

public class ForTest : MonoBehaviour
{
    public float addforcePower = 100;

    public Transform target;  // 큐브가 이동할 목표 지점
    public float moveForce = 10f;  // 큐브에 가할 힘의 크기
    public float maxVelocity = 5f; // 큐브의 최대 속력
    private Rigidbody rb;
    void Start()
    {
        // Rigidbody 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (target != null)
            {
                MoveTowardsTarget();
            }
        }
      
    }
   
    void MoveTowardsTarget()
    {
        // 목표 지점까지의 방향 계산
        Vector3 direction = (target.position - transform.position).normalized;

        // 현재 속도가 최대 속도를 넘지 않도록 제한
        if (rb.GetPointVelocity(transform.position).sqrMagnitude < maxVelocity * maxVelocity)
        {
            // 큐브에 힘을 가해 이동시키기
            rb.AddForce(direction * moveForce);
        }
    }
}
